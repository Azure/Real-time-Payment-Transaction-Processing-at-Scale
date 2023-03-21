using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using payments_model.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Container = Microsoft.Azure.Cosmos.Container;

namespace cosmos_payments_demo.APIs
{
    public static class PatchMember
    {
        [FunctionName("PatchMember")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "member/{memberId}")] HttpRequest req,
            [CosmosDB(
                databaseName: "%paymentsDatabase%",
                containerName: "%memberContainer%",
                PreferredLocations = "%preferredRegions%",
                Connection = "CosmosDBConnection")] CosmosClient client,
            ILogger log)
        {
            try
            {
                if (container == null)
                    container = client.GetContainer(Environment.GetEnvironmentVariable("paymentsDatabase"),
                        Environment.GetEnvironmentVariable("memberContainer"));

                //Read request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var member = JsonConvert.DeserializeObject<Member>(requestBody);
                JObject obj = JObject.FromObject(member);

                var ops = new List<PatchOperation>();

                foreach (JToken item in obj.Values())
                {
                    if (item.Path == "id" || item.Path == "memberId" || string.IsNullOrEmpty(item.ToString()))
                        continue;

                    ops.Add(PatchOperation.Add($"/{item.Path}", item.ToString()));
                }

                await container.PatchItemAsync<Member>(member.id, new PartitionKey(member.memberId), ops);

                //Return order to caller
                return new AcceptedResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);

                return new BadRequestResult();
            }
        }

        private static Container container;
    }
}
