using CorePayments.Infrastructure.Domain.Entities;

namespace CorePayments.Infrastructure.Repository
{
    public interface IMemberRepository
    {
        Task CreateItem(Member member);

        Task<(IEnumerable<Member>? members, string? continuationToken)> GetPagedMembers(int pageSize, string continuationToken);

        Task<Member?> GetMember(string memberId);

        Task<int> PatchMember(Member member, string memberId);
    }
}
