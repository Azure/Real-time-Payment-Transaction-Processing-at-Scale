using CorePayments.Infrastructure;
using CorePayments.Infrastructure.Events;
using CorePayments.Infrastructure.Repository;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Linq;
using Azure.Core.Serialization;
using CorePayments.SemanticKernel;
using Microsoft.Azure.Cosmos;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        builder.Services.AddLogging();
    })
    .ConfigureAppConfiguration(con =>
    {
        con.AddUserSecrets<Program>(optional: true, reloadOnChange: false);
        con.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<AnalyticsEngineSettings>(hostContext.Configuration.GetSection("AnalyticsEngine"));

        services.AddSingleton(s =>
        {
            //var endpoint = hostContext.Configuration["CosmosDBConnection__accountEndpoint"];
            var endpoint = Environment.GetEnvironmentVariable("CosmosDBConnection__accountEndpoint");
            var preferredRegions = Environment.GetEnvironmentVariable("preferredRegions");
            var region = "";

            var regions = string.IsNullOrEmpty(preferredRegions)
                ? Array.Empty<string>()
                : preferredRegions.Split(',');

            if (regions.Any())
            {
                if (regions.Length == 1)
                {
                    return new CosmosClientBuilder(accountEndpoint: endpoint, tokenCredential: new Azure.Identity.DefaultAzureCredential())
                        .WithApplicationRegion(regions[0])
                        .Build();
                }
                return new CosmosClientBuilder(accountEndpoint: endpoint, tokenCredential: new Azure.Identity.DefaultAzureCredential())
                        .WithApplicationPreferredRegions(regions)
                        .Build();
            }

            
        });
        services.AddSingleton<IEventHubService, EventHubService>(s => new EventHubService(
            Environment.GetEnvironmentVariable("EventHubConnection__fullyQualifiedNamespace"),
            Constants.EventHubs.PaymentEvents));

        services.AddSingleton<ICustomerRepository, CustomerRepository>();
        services.AddSingleton<IGlobalIndexRepository, GlobalIndexRepository>();
        services.AddSingleton<IMemberRepository, MemberRepository>();
        services.AddSingleton<ITransactionRepository, TransactionRepository>();

        services.AddSingleton<IAnalyticsEngine, AnalyticsEngine>();

        services.Configure<WorkerOptions>(workerOptions =>
        {
            var settings = NewtonsoftJsonObjectSerializer.CreateJsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            workerOptions.Serializer = new NewtonsoftJsonObjectSerializer(settings);
        });
        //services.AddControllers().AddNewtonsoftJson();
    })
    .Build();

await host.RunAsync();