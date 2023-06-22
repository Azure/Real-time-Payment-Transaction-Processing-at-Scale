<<<<<<< HEAD:src/CorePayments.FunctionApp/Processor/ProcessCustomerView.cs
using CorePayments.Infrastructure.Domain.Entities;
using Microsoft.Azure.Functions.Worker;
=======
using Microsoft.Azure.WebJobs;
>>>>>>> f482f0adcc278b6f40833ae5755a42e4c19aa5c3:src/cosmos-payments-demo/Processor/ProcessCustomerView.cs
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorePayments.FunctionApp.Processor
{
    public static class ProcessCustomerView
    {
<<<<<<< HEAD:src/CorePayments.FunctionApp/Processor/ProcessCustomerView.cs
        [Function("ProcessCustomerView")]
=======
        static ProcessCustomerView()
        {
            isMasterRegion = Convert.ToBoolean(Environment.GetEnvironmentVariable("isMasterRegion"));
        }

        static bool isMasterRegion;

        [FunctionName("ProcessCustomerView")]
>>>>>>> f482f0adcc278b6f40833ae5755a42e4c19aa5c3:src/cosmos-payments-demo/Processor/ProcessCustomerView.cs
        public static async Task RunAsync([CosmosDBTrigger(
            databaseName: "%paymentsDatabase%",
            containerName: "%transactionsContainer%",
            Connection = "CosmosDBConnection",
            CreateLeaseContainerIfNotExists = true,
            StartFromBeginning = true,
            FeedPollDelay = 1000,
            MaxItemsPerInvocation = 50,
<<<<<<< HEAD:src/CorePayments.FunctionApp/Processor/ProcessCustomerView.cs
            LeaseContainerName = "leases")] IReadOnlyList<JObject> input,
=======
            PreferredLocations = "%preferredRegions%",
            LeaseContainerName = "leases")]IReadOnlyList<JObject> input,
>>>>>>> f482f0adcc278b6f40833ae5755a42e4c19aa5c3:src/cosmos-payments-demo/Processor/ProcessCustomerView.cs
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