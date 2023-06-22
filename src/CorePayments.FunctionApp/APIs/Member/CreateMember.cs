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
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "member")] HttpRequest req,
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
                    return new BadRequestObjectResult(
                        "Invalid member record. Please check the fields and try again.");
                }

                //Return order to caller
                return new AcceptedResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);

                return new BadRequestResult();
            }
        }
    }
}
