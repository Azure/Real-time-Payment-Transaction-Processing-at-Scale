using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using payments_model;
using System;
using System.IO;
using System.Threading.Tasks;
using Container = Microsoft.Azure.Cosmos.Container;

namespace cosmos_payments_demo.APIs
{
    public static class CreateTransactionSProc
    {
        //[FunctionName("CreateTransactionSProc")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "transaction/createsproc")] HttpRequest req,
            [CosmosDB(
                databaseName: "%paymentsDatabase%",
                containerName: "%transactionsContainer%",
                PreferredLocations = "%preferredRegions%",
                Connection = "CosmosDBConnection")] CosmosClient client,
            ILogger log)
        {
            try
            {
                if (container == null)
                    container = client.GetContainer(Environment.GetEnvironmentVariable("paymentsDatabase"),
                        Environment.GetEnvironmentVariable("transactionsContainer"));

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var transaction = JsonConvert.DeserializeObject<Transaction>(requestBody);

                var response = await container.Scripts.ExecuteStoredProcedureAsync<AccountSummary>("processTransaction", new PartitionKey(transaction.accountId), new[] { transaction });

                //Should handle/retry precondition failure

                return new OkObjectResult(response.Resource);
            }
            catch (CosmosException ex)
            {
                log.LogError(ex.Message, ex);
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        private static Container container;
    }
}
