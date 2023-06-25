using CorePayments.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
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
        public async Task<HttpResponseData> AddAccountToMember(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "member/{memberId}/accounts/add/{accountId}")]
            HttpRequestData req,
            string memberId,
            string accountId,
            FunctionContext context)
        {
            var logger = context.GetLogger<SetMemberAccountAssignment>();

            try
            {
                await _globalIndexRepository.ProcessAccountAssignment(AccountAssignmentOperations.Add, memberId, accountId);
                
                return req.CreateResponse(System.Net.HttpStatusCode.OK);
            }
            catch (CosmosException ex)
            {
                logger.LogError(ex.Message, ex);
                var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await response.WriteStringAsync(ex.Message);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await response.WriteStringAsync(ex.Message);
                return response;
            }
        }

        [Function("RemoveAccountFromMember")]
        public async Task<HttpResponseData> RemoveAccountFromMember(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "member/{memberId}/accounts/remove/{accountId}")]
            HttpRequestData req,
            string memberId,
            string accountId,
            FunctionContext context)
        {
            var logger = context.GetLogger<SetMemberAccountAssignment>();

            try
            {
                await _globalIndexRepository.ProcessAccountAssignment(AccountAssignmentOperations.Remove, memberId, accountId);

                return req.CreateResponse(System.Net.HttpStatusCode.OK);
            }
            catch (CosmosException ex)
            {
                logger.LogError(ex.Message, ex);
                var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await response.WriteStringAsync(ex.Message);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await response.WriteStringAsync(ex.Message);
                return response;
            }
        }
    }
}
