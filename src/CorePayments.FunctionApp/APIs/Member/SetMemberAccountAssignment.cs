using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using payments_model.Model;
using Container = Microsoft.Azure.Cosmos.Container;
using static payments_model.Constants;

namespace cosmos_payments_demo.APIs
{
    public static class SetMemberAccountAssignment
    {
        [FunctionName("AddAccountToMember")]
        public static async Task<IActionResult> AddAccountToMember(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "member/{memberId}/accounts/add/{accountId}")]
            HttpRequest req,
            string memberId,
            string accountId,
            [CosmosDB(
                databaseName: "%paymentsDatabase%",
                containerName: "%globalIndexContainer%",
                PreferredLocations = "%preferredRegions%",
                Connection = "CosmosDBConnection")]
            CosmosClient client,
            ILogger log)
        {
            try
            {
                globalIndexContainer ??= client.GetContainer(
                    Environment.GetEnvironmentVariable("paymentsDatabase"),
                    Environment.GetEnvironmentVariable("globalIndexContainer"));

                var response = await ProcessAccountAssignment(AccountAssignmentOperations.Add, memberId, accountId);
                
                return response;
            }
            catch (CosmosException ex)
            {
                log.LogError(ex.Message, ex);
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        [FunctionName("RemoveAccountFromMember")]
        public static async Task<IActionResult> RemoveAccountFromMember(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "member/{memberId}/accounts/remove/{accountId}")]
            HttpRequest req,
            string memberId,
            string accountId,
            [CosmosDB(
                databaseName: "%paymentsDatabase%",
                containerName: "%globalIndexContainer%",
                PreferredLocations = "%preferredRegions%",
                Connection = "CosmosDBConnection")]
            CosmosClient client,
            ILogger log)
        {
            try
            {
                globalIndexContainer ??= client.GetContainer(
                    Environment.GetEnvironmentVariable("paymentsDatabase"),
                    Environment.GetEnvironmentVariable("globalIndexContainer"));

                var response = await ProcessAccountAssignment(AccountAssignmentOperations.Remove, memberId, accountId);

                return response;
            }
            catch (CosmosException ex)
            {
                log.LogError(ex.Message, ex);
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        private static Container globalIndexContainer;

        private static async Task<IActionResult> ProcessAccountAssignment(AccountAssignmentOperations operation, string memberId, string accountId)
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
                await globalIndexContainer.CreateItemAsync(globalIndexMemberAccount, new PartitionKey(globalIndexMemberAccount.partitionKey));
                await globalIndexContainer.CreateItemAsync(globalIndexAccountMember, new PartitionKey(globalIndexAccountMember.partitionKey));
                return new OkResult();
            }
            
            // Perform a point read to retrieve the global index document for the Member Account record if it exists:
            var pk = new PartitionKey(memberId);
            var responseReadGlobalIndex = await globalIndexContainer.ReadItemAsync<GlobalIndex>(accountId, pk);
            var globalIndexMemberAccountToDelete = responseReadGlobalIndex.Resource;

            // Perform a point read to retrieve the global index document for the Account Member record if it exists:
            pk = new PartitionKey(accountId);
            responseReadGlobalIndex = await globalIndexContainer.ReadItemAsync<GlobalIndex>(memberId, pk);
            var globalIndexAccountMemberToDelete = responseReadGlobalIndex.Resource;

            // Delete the global index records.
            if (globalIndexMemberAccountToDelete != null)
            {
                await globalIndexContainer.DeleteItemAsync<GlobalIndex>(globalIndexMemberAccountToDelete.id,
                    new PartitionKey(globalIndexMemberAccountToDelete.partitionKey));
            }
            if (globalIndexAccountMemberToDelete != null)
            {
                await globalIndexContainer.DeleteItemAsync<GlobalIndex>(globalIndexAccountMemberToDelete.id,
                    new PartitionKey(globalIndexAccountMemberToDelete.partitionKey));
            }

            return new OkResult();
        }
    }
}
