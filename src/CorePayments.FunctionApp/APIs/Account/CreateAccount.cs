using System;
using System.IO;
using System.Threading.Tasks;
using CorePayments.FunctionApp.Helpers;
using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CorePayments.FunctionApp.APIs.Account
{
    public class CreateAccount
    {
        readonly ITransactionRepository _transactionRepository;

        public CreateAccount(
            ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        [Function("CreateAccount")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "account")] HttpRequest req,
            FunctionContext context)
        {
            var logger = context.GetLogger<CreateAccount>();

            try
            {
                //Read request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var account = JsonSerializationHelper.DeserializeItem<AccountSummary>(requestBody);

                await _transactionRepository.CreateItem(account);

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
