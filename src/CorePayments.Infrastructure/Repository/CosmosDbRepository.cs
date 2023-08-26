using CorePayments.Infrastructure.Domain.Settings;
using CorePayments.Infrastructure.Events;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Bson;
using System.Drawing.Printing;
using System.Net;

namespace CorePayments.Infrastructure.Repository
{
    public class CosmosDbRepository
    {
        private readonly CosmosClient _client;
        private readonly Database _database;

        protected Container Container { get; }

        public CosmosDbRepository(CosmosClient client, string containerName, IOptions<DatabaseSettings> options)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentNullException(nameof(containerName));

            _client = client;
            _database = _client.GetDatabase(options.Value.PaymentsDatabase);

            Container = _database.GetContainer(containerName);
            
        }

        protected async Task<IEnumerable<TEntity>> Query<TEntity>(QueryDefinition queryDefinition, PartitionKey? partitionKey = null) where TEntity : new()
        {
            var resultList = new List<TEntity>();
            var resultIterator = Container.GetItemQueryIterator<TEntity>(
                queryDefinition,
                requestOptions: new QueryRequestOptions { PartitionKey = partitionKey });

            while (resultIterator.HasMoreResults)
            {
                var response = await resultIterator.ReadNextAsync();
                resultList.AddRange(response);
            }

            return resultList;
        }

        protected async Task<(IEnumerable<TEntity>?, string?)> PagedQuery<TEntity>(
            QueryDefinition queryDefinition, 
            int pageSize, 
            PartitionKey? partitionKey = null, 
            string? continuationToken = null
        ) where TEntity : new()
        {
            var resultIterator = Container.GetItemQueryIterator<TEntity>(
                queryDefinition,
                continuationToken,
                new QueryRequestOptions()
                {
                    PartitionKey = partitionKey,
                    MaxItemCount = pageSize,
                    ResponseContinuationTokenLimitInKb = 1
                });


            string? newContinuationToken = null;

            if (resultIterator.HasMoreResults)
            {
                var response = await resultIterator.ReadNextAsync();

                if (response.Count > 0)
                    newContinuationToken = response.ContinuationToken;

                return new(response.Resource, newContinuationToken);
            }
            else
            {
                return new(null, newContinuationToken);
            }

        }

        protected async Task<ItemResponse<TEntity>?> ReadItem<TEntity>(string partitionKey, string itemId) where TEntity : new()
        {
            try
            {
                return await Container.ReadItemAsync<TEntity>(itemId, new PartitionKey(partitionKey));
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }
        }
    }
}
