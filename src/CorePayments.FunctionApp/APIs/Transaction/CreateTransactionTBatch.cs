using Model = CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CorePayments.FunctionApp.APIs.Transaction
{
    public class CreateTransactionTBatch
    {
        readonly ITransactionRepository _transactionRepository;

        public CreateTransactionTBatch(
            ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        [Function("CreateTransactionTBatch")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "transaction/createtbatch")] HttpRequest req,
            FunctionContext context)
        {
            var logger = context.GetLogger<CreateTransactionTBatch>();

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var transaction = JsonConvert.DeserializeObject<Model.Transaction>(requestBody);

                var (account, statusCode, message) = await _transactionRepository.ProcessTransactionTBatch(transaction);

                if (new HttpResponseMessage(statusCode).IsSuccessStatusCode)
                    return new OkObjectResult(account);
                else if (statusCode == HttpStatusCode.PreconditionFailed)
                    return new StatusCodeResult((int)HttpStatusCode.PreconditionFailed);
                else if (statusCode == HttpStatusCode.NotFound)
                    return new NotFoundObjectResult(message);
                else
                    return string.IsNullOrWhiteSpace(message) ? new BadRequestResult() : new BadRequestObjectResult(message);
            }
            catch (CosmosException ex)
            {
                logger.LogError(ex.Message, ex);
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
