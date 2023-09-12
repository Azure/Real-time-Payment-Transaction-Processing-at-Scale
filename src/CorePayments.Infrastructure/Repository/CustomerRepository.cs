using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Domain.Settings;
using CorePayments.Infrastructure.Events;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace CorePayments.Infrastructure.Repository
{
    public class CustomerRepository : CosmosDbRepository, ICustomerRepository
    {
        public CustomerRepository(CosmosClient client, IOptions<DatabaseSettings> options) :
            base(client, containerName: options.Value.CustomerContainer ?? string.Empty, options)
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

        public async Task<IEnumerable<AccountSummary>> GetAccountSummaries(IEnumerable<string> accountSummaryIds)
        {
            QueryDefinition query = new QueryDefinition("select * from c where c.type = @docType and ARRAY_CONTAINS(@accountSummaryIds, c.id) order by c.accountId")
                .WithParameter("@docType", Constants.DocumentTypes.AccountSummary)
                .WithParameter("@accountSummaryIds", accountSummaryIds);

            return await Query<AccountSummary>(query);
        }

        public async Task<AccountSummary?> GetAccountSummary(string accountId)
        {
            var result = await ReadItem<AccountSummary?>(accountId, accountId);
            if (result != null)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public async Task<IEnumerable<AccountSummary>> FindAccountSummary(string searchString)
        {
            if (!searchString.Contains("%"))
            {
                searchString = $"%{searchString}%";
            }
            
            QueryDefinition query = new QueryDefinition("select * from c where c.id like @searchString and c.type = @docType order by c.accountId")
                .WithParameter("@searchString", searchString)
                .WithParameter("@docType", Constants.DocumentTypes.AccountSummary);

            return await Query<AccountSummary>(query);
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