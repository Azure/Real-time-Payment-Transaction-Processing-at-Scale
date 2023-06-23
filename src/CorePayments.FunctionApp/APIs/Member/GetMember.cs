using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Threading.Tasks;
using Model = CorePayments.Infrastructure.Domain.Entities;

namespace CorePayments.FunctionApp.APIs.Member
{
    public class GetMember
    {
        [Function("GetMember")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "member/{memberId}")] HttpRequestData req,
            [CosmosDBInput(
                databaseName: "%paymentsDatabase%",
                containerName: "%memberContainer%",
                PartitionKey = "{memberId}",
                Id = "{memberId}",
                PreferredLocations = "%preferredRegions%",
                Connection = "CosmosDBConnection")] Model.Member member,
            FunctionContext context)
        {
            var logger = context.GetLogger<FunctionContext>();

            if (member == null)
                return new NotFoundResult();

            return new OkObjectResult(member);
        }
    }
}
