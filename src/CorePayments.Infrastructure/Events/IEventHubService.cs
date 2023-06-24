using Azure.Messaging.EventHubs.Consumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePayments.Infrastructure.Events
{
    public interface IEventHubService
    {
        Task TriggerTrackingEvent<T>(T eventPayload);

        Task TriggerEvent<T>(T eventPayload, string eventHubName);

        IAsyncEnumerable<PartitionEvent> ReadEvents(string eventHubName, CancellationToken cancellationToken);

        IAsyncEnumerable<PartitionEvent> ReadTrackingEvents(CancellationToken cancellationToken);
    }
}
