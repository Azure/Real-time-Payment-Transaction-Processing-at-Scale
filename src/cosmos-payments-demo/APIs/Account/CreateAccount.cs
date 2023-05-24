using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using payments_model;

namespace cosmos_payments_demo.APIs
{
    public static class CreateAccount
    {
        [FunctionName("CreateAccount")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "account")] HttpRequest req,
            [CosmosDB(
                databaseName: "%paymentsDatabase%",
                containerName: "%transactionsContainer%",
                PreferredLocations = "%preferredRegions%",
                Connection = "CosmosDBConnection")] IAsyncCollector<AccountSummary> collector,
            ILogger log)
        {
            try
            {
                //Read request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var account = JsonConvert.DeserializeObject<AccountSummary>(requestBody);

                //Post account to Cosmos DB using output binding
                await collector.AddAsync(account);

                //Return order to caller
                return new AcceptedResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);

                return new BadRequestResult();
            }
        }
    }
}
