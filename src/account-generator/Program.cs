using Bogus;
using Microsoft.Azure.Cosmos;
using payments_model;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;

namespace account_generator
{
    public partial class Program
    {
        static CosmosClient cosmosClient;
        static Container container;
        static volatile bool cancel = false;
        static List<string> accountType = new List<string>() { "checking", "savings" };
        private static readonly AsyncRetryPolicy _pollyRetryPolicy = Policy
            .Handle<CosmosException>(e => e.RetryAfter > TimeSpan.Zero)
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: (i, e, ctx) =>
                {
                    var dce = (CosmosException)e;
                    return (TimeSpan)(dce.RetryAfter ?? TimeSpan.FromSeconds(2));
                },
                onRetryAsync: (e, ts, i, ctx) => Task.CompletedTask
            );

        public static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }
        
        public static async Task MainAsync(string[] args)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json").Build();

            Console.WriteLine("To STOP press CTRL+C...");

            Console.CancelKeyPress += Console_CancelKeyPress1;

            cosmosClient = new CosmosClient(configuration["CosmosDbConnectionString"],
                new CosmosClientOptions() { AllowBulkExecution = true, EnableContentResponseOnWrite = false });

            container = cosmosClient.GetContainer("payments", "transactions");

            var tasks = new List<Task>();

            try
            {
                for (int i = 1; i <= 5; i++)
                {
                    tasks.Add(LoadAsync(i));
                }

                Task.WhenAll(tasks).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Stopped!");
        }

        static void Console_CancelKeyPress1(object? sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Stopping...");
            cancel = e.Cancel = true;
        }

        static async Task LoadAsync(int batchNum)
        {
            var tasks = new List<Task>();

            try
            {
                while (!cancel)
                {
                    var totalTasks = 0;
                    for (int i = (1 + ((batchNum - 1) * 10000000)); i <= (batchNum * 10000000); i++)
                    //for (int i = (1 + ((batchNum - 1) * 1)); i <= (batchNum * 1); i++)
                    {
                        if (cancel)
                            break;

                        var accountId = i.ToString().PadLeft(9, '0');

                        var orderFaker = new Faker<AccountSummary>()
                            .RuleFor(u => u.id, (f, u) => accountId)
                            .RuleFor(u => u.customerGreetingName, (f, u) => f.Name.FirstName())
                            .RuleFor(u => u.balance, (f, u) => Convert.ToDouble(f.Finance.Amount(-1000, 50000, 2)))
                            .RuleFor(u => u.accountType, (f, u) => f.PickRandom(accountType))
                            .RuleFor(u => u.type, (f, u) => "accountSummary")
                            .RuleFor(u => u.overdraftLimit, (f, u) => 5000)
                            .RuleFor(u => u.memberSince, (f, u) => f.Date.Past(20));

                        tasks.Add(
                            _pollyRetryPolicy.ExecuteAsync(async () =>
                                {
                                    await container.UpsertItemAsync(orderFaker.Generate(), new PartitionKey(accountId));
                                }
                            )
                        );

                        if (tasks.Count == 100)
                        {
                            await Task.WhenAll(tasks);
                            totalTasks += tasks.Count;
                            Console.WriteLine($"{totalTasks} account summary items written in batch #{batchNum}");
                            tasks.Clear();
                        }
                    }

                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sending message failed: {ex.Message}");
            }
            finally
            {
                cosmosClient.Dispose();
            }
        }
    }
}