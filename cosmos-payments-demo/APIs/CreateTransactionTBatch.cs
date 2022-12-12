using cosmos_payments_demo.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Container = Microsoft.Azure.Cosmos.Container;

namespace cosmos_payments_demo.APIs
{
    public static class CreateTransactionTBatch
    {
        [FunctionName("CreateTransactionTBatch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "transaction/createtbatch")] HttpRequest req,
            [CosmosDB(
                databaseName: "%paymentsDatabase%",
                containerName: "%transactionsContainer%",
                Connection = "CosmosDBConnection")] CosmosClient client,
            ILogger log)
        {
            try
            {
                if (container == null)
                    container = client.GetContainer(Environment.GetEnvironmentVariable("paymentsDatabase"),
                        Environment.GetEnvironmentVariable("transactionsContainer"));

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var transaction = JsonConvert.DeserializeObject<Transaction>(requestBody);

                var response = await ProcessTransaction(transaction);

                //Should handle/retry precondition failure

                return response;
            }
            catch (CosmosException ex)
            {
                log.LogError(ex.Message, ex);
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        private static Container container;

        private static async Task<IActionResult> ProcessTransaction(Transaction transaction)
        {
            var pk = new PartitionKey(transaction.accountId);

            var responseRead = await container.ReadItemAsync<AccountSummary>(transaction.accountId, pk);
            var account = responseRead.Resource;

            if (account == null)
            {
                return new NotFoundObjectResult("Account not found!");
            }

            if (transaction.type.ToLowerInvariant() == "debit")
            {
                if ((account.balance + account.limit) < transaction.amount)
                {
                    return new BadRequestObjectResult("Insufficient balance/limit!");
                }
                else
                {
                    account.balance -= transaction.amount;
                }
            }
            else if (transaction.type.ToLowerInvariant() == "deposit")
            {
                account.balance += transaction.amount;
            }

            var batch = container.CreateTransactionalBatch(pk);

            batch.UpsertItem<AccountSummary>(account, new TransactionalBatchItemRequestOptions() { IfMatchEtag = responseRead.ETag });
            batch.CreateItem<Transaction>(transaction);

            var responseBatch = await batch.ExecuteAsync();

            if (responseBatch.IsSuccessStatusCode)
            {
                account = responseBatch.GetOperationResultAtIndex<AccountSummary>(0).Resource;
                return new OkObjectResult(account);
            }
            else if (responseBatch.StatusCode == HttpStatusCode.PreconditionFailed)
                return new StatusCodeResult((int)HttpStatusCode.PreconditionFailed);
            else
                return new BadRequestResult();
        }
    }
}
