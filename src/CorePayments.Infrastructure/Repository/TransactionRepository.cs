using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Events;
using Microsoft.Azure.Cosmos;
using System.ComponentModel;
using System.Net;

namespace CorePayments.Infrastructure.Repository
{
    public class TransactionRepository : CosmosDbRepository, ITransactionRepository
    {
        public TransactionRepository(CosmosClient client, IEventHubService eventHub) :
            base(client, containerName: Environment.GetEnvironmentVariable("transactionsContainer") ?? string.Empty, eventHub)
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

            var responseRead = await ReadItem<AccountSummary>(transaction.accountId, transaction.accountId);
            var account = responseRead.Resource;

            if (account == null)
            {
                return new(null, HttpStatusCode.NotFound, "Account not found!");
            }

            if (transaction.type.ToLowerInvariant() == Constants.DocumentTypes.TransactionDebit)
            {
                if ((account.balance + account.overdraftLimit) < transaction.amount)
                {
                    return new(null, HttpStatusCode.BadRequest, "Insufficient balance/limit!");
                }
                else
                {
                    account.balance -= transaction.amount;
                }
            }

            var batch = Container.CreateTransactionalBatch(pk);

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
                return new(account, HttpStatusCode.OK, string.Empty);
            }
            else if (responseBatch.StatusCode == HttpStatusCode.PreconditionFailed)
                return new (null, HttpStatusCode.PreconditionFailed, string.Empty);
            else
                return new (null, HttpStatusCode.BadRequest, string.Empty);
        }

        public async Task CreateItem<T>(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            await Container.CreateItemAsync(item);
        }
    }
}
