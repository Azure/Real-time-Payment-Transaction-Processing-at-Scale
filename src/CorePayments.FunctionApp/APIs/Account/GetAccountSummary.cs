using CorePayments.Infrastructure.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Threading.Tasks;

namespace CorePayments.FunctionApp.APIs.Account
{
    public class GetAccountSummary
    {
        [Function("GetAccountSummary")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "account/{accountId}")] HttpRequestData req,
            [CosmosDBInput(
                databaseName: "%paymentsDatabase%",
                containerName: "%customerContainer%",
                PartitionKey = "{accountId}",
                Id = "{accountId}",
                PreferredLocations = "%preferredRegions%",
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