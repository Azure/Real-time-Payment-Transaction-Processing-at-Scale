using CorePayments.Infrastructure.Domain.Entities;
using static CorePayments.Infrastructure.Constants;

namespace CorePayments.Infrastructure.Repository
{
    public interface IGlobalIndexRepository
    {
        Task ProcessAccountAssignment(AccountAssignmentOperations operation, string memberId, string accountId);
        Task<IEnumerable<GlobalIndex>> GetAccountsForMember(string memberId);
        Task CreateItem(GlobalIndex globalIndex);
    }
}
