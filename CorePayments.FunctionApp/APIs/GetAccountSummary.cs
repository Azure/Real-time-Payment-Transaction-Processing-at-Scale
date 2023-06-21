using CorePayments.Infrastructure.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CorePayments.FunctionApp.APIs
{
    public class GetAccountSummary
    {
        [Function("GetAccountSummary")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "account/{accountId}")] HttpRequest req,
            [CosmosDBInput(
                databaseName: "%paymentsDatabase%",
                containerName: "%customerContainer%",
                PartitionKey = "{accountId}",
                Id = "{accountId}",
                Connection = "CosmosDBConnection")] AccountSummary account,
            FunctionContext context)
        {
            var logger = context.GetLogger<AccountSummary>();

            if (account == null)
                return new NotFoundResult();

            return new OkObjectResult(account);
        }
    }
}