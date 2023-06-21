using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CorePayments.FunctionApp.APIs
{
    public class CreateTransactionSProc
    {
        readonly ITransactionRepository _transactionRepository;

        public CreateTransactionSProc(
            ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        [Function("CreateTransactionSProc")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "transaction/createsproc")] HttpRequest req,
            [CosmosDBInput(
                databaseName: "%paymentsDatabase%",
                containerName: "%transactionsContainer%",
                Connection = "CosmosDBConnection")] CosmosClient client,
            FunctionContext context)
        {
            var logger = context.GetLogger<CreateTransactionSProc>();

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var transaction = JsonConvert.DeserializeObject<Transaction>(requestBody);

                var result = await _transactionRepository.ProcessTransactionSProc(transaction);
                return new OkObjectResult(result);
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
