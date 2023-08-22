using CorePayments.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using static CorePayments.Infrastructure.Constants;

namespace CorePayments.FunctionApp.APIs.Member
{
    public class GetMemberAccounts
    {
        readonly IGlobalIndexRepository _globalIndexRepository;
        private readonly ICustomerRepository _customerRepository;

        public GetMemberAccounts(
            IGlobalIndexRepository globalIndexRepository,
            ICustomerRepository customerRepository)
        {
            _globalIndexRepository = globalIndexRepository;
            _customerRepository = customerRepository;
        }

        [Function("GetMemberAccounts")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "member/{memberId}/accounts")]
            HttpRequestData req,
            string memberId,
            FunctionContext context)
        {
            var logger = context.GetLogger<GetMemberAccounts>();

            try
            {
                // Perform a lookup using the global index:
                var accountsForMember = await _globalIndexRepository.GetAccountsForMember(memberId);

                var accounts = await _customerRepository.GetAccountSummaries(accountsForMember.Select(x => x.id));

                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await response.WriteAsJsonAsync(accounts);
                return response;
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
