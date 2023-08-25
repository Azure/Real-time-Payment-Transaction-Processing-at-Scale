using CorePayments.Infrastructure;
using CorePayments.Infrastructure.Domain.Settings;
using CorePayments.Infrastructure.Events;
using CorePayments.Infrastructure.Repository;
using CorePayments.SemanticKernel;
using CorePayments.WebAPI.Components;
using CorePayments.WebAPI.Endpoints.Http;
using Microsoft.Azure.Cosmos.Fluent;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var allowAllCorsOrigins = "AllowAllOrigins";

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddCors(policyBuilder =>
{
    policyBuilder.AddPolicy(allowAllCorsOrigins,
        policy =>
        {
            policy.AllowAnyOrigin();
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
        });
});

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));

builder.Services.AddSingleton(s =>
{
    var endpoint = builder.Configuration["CosmosDBConnection:accountEndpoint"];
    var preferredRegions = builder.Configuration["DatabaseSettings:PreferredRegions"];
    var region = "";

    var regions = string.IsNullOrEmpty(preferredRegions)
        ? Array.Empty<string>()
        : preferredRegions.Split(',');

    if (!regions.Any())
        return new CosmosClientBuilder(accountEndpoint: endpoint,
                tokenCredential: new Azure.Identity.DefaultAzureCredential())
            .Build();
    if (regions.Length == 1)
    {
        return new CosmosClientBuilder(accountEndpoint: endpoint, tokenCredential: new Azure.Identity.DefaultAzureCredential())
            .WithApplicationRegion(regions[0])
            .Build();
    }
    return new CosmosClientBuilder(accountEndpoint: endpoint, tokenCredential: new Azure.Identity.DefaultAzureCredential())
        .WithApplicationPreferredRegions(regions)
        .Build();
});

builder.Services.AddSingleton<IEventHubService, EventHubService>(s => new EventHubService(
            builder.Configuration["EventHubConnection:fullyQualifiedNamespace"],
            Constants.EventHubs.PaymentEvents));

builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();
builder.Services.AddSingleton<IGlobalIndexRepository, GlobalIndexRepository>();
builder.Services.AddSingleton<IMemberRepository, MemberRepository>();
builder.Services.AddSingleton<ITransactionRepository, TransactionRepository>();

builder.Services.AddSingleton<IAnalyticsEngine, AnalyticsEngine>();

// Add Endpoint classes.
builder.Services.AddScoped<EndpointsBase, AccountEndpoints>();
builder.Services.AddScoped<EndpointsBase, MemberEndpoints>();

// Implement serialization resolver and rules
builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Map the REST endpoints:
using (var scope = app.Services.CreateScope())
{
    // Build collection of all EndpointsBase classes
    var services = scope.ServiceProvider.GetServices<EndpointsBase>();
    // Loop through each EndpointsBase class
    foreach (var item in services)
    {
        // Invoke the AddRoutes() method to add the routes
        item.AddRoutes(app);
    }
}

app.UseCors(allowAllCorsOrigins);

//app.UseAuthorization();

app.Run();
