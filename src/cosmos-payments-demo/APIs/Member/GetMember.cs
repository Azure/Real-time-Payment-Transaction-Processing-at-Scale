using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using payments_model.Model;
using System.Threading.Tasks;

namespace cosmos_payments_demo.APIs
{
    public static class GetMember
    {
        [FunctionName("GetMember")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "member/{memberId}")] HttpRequest req,
            [CosmosDB(
                databaseName: "%paymentsDatabase%",
                containerName: "%memberContainer%",
                PartitionKey = "{memberId}",
                Id = "{memberId}",
                PreferredLocations = "%preferredRegions%",
                Connection = "CosmosDBConnection")] Member member,
            ILogger log)
        {
            if (member == null)
                return new NotFoundResult();

            return new OkObjectResult(member);
        }
    }
}
