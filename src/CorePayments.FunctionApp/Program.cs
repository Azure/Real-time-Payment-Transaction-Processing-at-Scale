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
using Azure.Core.Serialization;
using CorePayments.SemanticKernel;

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
        services.Configure<RulesEngineSettings>(hostContext.Configuration.GetSection("RulesEngine"));

        services.AddSingleton(s =>
        {
            //var endpoint = hostContext.Configuration["CosmosDBConnection__accountEndpoint"];
            var endpoint = Environment.GetEnvironmentVariable("CosmosDBConnection__accountEndpoint");

            return new CosmosClientBuilder(endpoint, new Azure.Identity.DefaultAzureCredential())
                .Build();
        });
        services.AddSingleton<IEventHubService, EventHubService>(s => new EventHubService(
            Environment.GetEnvironmentVariable("EventHubConnection__fullyQualifiedNamespace"),
            Constants.EventHubs.PaymentEvents));

        services.AddSingleton<ICustomerRepository, CustomerRepository>();
        services.AddSingleton<IGlobalIndexRepository, GlobalIndexRepository>();
        services.AddSingleton<IMemberRepository, MemberRepository>();
        services.AddSingleton<ITransactionRepository, TransactionRepository>();

        services.AddSingleton<IRulesEngine, RulesEngine>();

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