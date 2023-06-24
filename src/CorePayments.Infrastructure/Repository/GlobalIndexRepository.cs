using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Events;
using Microsoft.Azure.Cosmos;
using static CorePayments.Infrastructure.Constants;

namespace CorePayments.Infrastructure.Repository
{
    public class GlobalIndexRepository : CosmosDbRepository, IGlobalIndexRepository
    {
        public GlobalIndexRepository(CosmosClient client, IEventHubService eventHub) :
            base(client, containerName: Environment.GetEnvironmentVariable("globalIndexContainer") ?? string.Empty, eventHub)
        {
        }

        public async Task ProcessAccountAssignment(AccountAssignmentOperations operation, string memberId, string accountId)
        {
            if (operation == AccountAssignmentOperations.Add)
            {
                // Create the global index document for the Member Account record:
                var globalIndexMemberAccount = new GlobalIndex
                {
                    id = accountId,
                    partitionKey = memberId,
                    targetDocType = DocumentTypes.AccountSummary
                };
                // Create the global index document for the Account Member record:
                var globalIndexAccountMember = new GlobalIndex
                {
                    id = memberId,
                    partitionKey = accountId,
                    targetDocType = DocumentTypes.Member
                };

                // Cannot do a batch operation because the primary keys are different.
                await Container.CreateItemAsync(globalIndexMemberAccount, new PartitionKey(globalIndexMemberAccount.partitionKey));
                await Container.CreateItemAsync(globalIndexAccountMember, new PartitionKey(globalIndexAccountMember.partitionKey));
                return;
            }

            // Perform a point read to retrieve the global index document for the Member Account record if it exists:
            var pk = new PartitionKey(memberId);
            var responseReadGlobalIndex = await Container.ReadItemAsync<GlobalIndex>(accountId, pk);
            var globalIndexMemberAccountToDelete = responseReadGlobalIndex.Resource;

            // Perform a point read to retrieve the global index document for the Account Member record if it exists:
            pk = new PartitionKey(accountId);
            responseReadGlobalIndex = await Container.ReadItemAsync<GlobalIndex>(memberId, pk);
            var globalIndexAccountMemberToDelete = responseReadGlobalIndex.Resource;

            // Delete the global index records.
            if (globalIndexMemberAccountToDelete != null)
            {
                await Container.DeleteItemAsync<GlobalIndex>(globalIndexMemberAccountToDelete.id,
                    new PartitionKey(globalIndexMemberAccountToDelete.partitionKey));
            }
            if (globalIndexAccountMemberToDelete != null)
            {
                await Container.DeleteItemAsync<GlobalIndex>(globalIndexAccountMemberToDelete.id,
                    new PartitionKey(globalIndexAccountMemberToDelete.partitionKey));
            }
        }
    }
}