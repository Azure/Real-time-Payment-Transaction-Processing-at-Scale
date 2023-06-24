using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CorePayments.Infrastructure.Events
{
    public class EventHubService : IEventHubService
    {
        Dictionary<string, EventHubProducerClient> _producerClients = new Dictionary<string, EventHubProducerClient>();
        Dictionary<string, EventHubConsumerClient> _consumerClients = new Dictionary<string, EventHubConsumerClient>();

        readonly string _namespace;
        readonly string _trackingEventHubName;

        public EventHubService(string qualifiedNamespace, string trackingEventHubName)
        {
            if (string.IsNullOrWhiteSpace(qualifiedNamespace))
                throw new ArgumentNullException(nameof(qualifiedNamespace));

            if (string.IsNullOrWhiteSpace(trackingEventHubName))
                throw new ArgumentNullException(nameof(trackingEventHubName));

            _namespace = qualifiedNamespace;
            _trackingEventHubName = trackingEventHubName;
        }

        public async Task TriggerTrackingEvent<T>(T eventPayload)
        {
            var client = GetProducerClient(_trackingEventHubName);
            await client.SendAsync(new EventData[] { new EventData(JsonSerializer.Serialize<TrackingEventData>(new TrackingEventData
            {
                Timestamp = DateTime.UtcNow,    //TODO: Add more detailed tracking information if needed
                Data = eventPayload
            })) });
        }

        public async Task TriggerEvent<T>(T eventPayload, string eventHubName)
        {
            if (string.IsNullOrWhiteSpace(eventHubName))
                throw new ArgumentNullException(nameof(eventHubName));

            var client = GetProducerClient(eventHubName);
            await client.SendAsync(new EventData[] { new EventData(JsonSerializer.Serialize(eventPayload)) });
        }

        public IAsyncEnumerable<PartitionEvent> ReadEvents(string eventHubName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(eventHubName))
                throw new ArgumentNullException(nameof(eventHubName));

            var client = GetConsumerClient(eventHubName);
            return client.ReadEventsAsync(cancellationToken);
        }

        public IAsyncEnumerable<PartitionEvent> ReadTrackingEvents(CancellationToken cancellationToken)
        {
            return ReadEvents(_trackingEventHubName, cancellationToken);
        }

        private EventHubProducerClient GetProducerClient(string eventHubName)
        {
            if (_producerClients.ContainsKey(eventHubName))
                return _producerClients[eventHubName];

            var newClient = new EventHubProducerClient(_namespace, eventHubName, new Azure.Identity.DefaultAzureCredential());
            _producerClients.Add(eventHubName, newClient);
            return newClient;
        }

        private EventHubConsumerClient GetConsumerClient(string eventHubName)
        {
            if (_consumerClients.ContainsKey(eventHubName))
                return _consumerClients[eventHubName];

            var newClient = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, _namespace, eventHubName, new Azure.Identity.DefaultAzureCredential());
            _consumerClients.Add(eventHubName, newClient);
            return newClient;
        }
    }
}
