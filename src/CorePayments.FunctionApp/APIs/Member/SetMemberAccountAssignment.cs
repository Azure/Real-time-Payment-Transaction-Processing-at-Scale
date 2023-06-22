using CorePayments.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static CorePayments.Infrastructure.Constants;

namespace CorePayments.FunctionApp.APIs.Member
{
    public class SetMemberAccountAssignment
    {
        readonly IGlobalIndexRepository _globalIndexRepository;

        public SetMemberAccountAssignment(
            IGlobalIndexRepository globalIndexRepository)
        {
            _globalIndexRepository = globalIndexRepository;
        }

        [Function("AddAccountToMember")]
        public async Task<IActionResult> AddAccountToMember(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "member/{memberId}/accounts/add/{accountId}")]
            HttpRequest req,
            string memberId,
            string accountId,
            FunctionContext context)
        {
            var logger = context.GetLogger<SetMemberAccountAssignment>();

            try
            {
                await _globalIndexRepository.ProcessAccountAssignment(AccountAssignmentOperations.Add, memberId, accountId);
                
                return new OkResult();
            }
            catch (CosmosException ex)
            {
                logger.LogError(ex.Message, ex);
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        [Function("RemoveAccountFromMember")]
        public async Task<IActionResult> RemoveAccountFromMember(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "member/{memberId}/accounts/remove/{accountId}")]
            HttpRequest req,
            string memberId,
            string accountId,
            FunctionContext context)
        {
            var logger = context.GetLogger<SetMemberAccountAssignment>();

            try
            {
                await _globalIndexRepository.ProcessAccountAssignment(AccountAssignmentOperations.Remove, memberId, accountId);

                return new OkResult();
            }
            catch (CosmosException ex)
            {
                logger.LogError(ex.Message, ex);
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
