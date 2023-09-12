using CorePayments.Infrastructure;
using CorePayments.Infrastructure.Domain.Settings;
using CorePayments.Infrastructure.Events;
using CorePayments.Infrastructure.Repository;
using CorePayments.WorkerService;
using Microsoft.Azure.Cosmos.Fluent;
using Azure.Identity;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));

builder.Services.AddSingleton(s =>
{
    var endpoint = builder.Configuration["CosmosDBConnection:accountEndpoint"];
    var preferredRegions = builder.Configuration["DatabaseSettings:PreferredRegions"];
    var clientId = builder.Configuration[Constants.Identity.ClientId];
    var region = "";

    var regions = string.IsNullOrEmpty(preferredRegions)
        ? Array.Empty<string>()
        : preferredRegions.Split(',');

#if DEBUG
    var credential = new Azure.Identity.DefaultAzureCredential();
#else
    var credential = new ChainedTokenCredential(
            new ManagedIdentityCredential(clientId),
            new AzureCliCredential()
        );
#endif

    if (!regions.Any())
        return new CosmosClientBuilder(accountEndpoint: endpoint,
                tokenCredential: credential)
            .Build();
    if (regions.Length == 1)
    {
        return new CosmosClientBuilder(accountEndpoint: endpoint, tokenCredential: credential)
            .WithApplicationRegion(regions[0])
            .Build();
    }
    return new CosmosClientBuilder(accountEndpoint: endpoint, tokenCredential: credential)
        .WithApplicationPreferredRegions(regions)
        .Build();
});

builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();
builder.Services.AddSingleton<ICosmosDbChangeFeedService, CosmosDbChangeFeedService>();

builder.Services.AddHostedService<ChangeFeedWorker>();

var host = builder.Build();

host.Run();
