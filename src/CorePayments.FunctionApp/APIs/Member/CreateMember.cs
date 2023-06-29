using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using CorePayments.Infrastructure.Repository;
using Model = CorePayments.Infrastructure.Domain.Entities;
using CorePayments.FunctionApp.Helpers;
using Microsoft.Azure.Functions.Worker.Http;

namespace CorePayments.FunctionApp.APIs.Member
{
    public class CreateMember
    {
        readonly IMemberRepository _memberRepository;

        public CreateMember(
            IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        [Function("CreateMember")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "member")] HttpRequestData req,
            FunctionContext context)
        {
            var logger = context.GetLogger<CreateMember>();
            try
            {
                //Read request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var member = JsonSerializationHelper.DeserializeItem<Model.Member>(requestBody);
                if (member != null)
                {
                    member.memberId = Guid.NewGuid().ToString();

                    await _memberRepository.CreateItem(member);
                }
                else
                {
                    var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await response.WriteStringAsync(
                        "Invalid member record. Please check the fields and try again.");
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
