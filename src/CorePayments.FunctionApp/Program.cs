using CorePayments.Infrastructure.Repository;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

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
        services.AddSingleton(s =>
        {
            //var endpoint = hostContext.Configuration["CosmosDBConnection__accountEndpoint"];
            var endpoint = Environment.GetEnvironmentVariable("CosmosDBConnection__accountEndpoint");

            return new CosmosClientBuilder(endpoint, new Azure.Identity.DefaultAzureCredential())
                .Build();
        });
        services.AddSingleton<ICustomerRepository, CustomerRepository>();
        services.AddSingleton<IGlobalIndexRepository, GlobalIndexRepository>();
        services.AddSingleton<IMemberRepository, MemberRepository>();
        services.AddSingleton<ITransactionRepository, TransactionRepository>();
    })
    .Build();

await host.RunAsync();