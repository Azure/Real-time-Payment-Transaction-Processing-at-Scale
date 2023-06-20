using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using payments_model;
using System;
using System.Threading.Tasks;

namespace cosmos_payments_demo.APIs
{
    public static class GetTransactionStatement
    {
        [FunctionName("GetTransactionStatement")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "statement/{accountId}")] HttpRequest req,
            string accountId,
            [CosmosDB(
                databaseName: "%paymentsDatabase%",
                containerName: "%customerContainer%",
                PreferredLocations = "%preferredRegions%",
                Connection = "CosmosDBConnection")] CosmosClient client,
            ILogger log)
        {
            _ = int.TryParse(req.Query["pageSize"], out var pageSize);
            if (pageSize <= 0)
            {
                pageSize = 50;
            }

            string continuationToken = req.Query["continuationToken"];

            if (container == null)
                container = client.GetContainer(Environment.GetEnvironmentVariable("paymentsDatabase"),
                    Environment.GetEnvironmentVariable("customerContainer"));

            QueryDefinition query = new QueryDefinition("select * from c where c.accountId = @accountId and c.type != @docType order by c._ts desc")
                .WithParameter("@accountId", accountId)
                .WithParameter("@docType", Constants.DocumentTypes.AccountSummary);

            using (FeedIterator<Transaction> resultSet = container.GetItemQueryIterator<Transaction>(
                query,
                continuationToken,
                new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(accountId),
                    MaxItemCount = pageSize,
                    ResponseContinuationTokenLimitInKb = 1
                }))
            {
                continuationToken = null;

                if (resultSet.HasMoreResults)
                {
                    FeedResponse<Transaction> response = await resultSet.ReadNextAsync();

                    if (response.Count > 0)
                        continuationToken = response.ContinuationToken;

                    return new OkObjectResult(new
                    {
                        page = response.Resource,
                        continuationToken = Uri.EscapeDataString(continuationToken ?? String.Empty)
                    });
                }
                else
                {
                    return new NotFoundResult();
                }
            }
        }

        private static Container container;
    }
}
