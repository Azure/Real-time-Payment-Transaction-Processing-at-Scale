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
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "account/{accountId}")] HttpRequestData req,
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
                return req.CreateResponse(System.Net.HttpStatusCode.NotFound);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(account);

            return response;
        }
    }
}