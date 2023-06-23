using CorePayments.FunctionApp.Models.Response;
using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Threading.Tasks;

namespace CorePayments.FunctionApp.APIs.Account
{
    public class GetAccounts
    {
        readonly ICustomerRepository _customerRepository;

        public GetAccounts(
            ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        [Function("GetAccounts")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "accounts")] HttpRequestData req,
            FunctionContext context)
        {
            int.TryParse(req.Query["pageSize"], out var pageSize);
            if (pageSize <= 0)
            {
                pageSize = 50;
            }

            string continuationToken = req.Query["continuationToken"];

            var (accounts, newContinuationToken) = await _customerRepository.GetPagedAccountSummary(pageSize, continuationToken);
            return accounts == null
                ? new NotFoundResult()
                : new OkObjectResult(new PagedResponse<AccountSummary>
                {
                    Page = accounts,
                    ContinuationToken = Uri.EscapeDataString(newContinuationToken ?? String.Empty)
                });
        }
    }
}
