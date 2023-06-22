using static CorePayments.Infrastructure.Constants;

namespace CorePayments.Infrastructure.Repository
{
    public interface IGlobalIndexRepository
    {
        Task ProcessAccountAssignment(AccountAssignmentOperations operation, string memberId, string accountId);
    }
}
