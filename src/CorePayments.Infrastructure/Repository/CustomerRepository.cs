using CorePayments.Infrastructure.Domain.Entities;
using Microsoft.Azure.Cosmos;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Net;

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
            QueryDefinition query = new QueryDefinition("select * from c where c.accountId = @accountId and c.type != \"accountSummary\" order by c._ts desc")
                .WithParameter("@accountId", accountId);

            return await PagedQuery<Transaction>(query, pageSize, new PartitionKey(accountId), continuationToken);

            /*
            
            QueryDefinition query = new QueryDefinition("select * from c where c.accountId = @accountId and c.type != @docType order by c._ts desc")
                .WithParameter("@accountId", accountId)
                .WithParameter("@docType", Constants.DocumentTypes.AccountSummary);
            */
        }
    }
}