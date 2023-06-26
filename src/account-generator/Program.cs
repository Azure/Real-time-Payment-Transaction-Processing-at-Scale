using Bogus;
using CorePayments.Infrastructure;
using CorePayments.Infrastructure.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;
using System.Threading;

namespace account_generator
{
    public partial class Program
    {
        static CosmosClient cosmosClient;
        static Container transactionsContainer;
        private static Container membersContainer;
        static volatile CancellationTokenSource cancellationTokenSource;
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
                .AddJsonFile("local.settings.json")
                .AddJsonFile("settings.json", optional: true, reloadOnChange: true)
                .AddCommandLine(args, new Dictionary<string, string>
                {
                    {"-m", $"{nameof(GeneratorOptions)}:{nameof(GeneratorOptions.RunMode)}"},
                    {"-s", $"{nameof(GeneratorOptions)}:{nameof(GeneratorOptions.SleepTime)}"},
                    {"-c", $"{nameof(GeneratorOptions)}:{nameof(GeneratorOptions.BatchSize)}"},
                    {"-v", $"{nameof(GeneratorOptions)}:{nameof(GeneratorOptions.Verbose)}"}
                })
                .AddEnvironmentVariables()
                .Build();

            GeneratorOptions options = new();
            configuration.GetSection(nameof(GeneratorOptions))
                .Bind(options);

            Console.WriteLine("To STOP press CTRL+C...");

            Console.CancelKeyPress += Console_CancelKeyPressHandler;

            cosmosClient = new CosmosClient(configuration["CosmosDbConnectionString"],
                new CosmosClientOptions() { AllowBulkExecution = true, EnableContentResponseOnWrite = false });

            transactionsContainer = cosmosClient.GetContainer("payments", "transactions");
            membersContainer = cosmosClient.GetContainer("payments", "members");
            
            // Generate Members if they don't already exist:
            await CreateMembersAsync();

            var tasks = new List<Task>();

            try
            {
                for (var i = 1; i <= 5; i++)
                {
                    tasks.Add(LoadAsync(i, options));
                }

                Task.WhenAll(tasks).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Completed generating data.");
            cosmosClient.Dispose();
        }

        static void Console_CancelKeyPressHandler(object? sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Stopping...");
            cancellationTokenSource.Cancel();
            e.Cancel = true;
        }

        static async Task CreateMembersAsync()
        {
            // If there are already member records in the Members container, exit.
            var query = "SELECT VALUE COUNT(1) FROM c";
            var queryDefinition = new QueryDefinition(query);
            var count = await membersContainer.GetItemQueryIterator<int>(queryDefinition).ReadNextAsync();

            if (count.Resource.FirstOrDefault() > 0)
            {
                Console.WriteLine("Skipping Member record generation since records already exist.");
                return;
            }

            for (var i = 0; i <= 10; i++)
            {
                var memberId = Guid.NewGuid().ToString();
                var memberFaker = new Faker<Member>()
                    .RuleFor(u => u.memberId, (f, u) => memberId)
                    .RuleFor(u => u.firstName, (f, u) => f.Name.FirstName())
                    .RuleFor(u => u.lastName, (f, u) => f.Name.LastName())
                    .RuleFor(u => u.email, (f, u) => f.Internet.Email())
                    .RuleFor(u => u.phone, (f, u) => f.Phone.PhoneNumber())
                    .RuleFor(u => u.address, (f, u) => f.Address.StreetAddress())
                    .RuleFor(u => u.city, (f, u) => f.Address.City())
                    .RuleFor(u => u.state, (f, u) => f.Address.State())
                    .RuleFor(u => u.zipcode, (f, u) => f.Address.ZipCode("#####"))
                    .RuleFor(u => u.country, (f, u) => "USA")
                    .RuleFor(u => u.type, (f, u) => Constants.DocumentTypes.Member);

                await _pollyRetryPolicy.ExecuteAsync(async () =>
                {
                    await membersContainer.CreateItemAsync(memberFaker.Generate(), new PartitionKey(memberId));
                });
            }
            Console.WriteLine("Finished generating Members.");
        }

        static async Task LoadAsync(int batchNum, GeneratorOptions options)
        {
            cancellationTokenSource = new CancellationTokenSource();
            var tasks = new List<Task>();
            
            try
            {
                while (true)
                {
                    var totalTasks = 0;
                    for (var i = (1 + ((batchNum - 1) * 10000000)); i <= (batchNum * 10000000); i++)
                    {
                        cancellationTokenSource.Token.ThrowIfCancellationRequested();

                        var accountId = i.ToString().PadLeft(9, '0');

                        var orderFaker = new Faker<AccountSummary>()
                            .RuleFor(u => u.id, (f, u) => accountId)
                            .RuleFor(u => u.customerGreetingName, (f, u) => f.Name.FirstName())
                            .RuleFor(u => u.balance, (f, u) => Convert.ToDouble(f.Finance.Amount(-1000, 50000, 2)))
                            .RuleFor(u => u.accountType, (f, u) => f.PickRandom(accountType))
                            .RuleFor(u => u.type, (f, u) => Constants.DocumentTypes.AccountSummary)
                            .RuleFor(u => u.overdraftLimit, (f, u) => 5000)
                            .RuleFor(u => u.memberSince, (f, u) => f.Date.Past(20));

                        tasks.Add(
                            _pollyRetryPolicy.ExecuteAsync(async () =>
                                {
                                    await transactionsContainer.UpsertItemAsync(orderFaker.Generate(), new PartitionKey(accountId));
                                }
                            ).ContinueWith(t =>
                            {
                                if (t.IsFaulted)
                                {
                                    Console.WriteLine($"Error occurred while upserting item: {t.Exception}");
                                }
                            }, cancellationTokenSource.Token)
                        );

                        if (tasks.Count == 100)
                        {
                            await Task.WhenAll(tasks);
                            totalTasks += tasks.Count;
                            if (options.Verbose)
                            {
                                Console.WriteLine($"{totalTasks} account summary items written in batch #{batchNum}");
                            }
                            tasks.Clear();
                        }

                        if (options.RunMode == GeneratorOptions.RunModeOption.OneTime && totalTasks >= options.BatchSize)
                        {
                            await Task.WhenAll(tasks);
                            return;
                        }
                    }

                    await Task.WhenAll(tasks);
                    await Task.Delay(Math.Max(1, options.SleepTime), cancellationTokenSource.Token);
                    tasks.Clear();

                    if (options.RunMode == GeneratorOptions.RunModeOption.OneTime)
                    {
                        return;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation canceled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sending message failed: {ex.Message}");
            }
        }
    }
}