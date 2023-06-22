using CorePayments.Infrastructure.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace CorePayments.Infrastructure.Repository
{
    public class CustomerRepository : CosmosDbRepository, ICustomerRepository
    {
        public CustomerRepository(CosmosClient client) :
            base(client, containerName: Environment.GetEnvironmentVariable("customerContainer") ?? string.Empty)
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

            return await PagedQuery<AccountSummary>(query, pageSize, null, continuationToken);
        }

        public async Task CreateItem(JObject item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            await Container.CreateItemAsync(item);
        }
    }
}