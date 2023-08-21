using Bogus;
using CorePayments.Infrastructure;
using CorePayments.Infrastructure.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;
using System.Threading;
using Bogus.Distributions.Gaussian;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;
using System.Transactions;

namespace account_generator
{
    public partial class Program
    {
        static CosmosClient cosmosClient;
        static Container transactionsContainer;
        private static Container membersContainer;
        private static Container globalIndexContainer;
        static volatile CancellationTokenSource cancellationTokenSource;
        static List<string> accountType = new() {"checking", "savings"};
        private static List<BankTransaction> bankTransactions;

        public class BankTransaction
        {
            public string Description { get; set; }
            public TransactionType Type { get; set; }
            public string CompanyName { get; set; }
        }

        public enum TransactionType
        {
            Positive,
            Negative
        }

        private static Random random;

        private static readonly AsyncRetryPolicy _pollyRetryPolicy = Policy
            .Handle<CosmosException>(e => e.RetryAfter > TimeSpan.Zero)
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: (i, e, ctx) =>
                {
                    var dce = (CosmosException) e;
                    return (TimeSpan) (dce.RetryAfter ?? TimeSpan.FromSeconds(2));
                },
                onRetryAsync: (e, ts, i, ctx) => Task.CompletedTask
            );

        public static void Main(string[] args)
        {
            random = new Random();
            bankTransactions = GenerateBankTransactions();
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
                new CosmosClientOptions() {AllowBulkExecution = true, EnableContentResponseOnWrite = false});

            transactionsContainer = cosmosClient.GetContainer("payments", "transactions");
            membersContainer = cosmosClient.GetContainer("payments", "members");
            globalIndexContainer = cosmosClient.GetContainer("payments", "globalIndex");

            // Generate Members if they don't already exist:
            var memberList = await CreateMembersAsync();

            var tasks = new List<Task>();

            try
            {
                for (var i = 1; i <= 5; i++)
                {
                    tasks.Add(LoadAsync(i, options, memberList));
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

        private static void Console_CancelKeyPressHandler(object? sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Stopping...");
            cancellationTokenSource.Cancel();
            e.Cancel = true;
        }

        private static Member GetRandomMember(IReadOnlyList<Member> memberList)
        {
            var randomMember = memberList[random.Next(0, memberList.Count)];
            return randomMember;
        }

        private static async Task<List<Member>> CreateMembersAsync()
        {
            var memberList = new List<Member>();
            var query = "SELECT * FROM c";
            var queryDefinition = new QueryDefinition(query);
            var resultIterator = membersContainer.GetItemQueryIterator<Member>(queryDefinition);

            while (resultIterator.HasMoreResults)
            {
                var response = await resultIterator.ReadNextAsync();
                memberList.AddRange(response);
            }

            if (memberList.Count > 0)
            {
                Console.WriteLine("Skipping Member record generation since records already exist.");
                return memberList;
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
                    .RuleFor(u => u.type, (f, u) => Constants.DocumentTypes.Member)
                    .RuleFor(u => u.memberSince, (f, u) => f.Date.Past(20));

                await _pollyRetryPolicy.ExecuteAsync(async () =>
                {
                    var member = memberFaker.Generate();
                    await membersContainer.CreateItemAsync(member, new PartitionKey(memberId));
                    memberList.Add(member);
                    // Create a global index lookup for this member.
                    var globalIndex = new GlobalIndex
                    {
                        partitionKey = memberId,
                        targetDocType = nameof(Member),
                        id = memberId
                    };
                    await globalIndexContainer.CreateItemAsync(globalIndex, new PartitionKey(globalIndex.partitionKey));
                });
            }

            Console.WriteLine("Finished generating Members.");
            return memberList;
        }

        private static async Task LoadAsync(int batchNum, GeneratorOptions options, IReadOnlyList<Member> memberList)
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

                        var member = GetRandomMember(memberList);

                        var accountId = i.ToString().PadLeft(9, '0');

                        var orderFaker = new Faker<AccountSummary>()
                            .RuleFor(u => u.id, (f, u) => accountId)
                            .RuleFor(u => u.customerGreetingName, (f, u) => member.firstName)
                            .RuleFor(u => u.balance, (f, u) => Convert.ToDouble(f.Finance.Amount(-1000, 50000, 2)))
                            .RuleFor(u => u.accountType, (f, u) => f.PickRandom(accountType))
                            .RuleFor(u => u.type, (f, u) => Constants.DocumentTypes.AccountSummary)
                            .RuleFor(u => u.overdraftLimit, (f, u) => 5000)
                            .RuleFor(u => u.memberSince, (f, u) => member.memberSince);

                        var accountSummary = orderFaker.Generate();
                        var memberSinceDate = accountSummary.memberSince.Date;
                        var currentDate = DateTime.UtcNow;
                        var isNegative = false;
                        var positiveBankTransaction = GetRandomBankTransaction(TransactionType.Positive);
                        var negativeBankTransaction = GetRandomBankTransaction(TransactionType.Negative);

                        var transactionFaker = new Faker<CorePayments.Infrastructure.Domain.Entities.Transaction>()
                            .RuleFor(u => u.id, (f, u) => f.Random.Guid().ToString())
                            .RuleFor(u => u.accountId, (f, u) => accountId)
                            .RuleFor(u => u.amount, (f, u) =>
                            {
                                isNegative = f.Random.Bool(0.8f); // 80% chance of being negative
                                var minAmount = isNegative ? -5000 : 5; // adjust the minimum value based on negativity
                                var maxAmount = isNegative ? -5 : 5000; // adjust the maximum value based on negativity
                                return Convert.ToDouble(f.Finance.Amount(minAmount, maxAmount, 2));
                            })
                            .RuleFor(u => u.type, (f, u) => isNegative ? "Debit" : "Credit")
                            .RuleFor(u => u.description, (f, u) => isNegative ? negativeBankTransaction.Description : positiveBankTransaction.Description)
                            .RuleFor(u => u.merchant, (f, u) => isNegative ? negativeBankTransaction.CompanyName : positiveBankTransaction.CompanyName)
                            .RuleFor(u => u.timestamp, (f, u) => f.Date.Between(memberSinceDate, currentDate));

                        var transactions = transactionFaker.GenerateBetween(4, 8);

                        tasks.Add(
                            _pollyRetryPolicy.ExecuteAsync(async () =>
                                {
                                    var account = await transactionsContainer.UpsertItemAsync(accountSummary,
                                        new PartitionKey(accountId));

                                    // Create a global index entry to associate the member with the account.
                                    var globalIndexMemberAccount = new GlobalIndex
                                    {
                                        partitionKey = member.id,
                                        targetDocType = nameof(AccountSummary),
                                        id = accountId
                                    };
                                    await globalIndexContainer.CreateItemAsync(globalIndexMemberAccount,
                                        new PartitionKey(globalIndexMemberAccount.partitionKey));

                                    // Create a global index entry to associate the account with the member.
                                    var globalIndexAccountMember = new GlobalIndex
                                    {
                                        partitionKey = accountId,
                                        targetDocType = nameof(Member),
                                        id = member.id
                                    };
                                    await globalIndexContainer.CreateItemAsync(globalIndexAccountMember,
                                        new PartitionKey(globalIndexAccountMember.partitionKey));

                                    foreach (var transaction in transactions)
                                    {
                                        await transactionsContainer.UpsertItemAsync(transaction,
                                            new PartitionKey(accountId));
                                    }
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

                        if (options.RunMode == GeneratorOptions.RunModeOption.OneTime &&
                            totalTasks >= options.BatchSize)
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

        private static BankTransaction GetRandomBankTransaction(TransactionType transactionType)
        {
            var filteredTransactions = bankTransactions.Where(t => t.Type == transactionType).ToList();

            var index = random.Next(filteredTransactions.Count);
            return filteredTransactions[index];
        }

        private static List<BankTransaction> GenerateBankTransactions()
        {
            var transactions = new List<BankTransaction>();

            // Sample transactions for Adatum Corporation
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Salary deposit from Adatum Corporation.", Type = TransactionType.Positive,
                    CompanyName = "Adatum Corporation"
                },
                new BankTransaction
                {
                    Description = "Refund for software license from Adatum Corporation.",
                    Type = TransactionType.Positive, CompanyName = "Adatum Corporation"
                },
                new BankTransaction
                {
                    Description = "Software purchase from Adatum Corporation.", Type = TransactionType.Negative,
                    CompanyName = "Adatum Corporation"
                },
                new BankTransaction
                {
                    Description = "Subscription renewal with Adatum Corporation.", Type = TransactionType.Negative,
                    CompanyName = "Adatum Corporation"
                },
                new BankTransaction
                {
                    Description = "Workshop registration with Adatum Corporation.", Type = TransactionType.Negative,
                    CompanyName = "Adatum Corporation"
                },
                new BankTransaction
                {
                    Description = "Software service fee to Adatum Corporation.", Type = TransactionType.Negative,
                    CompanyName = "Adatum Corporation"
                },
            });

            // Sample transactions for Adventure Works Cycles
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Bicycle purchase from Adventure Works Cycles.", Type = TransactionType.Negative,
                    CompanyName = "Adventure Works Cycles"
                },
                new BankTransaction
                {
                    Description = "Refund for bike accessories from Adventure Works Cycles.",
                    Type = TransactionType.Positive, CompanyName = "Adventure Works Cycles"
                },
                new BankTransaction
                {
                    Description = "Service fee for bicycle repair at Adventure Works Cycles.",
                    Type = TransactionType.Negative, CompanyName = "Adventure Works Cycles"
                },
                new BankTransaction
                {
                    Description = "Bike helmet purchase from Adventure Works Cycles.", Type = TransactionType.Negative,
                    CompanyName = "Adventure Works Cycles"
                },
                new BankTransaction
                {
                    Description = "Monthly membership fee at Adventure Works Cycles.", Type = TransactionType.Negative,
                    CompanyName = "Adventure Works Cycles"
                },
                new BankTransaction
                {
                    Description = "Refund for duplicate charge at Adventure Works Cycles.",
                    Type = TransactionType.Positive, CompanyName = "Adventure Works Cycles"
                },
            });

            // Sample transactions for Alpine Ski House
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Skiing trip booking at Alpine Ski House.", Type = TransactionType.Negative,
                    CompanyName = "Alpine Ski House"
                },
                new BankTransaction
                {
                    Description = "Refund for winter gear from Alpine Ski House.", Type = TransactionType.Positive,
                    CompanyName = "Alpine Ski House"
                },
                new BankTransaction
                {
                    Description = "Winter jacket purchase from Alpine Ski House.", Type = TransactionType.Negative,
                    CompanyName = "Alpine Ski House"
                },
                new BankTransaction
                {
                    Description = "Reservation deposit for Alpine Ski House resort.", Type = TransactionType.Negative,
                    CompanyName = "Alpine Ski House"
                },
                new BankTransaction
                {
                    Description = "Ski pass renewal with Alpine Ski House.", Type = TransactionType.Negative,
                    CompanyName = "Alpine Ski House"
                },
                new BankTransaction
                {
                    Description = "Refund for cancelled skiing lesson at Alpine Ski House.",
                    Type = TransactionType.Positive, CompanyName = "Alpine Ski House"
                },
            });

            // Sample transactions for Bellows College
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Tuition fee payment to Bellows College.", Type = TransactionType.Negative,
                    CompanyName = "Bellows College"
                },
                new BankTransaction
                {
                    Description = "Refund for overpayment at Bellows College.", Type = TransactionType.Positive,
                    CompanyName = "Bellows College"
                },
                new BankTransaction
                {
                    Description = "Hostel fee payment to Bellows College.", Type = TransactionType.Negative,
                    CompanyName = "Bellows College"
                },
                new BankTransaction
                {
                    Description = "Library fine paid at Bellows College.", Type = TransactionType.Negative,
                    CompanyName = "Bellows College"
                },
                new BankTransaction
                {
                    Description = "Donation made to Bellows College alumni fund.", Type = TransactionType.Negative,
                    CompanyName = "Bellows College"
                },
                new BankTransaction
                {
                    Description = "Refund for unattended workshop at Bellows College.", Type = TransactionType.Positive,
                    CompanyName = "Bellows College"
                },
            });

            // Sample transactions for Best For You Organics Company
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Organic vegetable purchase from Best For You Organics Company.",
                    Type = TransactionType.Negative, CompanyName = "Best For You Organics Company"
                },
                new BankTransaction
                {
                    Description = "Refund for damaged goods from Best For You Organics Company.",
                    Type = TransactionType.Positive, CompanyName = "Best For You Organics Company"
                },
                new BankTransaction
                {
                    Description = "Membership fee for Best For You Organics Club.", Type = TransactionType.Negative,
                    CompanyName = "Best For You Organics Company"
                },
                new BankTransaction
                {
                    Description = "Organic cosmetics purchase from Best For You Organics Company.",
                    Type = TransactionType.Negative, CompanyName = "Best For You Organics Company"
                },
                new BankTransaction
                {
                    Description = "Gift card reload for Best For You Organics Company.",
                    Type = TransactionType.Negative, CompanyName = "Best For You Organics Company"
                },
                new BankTransaction
                {
                    Description = "Refund for wrong delivery by Best For You Organics Company.",
                    Type = TransactionType.Positive, CompanyName = "Best For You Organics Company"
                },
            });

            // Sample transactions for Contoso, Ltd.
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Tech gadget purchase from Contoso, Ltd.", Type = TransactionType.Negative,
                    CompanyName = "Contoso, Ltd."
                },
                new BankTransaction
                {
                    Description = "Refund for returned item to Contoso, Ltd.", Type = TransactionType.Positive,
                    CompanyName = "Contoso, Ltd."
                },
                new BankTransaction
                {
                    Description = "Software license purchase from Contoso, Ltd.", Type = TransactionType.Negative,
                    CompanyName = "Contoso, Ltd."
                },
                new BankTransaction
                {
                    Description = "Monthly subscription fee for Contoso services.", Type = TransactionType.Negative,
                    CompanyName = "Contoso, Ltd."
                },
                new BankTransaction
                {
                    Description = "Warranty extension payment to Contoso, Ltd.", Type = TransactionType.Negative,
                    CompanyName = "Contoso, Ltd."
                },
                new BankTransaction
                {
                    Description = "Refund for double billing by Contoso, Ltd.", Type = TransactionType.Positive,
                    CompanyName = "Contoso, Ltd."
                },
            });

            // Sample transactions for Contoso Pharmaceuticals
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Medication purchase from Contoso Pharmaceuticals.", Type = TransactionType.Negative,
                    CompanyName = "Contoso Pharmaceuticals"
                },
                new BankTransaction
                {
                    Description = "Refund for mistaken prescription at Contoso Pharmaceuticals.",
                    Type = TransactionType.Positive, CompanyName = "Contoso Pharmaceuticals"
                },
                new BankTransaction
                {
                    Description = "Vitamin supplements order from Contoso Pharmaceuticals.",
                    Type = TransactionType.Negative, CompanyName = "Contoso Pharmaceuticals"
                },
                new BankTransaction
                {
                    Description = "Health insurance claim by Contoso Pharmaceuticals.", Type = TransactionType.Positive,
                    CompanyName = "Contoso Pharmaceuticals"
                },
                new BankTransaction
                {
                    Description = "Annual flu shot at Contoso Pharmaceuticals.", Type = TransactionType.Negative,
                    CompanyName = "Contoso Pharmaceuticals"
                },
                new BankTransaction
                {
                    Description = "Refund for overcharge at Contoso Pharmaceuticals.", Type = TransactionType.Positive,
                    CompanyName = "Contoso Pharmaceuticals"
                },
            });

            // Sample transactions for Contoso Suites
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Room booking at Contoso Suites.", Type = TransactionType.Negative,
                    CompanyName = "Contoso Suites"
                },
                new BankTransaction
                {
                    Description = "Refund for canceled booking at Contoso Suites.", Type = TransactionType.Positive,
                    CompanyName = "Contoso Suites"
                },
                new BankTransaction
                {
                    Description = "Catering services bill from Contoso Suites.", Type = TransactionType.Negative,
                    CompanyName = "Contoso Suites"
                },
                new BankTransaction
                {
                    Description = "Spa services at Contoso Suites.", Type = TransactionType.Negative,
                    CompanyName = "Contoso Suites"
                },
                new BankTransaction
                {
                    Description = "Annual membership fee for Contoso Suites Club.", Type = TransactionType.Negative,
                    CompanyName = "Contoso Suites"
                },
                new BankTransaction
                {
                    Description = "Refund for erroneous minibar charges at Contoso Suites.",
                    Type = TransactionType.Positive, CompanyName = "Contoso Suites"
                },
            });

            // Sample transactions for Consolidated Messenger
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Parcel delivery service from Consolidated Messenger.",
                    Type = TransactionType.Negative, CompanyName = "Consolidated Messenger"
                },
                new BankTransaction
                {
                    Description = "Refund for late delivery by Consolidated Messenger.",
                    Type = TransactionType.Positive, CompanyName = "Consolidated Messenger"
                },
                new BankTransaction
                {
                    Description = "Bulk mailing service fee to Consolidated Messenger.",
                    Type = TransactionType.Negative, CompanyName = "Consolidated Messenger"
                },
                new BankTransaction
                {
                    Description = "Annual subscription for courier services at Consolidated Messenger.",
                    Type = TransactionType.Negative, CompanyName = "Consolidated Messenger"
                },
                new BankTransaction
                {
                    Description = "Package insurance fee with Consolidated Messenger.", Type = TransactionType.Negative,
                    CompanyName = "Consolidated Messenger"
                },
                new BankTransaction
                {
                    Description = "Refund for lost parcel by Consolidated Messenger.", Type = TransactionType.Positive,
                    CompanyName = "Consolidated Messenger"
                },
            });

            // Sample transactions for Fabrikam, Inc.
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Software purchase from Fabrikam, Inc.", Type = TransactionType.Negative,
                    CompanyName = "Fabrikam, Inc."
                },
                new BankTransaction
                {
                    Description = "Refund for software glitch by Fabrikam, Inc.", Type = TransactionType.Positive,
                    CompanyName = "Fabrikam, Inc."
                },
                new BankTransaction
                {
                    Description = "Cloud storage subscription at Fabrikam, Inc.", Type = TransactionType.Negative,
                    CompanyName = "Fabrikam, Inc."
                },
                new BankTransaction
                {
                    Description = "Online workshop registration with Fabrikam, Inc.", Type = TransactionType.Negative,
                    CompanyName = "Fabrikam, Inc."
                },
                new BankTransaction
                {
                    Description = "Annual software maintenance fee to Fabrikam, Inc.", Type = TransactionType.Negative,
                    CompanyName = "Fabrikam, Inc."
                },
                new BankTransaction
                {
                    Description = "Refund for duplicate billing by Fabrikam, Inc.", Type = TransactionType.Positive,
                    CompanyName = "Fabrikam, Inc."
                },
            });

            // Sample transactions for Fabrikam Residences
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Booking deposit for Fabrikam Residences.", Type = TransactionType.Negative,
                    CompanyName = "Fabrikam Residences"
                },
                new BankTransaction
                {
                    Description = "Refund for booking cancellation at Fabrikam Residences.",
                    Type = TransactionType.Positive, CompanyName = "Fabrikam Residences"
                },
                new BankTransaction
                {
                    Description = "Monthly maintenance fee for Fabrikam Residences property.",
                    Type = TransactionType.Negative, CompanyName = "Fabrikam Residences"
                },
                new BankTransaction
                {
                    Description = "Facility usage charge at Fabrikam Residences.", Type = TransactionType.Negative,
                    CompanyName = "Fabrikam Residences"
                },
                new BankTransaction
                {
                    Description = "Property tax for Fabrikam Residences unit.", Type = TransactionType.Negative,
                    CompanyName = "Fabrikam Residences"
                },
                new BankTransaction
                {
                    Description = "Refund for overpayment on Fabrikam Residences services.",
                    Type = TransactionType.Positive, CompanyName = "Fabrikam Residences"
                },
            });

            // Sample transactions for Fincher Architects
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Consultation fee for Fincher Architects.", Type = TransactionType.Negative,
                    CompanyName = "Fincher Architects"
                },
                new BankTransaction
                {
                    Description = "Refund for project cancellation with Fincher Architects.",
                    Type = TransactionType.Positive, CompanyName = "Fincher Architects"
                },
                new BankTransaction
                {
                    Description = "Blueprint drafting fee by Fincher Architects.", Type = TransactionType.Negative,
                    CompanyName = "Fincher Architects"
                },
                new BankTransaction
                {
                    Description = "Model creation fee with Fincher Architects.", Type = TransactionType.Negative,
                    CompanyName = "Fincher Architects"
                },
                new BankTransaction
                {
                    Description = "Annual subscription for architectural insights from Fincher Architects.",
                    Type = TransactionType.Negative, CompanyName = "Fincher Architects"
                },
                new BankTransaction
                {
                    Description = "Refund for design changes by Fincher Architects.", Type = TransactionType.Positive,
                    CompanyName = "Fincher Architects"
                },
            });

            // Sample transactions for First Up Consultants
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Business strategy session with First Up Consultants.",
                    Type = TransactionType.Negative, CompanyName = "First Up Consultants"
                },
                new BankTransaction
                {
                    Description = "Refund for overcharge by First Up Consultants.", Type = TransactionType.Positive,
                    CompanyName = "First Up Consultants"
                },
                new BankTransaction
                {
                    Description = "Digital transformation workshop at First Up Consultants.",
                    Type = TransactionType.Negative, CompanyName = "First Up Consultants"
                },
                new BankTransaction
                {
                    Description = "Quarterly business review fee with First Up Consultants.",
                    Type = TransactionType.Negative, CompanyName = "First Up Consultants"
                },
                new BankTransaction
                {
                    Description = "Annual consulting service subscription with First Up Consultants.",
                    Type = TransactionType.Negative, CompanyName = "First Up Consultants"
                },
                new BankTransaction
                {
                    Description = "Refund for postponed workshop by First Up Consultants.",
                    Type = TransactionType.Positive, CompanyName = "First Up Consultants"
                },
            });

            // Sample transactions for Fourth Coffee
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Monthly coffee subscription from Fourth Coffee.", Type = TransactionType.Negative,
                    CompanyName = "Fourth Coffee"
                },
                new BankTransaction
                {
                    Description = "Refund for wrong coffee shipment by Fourth Coffee.", Type = TransactionType.Positive,
                    CompanyName = "Fourth Coffee"
                },
                new BankTransaction
                {
                    Description = "Purchase of coffee beans from Fourth Coffee.", Type = TransactionType.Negative,
                    CompanyName = "Fourth Coffee"
                },
                new BankTransaction
                {
                    Description = "Coffee machine maintenance by Fourth Coffee.", Type = TransactionType.Negative,
                    CompanyName = "Fourth Coffee"
                },
                new BankTransaction
                {
                    Description = "Gift card reload for Fourth Coffee.", Type = TransactionType.Negative,
                    CompanyName = "Fourth Coffee"
                },
                new BankTransaction
                {
                    Description = "Refund for double billing by Fourth Coffee.", Type = TransactionType.Positive,
                    CompanyName = "Fourth Coffee"
                },
            });

            // Sample transactions for Graphic Design Institute
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Design course enrollment at Graphic Design Institute.",
                    Type = TransactionType.Negative, CompanyName = "Graphic Design Institute"
                },
                new BankTransaction
                {
                    Description = "Refund for course cancellation at Graphic Design Institute.",
                    Type = TransactionType.Positive, CompanyName = "Graphic Design Institute"
                },
                new BankTransaction
                {
                    Description = "Purchase of design software from Graphic Design Institute.",
                    Type = TransactionType.Negative, CompanyName = "Graphic Design Institute"
                },
                new BankTransaction
                {
                    Description = "Annual membership fee for Graphic Design Institute.",
                    Type = TransactionType.Negative, CompanyName = "Graphic Design Institute"
                },
                new BankTransaction
                {
                    Description = "Workshop fee for advanced design techniques at Graphic Design Institute.",
                    Type = TransactionType.Negative, CompanyName = "Graphic Design Institute"
                },
                new BankTransaction
                {
                    Description = "Refund for software issues from Graphic Design Institute.",
                    Type = TransactionType.Positive, CompanyName = "Graphic Design Institute"
                },
            });

            // Sample transactions for Humongous Insurance
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Monthly car insurance premium to Humongous Insurance.",
                    Type = TransactionType.Negative, CompanyName = "Humongous Insurance"
                },
                new BankTransaction
                {
                    Description = "Insurance claim payout from Humongous Insurance.", Type = TransactionType.Positive,
                    CompanyName = "Humongous Insurance"
                },
                new BankTransaction
                {
                    Description = "Life insurance annual premium to Humongous Insurance.",
                    Type = TransactionType.Negative, CompanyName = "Humongous Insurance"
                },
                new BankTransaction
                {
                    Description = "Home insurance policy renewal with Humongous Insurance.",
                    Type = TransactionType.Negative, CompanyName = "Humongous Insurance"
                },
                new BankTransaction
                {
                    Description = "Travel insurance purchase from Humongous Insurance.",
                    Type = TransactionType.Negative, CompanyName = "Humongous Insurance"
                },
                new BankTransaction
                {
                    Description = "Refund for policy cancellation at Humongous Insurance.",
                    Type = TransactionType.Positive, CompanyName = "Humongous Insurance"
                },
            });

            // Sample transactions for Lamna Healthcare Company
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Monthly medical insurance premium to Lamna Healthcare Company.",
                    Type = TransactionType.Negative, CompanyName = "Lamna Healthcare Company"
                },
                new BankTransaction
                {
                    Description = "Medical procedure refund from Lamna Healthcare Company.",
                    Type = TransactionType.Positive, CompanyName = "Lamna Healthcare Company"
                },
                new BankTransaction
                {
                    Description = "Purchase of health supplements from Lamna Healthcare Company.",
                    Type = TransactionType.Negative, CompanyName = "Lamna Healthcare Company"
                },
                new BankTransaction
                {
                    Description = "Health check-up fee at Lamna Healthcare Company.", Type = TransactionType.Negative,
                    CompanyName = "Lamna Healthcare Company"
                },
                new BankTransaction
                {
                    Description = "Annual gym membership fee with Lamna Healthcare Company.",
                    Type = TransactionType.Negative, CompanyName = "Lamna Healthcare Company"
                },
                new BankTransaction
                {
                    Description = "Refund for unused gym sessions by Lamna Healthcare Company.",
                    Type = TransactionType.Positive, CompanyName = "Lamna Healthcare Company"
                },
            });

            // Sample transactions for Liberty's Delightful Sinful Bakery & Cafe
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Purchase of pastries from Liberty's Delightful Sinful Bakery & Cafe.",
                    Type = TransactionType.Negative, CompanyName = "Liberty's Delightful Sinful Bakery & Cafe"
                },
                new BankTransaction
                {
                    Description = "Refund for wrong order at Liberty's Delightful Sinful Bakery & Cafe.",
                    Type = TransactionType.Positive, CompanyName = "Liberty's Delightful Sinful Bakery & Cafe"
                },
                new BankTransaction
                {
                    Description = "Monthly coffee subscription from Liberty's Delightful Sinful Bakery & Cafe.",
                    Type = TransactionType.Negative, CompanyName = "Liberty's Delightful Sinful Bakery & Cafe"
                },
                new BankTransaction
                {
                    Description = "Booking of event space at Liberty's Delightful Sinful Bakery & Cafe.",
                    Type = TransactionType.Negative, CompanyName = "Liberty's Delightful Sinful Bakery & Cafe"
                },
                new BankTransaction
                {
                    Description = "Cake customization fee with Liberty's Delightful Sinful Bakery & Cafe.",
                    Type = TransactionType.Negative, CompanyName = "Liberty's Delightful Sinful Bakery & Cafe"
                },
                new BankTransaction
                {
                    Description = "Refund for event cancellation at Liberty's Delightful Sinful Bakery & Cafe.",
                    Type = TransactionType.Positive, CompanyName = "Liberty's Delightful Sinful Bakery & Cafe"
                },
            });

            // Sample transactions for Lucerne Publishing
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Purchase of e-books from Lucerne Publishing.", Type = TransactionType.Negative,
                    CompanyName = "Lucerne Publishing"
                },
                new BankTransaction
                {
                    Description = "Royalty income from Lucerne Publishing.", Type = TransactionType.Positive,
                    CompanyName = "Lucerne Publishing"
                },
                new BankTransaction
                {
                    Description = "Subscription to monthly magazines with Lucerne Publishing.",
                    Type = TransactionType.Negative, CompanyName = "Lucerne Publishing"
                },
                new BankTransaction
                {
                    Description = "Annual academic journal fee with Lucerne Publishing.",
                    Type = TransactionType.Negative, CompanyName = "Lucerne Publishing"
                },
                new BankTransaction
                {
                    Description = "Purchase of limited edition print from Lucerne Publishing.",
                    Type = TransactionType.Negative, CompanyName = "Lucerne Publishing"
                },
                new BankTransaction
                {
                    Description = "Refund for undelivered magazine issue by Lucerne Publishing.",
                    Type = TransactionType.Positive, CompanyName = "Lucerne Publishing"
                },
            });

            // Sample transactions for Margie's Travel
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Vacation package booking with Margie's Travel.", Type = TransactionType.Negative,
                    CompanyName = "Margie's Travel"
                },
                new BankTransaction
                {
                    Description = "Refund for trip cancellation from Margie's Travel.", Type = TransactionType.Positive,
                    CompanyName = "Margie's Travel"
                },
                new BankTransaction
                {
                    Description = "Travel insurance fee with Margie's Travel.", Type = TransactionType.Negative,
                    CompanyName = "Margie's Travel"
                },
                new BankTransaction
                {
                    Description = "Excursion booking during vacation with Margie's Travel.",
                    Type = TransactionType.Negative, CompanyName = "Margie's Travel"
                },
                new BankTransaction
                {
                    Description = "Currency exchange fee at Margie's Travel.", Type = TransactionType.Negative,
                    CompanyName = "Margie's Travel"
                },
                new BankTransaction
                {
                    Description = "Refund for hotel overbooking by Margie's Travel.", Type = TransactionType.Positive,
                    CompanyName = "Margie's Travel"
                },
            });


            // Sample transactions for Munson's Pickles and Preserves Farm
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Purchase of gourmet pickles from Munson's Pickles and Preserves Farm.",
                    Type = TransactionType.Negative, CompanyName = "Munson's Pickles and Preserves Farm"
                },
                new BankTransaction
                {
                    Description = "Refund for damaged product from Munson's Pickles and Preserves Farm.",
                    Type = TransactionType.Positive, CompanyName = "Munson's Pickles and Preserves Farm"
                },
                new BankTransaction
                {
                    Description = "Monthly jam subscription with Munson's Pickles and Preserves Farm.",
                    Type = TransactionType.Negative, CompanyName = "Munson's Pickles and Preserves Farm"
                },
                new BankTransaction
                {
                    Description = "Purchase of gift basket for holidays from Munson's Pickles and Preserves Farm.",
                    Type = TransactionType.Negative, CompanyName = "Munson's Pickles and Preserves Farm"
                },
                new BankTransaction
                {
                    Description = "Bulk order of pickled veggies from Munson's Pickles and Preserves Farm.",
                    Type = TransactionType.Negative, CompanyName = "Munson's Pickles and Preserves Farm"
                },
                new BankTransaction
                {
                    Description = "Refund for cancelled subscription by Munson's Pickles and Preserves Farm.",
                    Type = TransactionType.Positive, CompanyName = "Munson's Pickles and Preserves Farm"
                },
            });

            // Sample transactions for Nod Publishers
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Yearly subscription for literary magazine from Nod Publishers.",
                    Type = TransactionType.Negative, CompanyName = "Nod Publishers"
                },
                new BankTransaction
                {
                    Description = "Royalty payment for manuscript from Nod Publishers.",
                    Type = TransactionType.Positive, CompanyName = "Nod Publishers"
                },
                new BankTransaction
                {
                    Description = "Purchase of best-selling novels from Nod Publishers.",
                    Type = TransactionType.Negative, CompanyName = "Nod Publishers"
                },
                new BankTransaction
                {
                    Description = "Manuscript editing fee at Nod Publishers.", Type = TransactionType.Negative,
                    CompanyName = "Nod Publishers"
                },
                new BankTransaction
                {
                    Description = "Bulk purchase of academic books from Nod Publishers.",
                    Type = TransactionType.Negative, CompanyName = "Nod Publishers"
                },
                new BankTransaction
                {
                    Description = "Refund for misprinted book from Nod Publishers.", Type = TransactionType.Positive,
                    CompanyName = "Nod Publishers"
                },
            });

            // Sample transactions for Northwind Traders
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Bulk food items purchase from Northwind Traders.", Type = TransactionType.Negative,
                    CompanyName = "Northwind Traders"
                },
                new BankTransaction
                {
                    Description = "Refund for faulty product from Northwind Traders.", Type = TransactionType.Positive,
                    CompanyName = "Northwind Traders"
                },
                new BankTransaction
                {
                    Description = "Membership for monthly gourmet box from Northwind Traders.",
                    Type = TransactionType.Negative, CompanyName = "Northwind Traders"
                },
                new BankTransaction
                {
                    Description = "Purchase of wine collection from Northwind Traders.",
                    Type = TransactionType.Negative, CompanyName = "Northwind Traders"
                },
                new BankTransaction
                {
                    Description = "Organic fruit and veggie box subscription with Northwind Traders.",
                    Type = TransactionType.Negative, CompanyName = "Northwind Traders"
                },
                new BankTransaction
                {
                    Description = "Refund for double-billed subscription by Northwind Traders.",
                    Type = TransactionType.Positive, CompanyName = "Northwind Traders"
                },
            });

            // Sample transactions for Proseware, Inc.
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Purchase of software licenses from Proseware, Inc.", Type = TransactionType.Negative,
                    CompanyName = "Proseware, Inc."
                },
                new BankTransaction
                {
                    Description = "Refund for malfunctioning software from Proseware, Inc.",
                    Type = TransactionType.Positive, CompanyName = "Proseware, Inc."
                },
                new BankTransaction
                {
                    Description = "Monthly cloud services fee with Proseware, Inc.", Type = TransactionType.Negative,
                    CompanyName = "Proseware, Inc."
                },
                new BankTransaction
                {
                    Description = "Custom software development fee with Proseware, Inc.",
                    Type = TransactionType.Negative, CompanyName = "Proseware, Inc."
                },
                new BankTransaction
                {
                    Description = "Annual maintenance and support fee with Proseware, Inc.",
                    Type = TransactionType.Negative, CompanyName = "Proseware, Inc."
                },
                new BankTransaction
                {
                    Description = "Rebate for early contract renewal with Proseware, Inc.",
                    Type = TransactionType.Positive, CompanyName = "Proseware, Inc."
                },
            });

            // Sample transactions for Relecloud
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Subscription for cloud storage services from Relecloud.",
                    Type = TransactionType.Negative, CompanyName = "Relecloud"
                },
                new BankTransaction
                {
                    Description = "Refund for service interruption from Relecloud.", Type = TransactionType.Positive,
                    CompanyName = "Relecloud"
                },
                new BankTransaction
                {
                    Description = "Purchase of additional cloud compute units from Relecloud.",
                    Type = TransactionType.Negative, CompanyName = "Relecloud"
                },
                new BankTransaction
                {
                    Description = "Web hosting fee with Relecloud.", Type = TransactionType.Negative,
                    CompanyName = "Relecloud"
                },
                new BankTransaction
                {
                    Description = "Database backup service fee with Relecloud.", Type = TransactionType.Negative,
                    CompanyName = "Relecloud"
                },
                new BankTransaction
                {
                    Description = "Credit for referral to Relecloud services.", Type = TransactionType.Positive,
                    CompanyName = "Relecloud"
                },
            });

            // Sample transactions for School of Fine Art
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Tuition fee for art course at School of Fine Art.", Type = TransactionType.Negative,
                    CompanyName = "School of Fine Art"
                },
                new BankTransaction
                {
                    Description = "Refund for cancelled workshop at School of Fine Art.",
                    Type = TransactionType.Positive, CompanyName = "School of Fine Art"
                },
                new BankTransaction
                {
                    Description = "Purchase of art supplies from School of Fine Art.", Type = TransactionType.Negative,
                    CompanyName = "School of Fine Art"
                },
                new BankTransaction
                {
                    Description = "Booking of exhibition space at School of Fine Art.", Type = TransactionType.Negative,
                    CompanyName = "School of Fine Art"
                },
                new BankTransaction
                {
                    Description = "Artwork purchase from School of Fine Art student showcase.",
                    Type = TransactionType.Negative, CompanyName = "School of Fine Art"
                },
                new BankTransaction
                {
                    Description = "Refund for damaged art piece from School of Fine Art.",
                    Type = TransactionType.Positive, CompanyName = "School of Fine Art"
                },
            });

            // Sample transactions for Southridge Video
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Monthly subscription for Southridge Video streaming service.",
                    Type = TransactionType.Negative, CompanyName = "Southridge Video"
                },
                new BankTransaction
                {
                    Description = "Refund for incorrect video purchase on Southridge Video.",
                    Type = TransactionType.Positive, CompanyName = "Southridge Video"
                },
                new BankTransaction
                {
                    Description = "Purchase of movie bundle on Southridge Video.", Type = TransactionType.Negative,
                    CompanyName = "Southridge Video"
                },
                new BankTransaction
                {
                    Description = "Rental of new movie release on Southridge Video.", Type = TransactionType.Negative,
                    CompanyName = "Southridge Video"
                },
                new BankTransaction
                {
                    Description = "Annual premium subscription renewal with Southridge Video.",
                    Type = TransactionType.Negative, CompanyName = "Southridge Video"
                },
                new BankTransaction
                {
                    Description = "Credit for participating in Southridge Video survey.",
                    Type = TransactionType.Positive, CompanyName = "Southridge Video"
                },
            });

            // Sample transactions for Tailspin Toys
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Purchase of kids toys from Tailspin Toys.", Type = TransactionType.Negative,
                    CompanyName = "Tailspin Toys"
                },
                new BankTransaction
                {
                    Description = "Refund for defective toy from Tailspin Toys.", Type = TransactionType.Positive,
                    CompanyName = "Tailspin Toys"
                },
                new BankTransaction
                {
                    Description = "Exclusive toy collection pre-order from Tailspin Toys.",
                    Type = TransactionType.Negative, CompanyName = "Tailspin Toys"
                },
                new BankTransaction
                {
                    Description = "Annual subscription for toy collectors club with Tailspin Toys.",
                    Type = TransactionType.Negative, CompanyName = "Tailspin Toys"
                },
                new BankTransaction
                {
                    Description = "Purchase of board games from Tailspin Toys.", Type = TransactionType.Negative,
                    CompanyName = "Tailspin Toys"
                },
                new BankTransaction
                {
                    Description = "Gift card redemption at Tailspin Toys.", Type = TransactionType.Positive,
                    CompanyName = "Tailspin Toys"
                },
            });

            // Sample transactions for Tailwind Traders
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Purchase of office furniture from Tailwind Traders.",
                    Type = TransactionType.Negative, CompanyName = "Tailwind Traders"
                },
                new BankTransaction
                {
                    Description = "Refund for defective chair from Tailwind Traders.", Type = TransactionType.Positive,
                    CompanyName = "Tailwind Traders"
                },
                new BankTransaction
                {
                    Description = "Bulk order of office supplies from Tailwind Traders.",
                    Type = TransactionType.Negative, CompanyName = "Tailwind Traders"
                },
                new BankTransaction
                {
                    Description = "Annual software license renewal with Tailwind Traders.",
                    Type = TransactionType.Negative, CompanyName = "Tailwind Traders"
                },
                new BankTransaction
                {
                    Description = "Purchase of ergonomic equipment from Tailwind Traders.",
                    Type = TransactionType.Negative, CompanyName = "Tailwind Traders"
                },
                new BankTransaction
                {
                    Description = "Cashback on bulk purchase at Tailwind Traders.", Type = TransactionType.Positive,
                    CompanyName = "Tailwind Traders"
                },
            });

            // Sample transactions for Trey Research
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Subscription for monthly research reports from Trey Research.",
                    Type = TransactionType.Negative, CompanyName = "Trey Research"
                },
                new BankTransaction
                {
                    Description = "Refund for incorrect report from Trey Research.", Type = TransactionType.Positive,
                    CompanyName = "Trey Research"
                },
                new BankTransaction
                {
                    Description = "Purchase of annual research compilation from Trey Research.",
                    Type = TransactionType.Negative, CompanyName = "Trey Research"
                },
                new BankTransaction
                {
                    Description = "Consultation fee with Trey Research expert.", Type = TransactionType.Negative,
                    CompanyName = "Trey Research"
                },
                new BankTransaction
                {
                    Description = "Purchase of specialized research equipment from Trey Research.",
                    Type = TransactionType.Negative, CompanyName = "Trey Research"
                },
                new BankTransaction
                {
                    Description = "Grant received for collaborative study with Trey Research.",
                    Type = TransactionType.Positive, CompanyName = "Trey Research"
                },
            });

            // Sample transactions for The Phone Company
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Monthly phone bill payment to The Phone Company.", Type = TransactionType.Negative,
                    CompanyName = "The Phone Company"
                },
                new BankTransaction
                {
                    Description = "Refund for incorrect charges from The Phone Company.",
                    Type = TransactionType.Positive, CompanyName = "The Phone Company"
                },
                new BankTransaction
                {
                    Description = "Purchase of new smartphone from The Phone Company.", Type = TransactionType.Negative,
                    CompanyName = "The Phone Company"
                },
                new BankTransaction
                {
                    Description = "Annual plan renewal with The Phone Company.", Type = TransactionType.Negative,
                    CompanyName = "The Phone Company"
                },
                new BankTransaction
                {
                    Description = "Roaming charges for international travel with The Phone Company.",
                    Type = TransactionType.Negative, CompanyName = "The Phone Company"
                },
                new BankTransaction
                {
                    Description = "Rebate for trading in old phone at The Phone Company.",
                    Type = TransactionType.Positive, CompanyName = "The Phone Company"
                },
            });

            // Sample transactions for VanArsdel, Ltd.
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Purchase of luxury items from VanArsdel, Ltd.", Type = TransactionType.Negative,
                    CompanyName = "VanArsdel, Ltd."
                },
                new BankTransaction
                {
                    Description = "Refund for returned item at VanArsdel, Ltd.", Type = TransactionType.Positive,
                    CompanyName = "VanArsdel, Ltd."
                },
                new BankTransaction
                {
                    Description = "Custom jewelry order from VanArsdel, Ltd.", Type = TransactionType.Negative,
                    CompanyName = "VanArsdel, Ltd."
                },
                new BankTransaction
                {
                    Description = "Annual membership fee for VanArsdel, Ltd. exclusive club.",
                    Type = TransactionType.Negative, CompanyName = "VanArsdel, Ltd."
                },
                new BankTransaction
                {
                    Description = "Purchase of designer apparel from VanArsdel, Ltd.", Type = TransactionType.Negative,
                    CompanyName = "VanArsdel, Ltd."
                },
                new BankTransaction
                {
                    Description = "Loyalty bonus from VanArsdel, Ltd.", Type = TransactionType.Positive,
                    CompanyName = "VanArsdel, Ltd."
                },
            });

            // Sample transactions for Wide World Importers
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Bulk order of imported goods from Wide World Importers.",
                    Type = TransactionType.Negative, CompanyName = "Wide World Importers"
                },
                new BankTransaction
                {
                    Description = "Refund for damaged imported item from Wide World Importers.",
                    Type = TransactionType.Positive, CompanyName = "Wide World Importers"
                },
                new BankTransaction
                {
                    Description = "Purchase of specialty foods from Wide World Importers.",
                    Type = TransactionType.Negative, CompanyName = "Wide World Importers"
                },
                new BankTransaction
                {
                    Description = "Annual membership for priority shipping with Wide World Importers.",
                    Type = TransactionType.Negative, CompanyName = "Wide World Importers"
                },
                new BankTransaction
                {
                    Description = "Order of imported decor items from Wide World Importers.",
                    Type = TransactionType.Negative, CompanyName = "Wide World Importers"
                },
                new BankTransaction
                {
                    Description = "Cashback for bulk purchase from Wide World Importers.",
                    Type = TransactionType.Positive, CompanyName = "Wide World Importers"
                },
            });

            // Sample transactions for Wingtip Toys
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Purchase of collectible toys from Wingtip Toys.", Type = TransactionType.Negative,
                    CompanyName = "Wingtip Toys"
                },
                new BankTransaction
                {
                    Description = "Refund for a missing toy piece from Wingtip Toys.", Type = TransactionType.Positive,
                    CompanyName = "Wingtip Toys"
                },
                new BankTransaction
                {
                    Description = "Exclusive toy set order from Wingtip Toys.", Type = TransactionType.Negative,
                    CompanyName = "Wingtip Toys"
                },
                new BankTransaction
                {
                    Description = "Toy repair service fee at Wingtip Toys.", Type = TransactionType.Negative,
                    CompanyName = "Wingtip Toys"
                },
                new BankTransaction
                {
                    Description = "Purchase of toy storage solutions from Wingtip Toys.",
                    Type = TransactionType.Negative, CompanyName = "Wingtip Toys"
                },
                new BankTransaction
                {
                    Description = "Redemption of loyalty points at Wingtip Toys.", Type = TransactionType.Positive,
                    CompanyName = "Wingtip Toys"
                },
            });

            // Sample transactions for Woodgrove Bank
            transactions.AddRange(new[]
            {
                new BankTransaction
                {
                    Description = "Monthly service fee from Woodgrove Bank.", Type = TransactionType.Negative,
                    CompanyName = "Woodgrove Bank"
                },
                new BankTransaction
                {
                    Description = "Interest deposit from Woodgrove Bank savings account.",
                    Type = TransactionType.Positive, CompanyName = "Woodgrove Bank"
                },
                new BankTransaction
                {
                    Description = "Loan payment to Woodgrove Bank.", Type = TransactionType.Negative,
                    CompanyName = "Woodgrove Bank"
                },
                new BankTransaction
                {
                    Description = "Foreign transaction fee by Woodgrove Bank.", Type = TransactionType.Negative,
                    CompanyName = "Woodgrove Bank"
                },
                new BankTransaction
                {
                    Description = "Wire transfer fee at Woodgrove Bank.", Type = TransactionType.Negative,
                    CompanyName = "Woodgrove Bank"
                },
                new BankTransaction
                {
                    Description = "Cashback reward deposit from Woodgrove Bank credit card.",
                    Type = TransactionType.Positive, CompanyName = "Woodgrove Bank"
                },
            });

            return transactions;
        }

    }
}