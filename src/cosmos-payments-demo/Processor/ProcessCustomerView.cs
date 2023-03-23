using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cosmos_payments_demo.Processor
{
    public static class ProcessCustomerView
    {
        static ProcessCustomerView()
        {
            isMasterRegion = Convert.ToBoolean(Environment.GetEnvironmentVariable("isMasterRegion"));
        }

        static bool isMasterRegion;

        [FunctionName("ProcessCustomerView")]
        public static async Task RunAsync([CosmosDBTrigger(
            databaseName: "%paymentsDatabase%",
            containerName: "%transactionsContainer%",
            Connection = "CosmosDBConnection",
            CreateLeaseContainerIfNotExists = false,
            StartFromBeginning = true,
            FeedPollDelay = 1000,
            MaxItemsPerInvocation = 50,
            PreferredLocations = "%preferredRegions%",
            LeaseContainerName = "leases")]IReadOnlyList<JObject> input,
            [CosmosDB(
                databaseName: "%paymentsDatabase%",
                containerName: "%customerContainer%",
                Connection = "CosmosDBConnection")] IAsyncCollector<JObject> eventCollector,
            ILogger log)
        {
            if (!isMasterRegion)
                return;

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