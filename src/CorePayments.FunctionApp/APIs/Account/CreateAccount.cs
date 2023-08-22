using System;
using System.IO;
using System.Threading.Tasks;
using CorePayments.FunctionApp.Helpers;
using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
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
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "account")] HttpRequestData req,
            FunctionContext context)
        {
            var logger = context.GetLogger<CreateAccount>();

            try
            {
                //Read request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var account = JsonSerializationHelper.DeserializeItem<AccountSummary>(requestBody);

                account.id = Guid.NewGuid().ToString();

                await _transactionRepository.CreateItem(account);

                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await response.WriteAsJsonAsync(account);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);

                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}
