using Azure;
using CorePayments.FunctionApp.Models.Response;
using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CorePayments.FunctionApp.APIs
{
    public class GetTransactionStatement
    {
        readonly ICustomerRepository _customerRepository;

        public GetTransactionStatement(
            ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        [Function("GetTransactionStatement")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "statement/{accountId}")] HttpRequest req,
            string accountId,
            FunctionContext context)
        {
            var logger = context.GetLogger<GetTransactionStatement>();
            int pageSize = -1;
            int.TryParse(req.Query["pageSize"], out pageSize);

            string continuationToken = req.Query["continuationToken"];

            var (transactions, newContinuationToken) = await _customerRepository.GetPagedTransactionStatement(accountId, pageSize, continuationToken);
            return transactions == null
                ? new NotFoundResult()
                : new OkObjectResult(new PagedTransactionsResponse
                {
                    Page = transactions,
                    ContinuationToken = Uri.EscapeDataString(newContinuationToken ?? String.Empty)
                });
        }
    }
}
