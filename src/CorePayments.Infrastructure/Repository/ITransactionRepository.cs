using CorePayments.Infrastructure.Domain.Entities;
using System.Net;

namespace CorePayments.Infrastructure.Repository
{
    public interface ITransactionRepository
    {
        Task<AccountSummary> ProcessTransactionSProc(Transaction transaction);
        Task<(AccountSummary? accountSummary, HttpStatusCode statusCode, string message)> ProcessTransactionTBatch(Transaction transaction);
        Task CreateItem<T>(T item);
    }
}
