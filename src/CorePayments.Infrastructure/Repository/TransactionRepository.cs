using CorePayments.Infrastructure.Domain.Entities;
using Microsoft.Azure.Cosmos;
using System.ComponentModel;
using System.Net;

namespace CorePayments.Infrastructure.Repository
{
    public class TransactionRepository : CosmosDbRepository, ITransactionRepository
    {
        public TransactionRepository(CosmosClient client) :
            base(client, containerName: Environment.GetEnvironmentVariable("transactionsContainer") ?? string.Empty)
        {
        }

        public async Task<AccountSummary> ProcessTransactionSProc(Transaction transaction)
        {
            var response = await Container.Scripts.ExecuteStoredProcedureAsync<AccountSummary>("processTransaction", new PartitionKey(transaction.accountId), new[] { transaction });

            //Should handle/retry precondition failure

            return response.Resource;
        }

        public async Task<(AccountSummary? accountSummary, HttpStatusCode statusCode, string message)> ProcessTransactionTBatch(Transaction transaction)
        {
            var pk = new PartitionKey(transaction.accountId);

            var response = await ReadItem<AccountSummary>(transaction.accountId, transaction.accountId);
            var account = response.Resource;

            if (account == null)
            {
                return new(null, HttpStatusCode.NotFound, "Account not found!");
            }

            if (transaction.type.ToLowerInvariant() == "debit")
            {
                if ((account.balance + account.limit) < transaction.amount)
                {
                    return new(null, HttpStatusCode.BadRequest, "Insufficient balance/limit!");
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

            var batch = Container.CreateTransactionalBatch(pk);

            batch.UpsertItem<AccountSummary>(account, new TransactionalBatchItemRequestOptions() { IfMatchEtag = response.ETag });
            batch.CreateItem<Transaction>(transaction);

            var responseBatch = await batch.ExecuteAsync();

            if (responseBatch.IsSuccessStatusCode)
            {
                account = responseBatch.GetOperationResultAtIndex<AccountSummary>(0).Resource;
                return new(account, HttpStatusCode.OK, string.Empty);
            }
            else if (responseBatch.StatusCode == HttpStatusCode.PreconditionFailed)
                return new (null, HttpStatusCode.PreconditionFailed, string.Empty);
            else
                return new (null, HttpStatusCode.BadRequest, string.Empty);
        }
    }

    /*
    private static async Task<IActionResult> ProcessTransaction(Transaction transaction)
        {
            var pk = new PartitionKey(transaction.accountId);

            var responseRead = await container.ReadItemAsync<AccountSummary>(transaction.accountId, pk);
            var account = responseRead.Resource;

            if (account == null)
            {
                return new NotFoundObjectResult("Account not found!");
            }

            if (transaction.type.ToLowerInvariant() == Constants.DocumentTypes.TransactionDebit)
            {
                if ((account.balance + account.overdraftLimit) < transaction.amount)
                {
                    return new BadRequestObjectResult("Insufficient balance/limit!");
                }
            }

            var batch = container.CreateTransactionalBatch(pk);

            batch.PatchItem(account.id, 
                new List<PatchOperation>() 
                { 
                    PatchOperation.Increment("/balance", transaction.type.ToLowerInvariant() == Constants.DocumentTypes.TransactionDebit ? -transaction.amount : transaction.amount) 
                }, 
                new TransactionalBatchPatchItemRequestOptions()
                { 
                    IfMatchEtag = responseRead.ETag 
                }
            );
            
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
    */
}
