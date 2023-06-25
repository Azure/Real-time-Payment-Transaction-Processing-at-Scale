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
using Microsoft.Azure.Functions.Worker.Http;

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
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "transaction/createtbatch")] HttpRequestData req,
            FunctionContext context)
        {
            var logger = context.GetLogger<CreateTransactionTBatch>();

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var transaction = JsonConvert.DeserializeObject<Model.Transaction>(requestBody);

                var (account, statusCode, message) = await _transactionRepository.ProcessTransactionTBatch(transaction);

                if (new HttpResponseMessage(statusCode).IsSuccessStatusCode)
                {
                    var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                    await response.WriteAsJsonAsync(account);

                    return response;
                }
                else if (statusCode == HttpStatusCode.PreconditionFailed)
                {
                    return req.CreateResponse(HttpStatusCode.PreconditionFailed);
                }
                else if (statusCode == HttpStatusCode.NotFound)
                {
                    var response = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                    await response.WriteStringAsync(message);
                    return response;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    }
                    var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await response.WriteStringAsync(message);
                    return response;
                }
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
