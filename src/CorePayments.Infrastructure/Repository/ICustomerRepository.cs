using CorePayments.Infrastructure.Domain.Entities;

namespace CorePayments.Infrastructure.Repository
{
    public interface ICustomerRepository
    {
        Task<(IEnumerable<Transaction>? transactions, string? continuationToken)> GetPagedTransactionStatement(string accountId, int pageSize, string continuationToken);
    }
}
