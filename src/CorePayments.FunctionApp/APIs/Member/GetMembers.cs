using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using payments_model;
using System;
using System.Threading.Tasks;
using payments_model.Model;

namespace cosmos_payments_demo.APIs
{
    public static class GetMembers
    {
        [FunctionName("GetMembers")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "members")] HttpRequest req,
            [CosmosDB(
                databaseName: "%paymentsDatabase%",
                containerName: "%memberContainer%",
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
                    Environment.GetEnvironmentVariable("memberContainer"));

            QueryDefinition query = new QueryDefinition("select * from c order by c.lastName desc");

            using (FeedIterator<Member> resultSet = container.GetItemQueryIterator<Member>(
                query,
                continuationToken,
                new QueryRequestOptions()
                {
                    MaxItemCount = pageSize,
                    ResponseContinuationTokenLimitInKb = 1
                }))
            {
                continuationToken = null;

                if (resultSet.HasMoreResults)
                {
                    FeedResponse<Member> response = await resultSet.ReadNextAsync();

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
