using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Domain.Settings;
using CorePayments.Infrastructure.Repository;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            ICustomerRepository customerRepository, IOptions<DatabaseSettings> options)
        {
            _isMasterRegion = options.Value.IsMasterRegion;
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
                    await _customerRepository.UpsertItem(record);
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