<<<<<<< HEAD:src/CorePayments.FunctionApp/APIs/GetAccountSummary.cs
using CorePayments.Infrastructure.Domain.Entities;
=======
>>>>>>> f482f0adcc278b6f40833ae5755a42e4c19aa5c3:src/cosmos-payments-demo/APIs/Account/GetAccountSummary.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using payments_model;
using System.Threading.Tasks;

namespace CorePayments.FunctionApp.APIs
{
    public class GetAccountSummary
    {
<<<<<<< HEAD:src/CorePayments.FunctionApp/APIs/GetAccountSummary.cs
        [Function("GetAccountSummary")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "account/{accountId}")] HttpRequest req,
            [CosmosDBInput(
=======
        [FunctionName("GetAccountSummary")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "account/{accountId}")] HttpRequest req,
            [CosmosDB(
>>>>>>> f482f0adcc278b6f40833ae5755a42e4c19aa5c3:src/cosmos-payments-demo/APIs/Account/GetAccountSummary.cs
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