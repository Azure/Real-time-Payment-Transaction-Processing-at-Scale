using CorePayments.FunctionApp.Models.Response;
using CorePayments.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using Model = CorePayments.Infrastructure.Domain.Entities;

namespace CorePayments.FunctionApp.APIs.Member
{
    public class GetMembers
    {
        readonly IMemberRepository _memberRepository;

        public GetMembers(
            IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        [Function("GetMembers")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "members")] HttpRequest req,
            FunctionContext context)
        {
            var logger = context.GetLogger<GetMembers>();
            int.TryParse(req.Query["pageSize"], out var pageSize);
            if (pageSize <= 0)
            {
                pageSize = 50;
            }

            string continuationToken = req.Query["continuationToken"];

            var (members, newContinuationToken) = await _memberRepository.GetPagedMembers(pageSize, continuationToken);
            return members == null
                ? new NotFoundResult()
                : new OkObjectResult(new PagedResponse<Model.Member>
                {
                    Page = members,
                    ContinuationToken = Uri.EscapeDataString(newContinuationToken ?? String.Empty)
                });
        }
    }
}
