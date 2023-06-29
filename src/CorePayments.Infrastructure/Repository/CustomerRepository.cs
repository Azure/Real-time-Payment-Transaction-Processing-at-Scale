using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Events;
using Microsoft.Azure.Cosmos;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace CorePayments.Infrastructure.Repository
{
    public class CustomerRepository : CosmosDbRepository, ICustomerRepository
    {
        public CustomerRepository(CosmosClient client, IEventHubService eventHub) :
            base(client, containerName: Environment.GetEnvironmentVariable("customerContainer") ?? string.Empty, eventHub)
        {
        }

        public async Task<(IEnumerable<Transaction>? transactions, string? continuationToken)> GetPagedTransactionStatement(string accountId, int pageSize, string continuationToken)
        {
            QueryDefinition query = new QueryDefinition("select * from c where c.accountId = @accountId and c.type != @docType order by c._ts desc")
                .WithParameter("@accountId", accountId)
                .WithParameter("@docType", Constants.DocumentTypes.AccountSummary);

            return await PagedQuery<Transaction>(query, pageSize, new PartitionKey(accountId), continuationToken);
        }

        public async Task<(IEnumerable<AccountSummary>? accounts, string? continuationToken)> GetPagedAccountSummary(int pageSize, string continuationToken)
        {
            QueryDefinition query = new QueryDefinition("select * from c where c.type = @docType order by c.accountId")
                .WithParameter("@docType", Constants.DocumentTypes.AccountSummary);

            await TriggerTrackingEvent($"Retrieving paged account summary with page size {pageSize}.");

            return await PagedQuery<AccountSummary>(query, pageSize, null, continuationToken);
        }

        public async Task CreateItem(JObject item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            await Container.CreateItemAsync(item);
        }

        public async Task UpsertItem(JObject item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            await Container.UpsertItemAsync(item);
        }
    }
}