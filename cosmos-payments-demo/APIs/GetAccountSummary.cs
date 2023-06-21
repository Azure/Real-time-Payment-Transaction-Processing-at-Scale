using CorePayments.Infrastructure.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace cosmos_payments_demo.APIs
{
    public static class GetAccountSummary
    {
        [FunctionName("GetAccountSummary")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "account/{accountId}")] HttpRequest req,
            [CosmosDB(
                databaseName: "%paymentsDatabase%",
                containerName: "%customerContainer%",
                PartitionKey = "{accountId}",
                Id = "{accountId}",
                Connection = "CosmosDBConnection")] AccountSummary account,
            ILogger log)
        {
            if (account == null)
                return new NotFoundResult();

            return new OkObjectResult(account);
        }
    }
}