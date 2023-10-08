using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Domain.Settings;
using CorePayments.Infrastructure.Events;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Drawing.Printing;

namespace CorePayments.Infrastructure.Repository
{
    public class MemberRepository : CosmosDbRepository, IMemberRepository
    {
        public MemberRepository(CosmosClient client, IOptions<DatabaseSettings> options) :
            base(client, containerName: options.Value.MemberContainer ?? string.Empty, options)
        {
        }

        public async Task CreateItem(Member member)
        {
            await Container.CreateItemAsync(member);
        }

        public async Task<(IEnumerable<Member>? members, string? continuationToken)> GetPagedMembers(int pageSize, string continuationToken)
        {
            QueryDefinition query = new QueryDefinition("select * from c order by c._ts desc");

            return await PagedQuery<Member>(query, pageSize, null, continuationToken);
        }

        public async Task<Member?> GetMember(string memberId)
        {
            var result = await ReadItem<Member?>(memberId, memberId);
            if (result != null)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public async Task<int> PatchMember(Member member, string memberId)
        {
            JObject obj = JObject.FromObject(member);

            var ops = new List<PatchOperation>();

            foreach (JToken item in obj.Values())
            {
                if (item.Path is "id" or "memberId" or "type" || string.IsNullOrEmpty(item.ToString()))
                    continue;

                ops.Add(PatchOperation.Add($"/{item.Path}", item.ToString()));
            }

            if (ops.Count == 0)
                return 0;

            var response = await Container.PatchItemAsync<Member>(memberId, new PartitionKey(memberId), ops);

            return ops.Count;
        }
    }
}
