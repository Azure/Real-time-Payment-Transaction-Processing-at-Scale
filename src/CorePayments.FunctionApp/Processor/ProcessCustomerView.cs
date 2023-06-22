using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Repository;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorePayments.FunctionApp.Processor
{
    public class ProcessCustomerView
    {
        readonly bool _isMasterRegion;
        readonly ICustomerRepository _customerRepository;

        public ProcessCustomerView(
            ICustomerRepository customerRepository)
        {
            _isMasterRegion = Convert.ToBoolean(Environment.GetEnvironmentVariable("isMasterRegion"));
            _customerRepository = customerRepository;
        }

        [Function("ProcessCustomerView")]
        public async Task RunAsync([CosmosDBTrigger(
            databaseName: "%paymentsDatabase%",
            containerName: "%transactionsContainer%",
            Connection = "CosmosDBConnection",
            CreateLeaseContainerIfNotExists = true,
            StartFromBeginning = true,
            FeedPollDelay = 1000,
            MaxItemsPerInvocation = 50,
            PreferredLocations = "%preferredRegions%",
            LeaseContainerName = "leases")]IReadOnlyList<JObject> input,
            FunctionContext context)
        {
            var logger = context.GetLogger<ProcessCustomerView>();

            if (!_isMasterRegion)
                return;

            await Parallel.ForEachAsync(input, async (record, token) =>
            {
                try
                {
                    await _customerRepository.CreateItem(record);
                }
                catch (Exception ex)
                {
                    //Should handle DLQ
                    logger.LogError(ex.Message, ex);
                }
            });
        }
    }
}