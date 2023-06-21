using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton(s =>
        {
            var endpoint = hostContext.Configuration["CosmosDBConnection__accountEndpoint"];

            return new CosmosClientBuilder(endpoint, new Azure.Identity.DefaultAzureCredential())
                .Build();
        });
    })
    .Build();

await host.RunAsync();