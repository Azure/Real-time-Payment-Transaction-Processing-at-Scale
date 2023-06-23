using Azure;
using CorePayments.FunctionApp.Models.Response;
using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model = CorePayments.Infrastructure.Domain.Entities;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;

namespace CorePayments.FunctionApp.APIs.Transaction
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "statement/{accountId}")] HttpRequestData req,
            string accountId,
            FunctionContext context)
        {
            var logger = context.GetLogger<GetTransactionStatement>();
            int pageSize = -1;
            int.TryParse(req.Query["pageSize"], out pageSize);
            if (pageSize <= 0)
            {
                pageSize = 50;
            }

            string continuationToken = req.Query["continuationToken"];

            var (transactions, newContinuationToken) = await _customerRepository.GetPagedTransactionStatement(accountId, pageSize, continuationToken);
            return transactions == null
                ? new NotFoundResult()
                : new OkObjectResult(new PagedResponse<Model.Transaction>
                {
                    Page = transactions,
                    ContinuationToken = Uri.EscapeDataString(newContinuationToken ?? String.Empty)
                });
        }
    }
}
