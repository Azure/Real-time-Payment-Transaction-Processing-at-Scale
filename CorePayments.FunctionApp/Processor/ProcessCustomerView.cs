using CorePayments.Infrastructure.Domain.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorePayments.FunctionApp.Processor
{
    public static class ProcessCustomerView
    {
        [Function("ProcessCustomerView")]
        public static async Task RunAsync([CosmosDBTrigger(
            databaseName: "%paymentsDatabase%",
            containerName: "%transactionsContainer%",
            Connection = "CosmosDBConnection",
            CreateLeaseContainerIfNotExists = true,
            StartFromBeginning = true,
            FeedPollDelay = 1000,
            MaxItemsPerInvocation = 50,
            LeaseContainerName = "leases")] IReadOnlyList<JObject> input,
            [CosmosDB(
                databaseName: "%paymentsDatabase%",
                containerName: "%customerContainer%",
                Connection = "CosmosDBConnection")] IAsyncCollector<JObject> eventCollector,
            ILogger log)
        {
            await Parallel.ForEachAsync(input, async (record, token) =>
            {
                try
                {
                    await eventCollector.AddAsync(record, token);
                }
                catch (Exception ex)
                {
                    //Should handle DLQ
                    log.LogError(ex.Message, ex);
                }
            });
        }
    }
}