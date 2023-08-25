using CorePayments.Infrastructure.Repository;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CorePayments.Infrastructure.Domain.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace CorePayments.Infrastructure.Events
{
    public class CosmosDbChangeFeedService : ICosmosDbChangeFeedService
    {
        private readonly CosmosClient _client;
        private readonly Database _database;
        private readonly Container _transaction;
        private readonly Container _leases;

        private ChangeFeedProcessor _changeFeedProcessorProcessCustomerView;

        private readonly ILogger<CosmosDbChangeFeedService> _logger;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICustomerRepository _customerRepository;

        private bool _changeFeedsInitialized = false;

        public bool IsInitialized => _changeFeedsInitialized;

        public CosmosDbChangeFeedService(CosmosClient client,
            ILogger<CosmosDbChangeFeedService> logger,
            ITransactionRepository transactionRepository,
            ICustomerRepository customerRepository,
            IOptions<DatabaseSettings> options)
        {
            _client = client;
            _transactionRepository = transactionRepository;
            _customerRepository = customerRepository;
            _logger = logger;

            var database = _client.GetDatabase(options.Value.PaymentsDatabase);

            _database = database ??
                        throw new ArgumentException("Unable to connect to existing Azure Cosmos DB database.");

            _transaction = database?.GetContainer(options.Value.TransactionsContainer) ??
                        throw new ArgumentException("Unable to connect to existing Azure Cosmos DB container or database.");
            _leases = database?.GetContainer("leases") ??
                      throw new ArgumentException("Unable to connect to existing Azure Cosmos DB container or database.");
        }

        public async Task StartChangeFeedProcessorsAsync()
        {
            try
            {
                _changeFeedProcessorProcessCustomerView = _transaction
                    .GetChangeFeedProcessorBuilder<JObject>("ProcessCustomerView", ProcessCustomerViewChangeFeedHandler)
                    .WithInstanceName("ProcessCustomerView")
                    .WithLeaseContainer(_leases)
                    .WithStartTime(DateTime.MinValue.ToUniversalTime()) // Read from the beginning.
                    .Build();

                await _changeFeedProcessorProcessCustomerView.StartAsync();

                _changeFeedsInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing change feed processors.");
            }
        }

        public async Task StopChangeFeedProcessorAsync()
        {
            // Stop the ChangeFeedProcessor
            _logger.LogInformation("Stopping Change Feed Processors...");

            if (_changeFeedProcessorProcessCustomerView != null) await _changeFeedProcessorProcessCustomerView.StopAsync();

            _logger.LogInformation("Change Feed Processors stopped.");
        }

        private async Task ProcessCustomerViewChangeFeedHandler(
            ChangeFeedProcessorContext context,
            IReadOnlyCollection<JObject> input,
            CancellationToken cancellationToken)
        {
            using var logScope = _logger.BeginScope("CosmosDbTrigger: AssignClaimAdjudicator");

            await Parallel.ForEachAsync(input, cancellationToken, async (record, token) =>
            {
                try
                {
                    await _customerRepository.UpsertItem(record);
                }
                catch (Exception ex)
                {
                    //Should handle DLQ
                    _logger.LogError(ex.Message, ex);
                }
            });
        }

    }
}
