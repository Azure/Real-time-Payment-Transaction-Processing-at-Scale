using CorePayments.Infrastructure.Domain.Entities;
using Newtonsoft.Json.Linq;

namespace CorePayments.Infrastructure.Repository
{
    public interface ICustomerRepository
    {
        Task<(IEnumerable<Transaction>? transactions, string? continuationToken)> GetPagedTransactionStatement(string accountId, int pageSize, string continuationToken);

        Task<(IEnumerable<AccountSummary>? accounts, string? continuationToken)> GetPagedAccountSummary(int pageSize, string continuationToken);

        Task<IEnumerable<AccountSummary>> GetAccountSummaries(IEnumerable<string> accountSummaryIds);

        Task<AccountSummary?> GetAccountSummary(string accountId);

        Task<IEnumerable<AccountSummary>> FindAccountSummary(string searchString);
        
        Task CreateItem(JObject item);

        Task UpsertItem(JObject item);
    }
}
