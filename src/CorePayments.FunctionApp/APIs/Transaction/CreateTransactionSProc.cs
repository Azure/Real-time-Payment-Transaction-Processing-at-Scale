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
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;

namespace CorePayments.FunctionApp.APIs.Transaction
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
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "transaction/createsproc")] HttpRequestData req,
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
                var transaction = JsonConvert.DeserializeObject<Model.Transaction>(requestBody);

                var result = await _transactionRepository.ProcessTransactionSProc(transaction);
                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await response.WriteAsJsonAsync(result);

                return response;
            }
            catch (CosmosException ex)
            {
                logger.LogError(ex.Message, ex);
                var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await response.WriteStringAsync(ex.Message);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await response.WriteStringAsync(ex.Message);
                return response;
            }
        }
    }
}
