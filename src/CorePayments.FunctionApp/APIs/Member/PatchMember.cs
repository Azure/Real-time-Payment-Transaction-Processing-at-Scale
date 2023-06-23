using CorePayments.FunctionApp.Helpers;
using CorePayments.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Model = CorePayments.Infrastructure.Domain.Entities;

namespace CorePayments.FunctionApp.APIs.Member
{
    public class PatchMember
    {
        readonly IMemberRepository _memberRepository;

        public PatchMember(
            IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        [Function("PatchMember")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "member/{memberId}")] HttpRequestData req,
            string memberId,
            FunctionContext context)
        {
            var logger = context.GetLogger<PatchMember>();

            try
            {
                //Read request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var member = JsonSerializationHelper.DeserializeItem<Model.Member>(requestBody);

                var patchOpsCount = await _memberRepository.PatchMember(member, memberId);

                if (patchOpsCount == 0)
                {
                    var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await response.WriteStringAsync(
                        "No attributes provided.");
                    return response;
                }

                //Return order to caller
                return req.CreateResponse(System.Net.HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);

                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}
