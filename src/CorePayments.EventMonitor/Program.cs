using CorePayments.Infrastructure;
using CorePayments.Infrastructure.Events;
using Microsoft.Extensions.Configuration;
using System.Text;

Console.WriteLine("Welcome to the Payments Event Monitor!");

var configBuilder = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
var config = configBuilder.Build();

var eventHub = new EventHubService(
            config["EventHubConnection__fullyQualifiedNamespace"],
            Constants.EventHubs.PaymentEvents);

var cancellationSource = new CancellationTokenSource();
cancellationSource.CancelAfter(TimeSpan.FromHours(1));   // Listen to events for up to one hour

await foreach (var partitionEvent in eventHub.ReadTrackingEvents(cancellationSource.Token))
{
    Console.WriteLine("Tracking event:");
    Console.WriteLine(Encoding.UTF8.GetString(partitionEvent.Data.EventBody.ToArray()));
}

Console.WriteLine("Finished listening.");
