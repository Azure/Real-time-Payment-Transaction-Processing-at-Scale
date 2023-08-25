using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Repository;
using CorePayments.WebAPI.Components;
using CorePayments.WebAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Reflection;
using CorePayments.Infrastructure;
using Microsoft.Azure.Cosmos;
using static CorePayments.Infrastructure.Constants;
using System.Net;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using System.Drawing.Printing;
using CorePayments.SemanticKernel;

namespace CorePayments.WebAPI.Endpoints.Http
{
    public class TransactionEndpoints : EndpointsBase
    {
        readonly ITransactionRepository _transactionRepository;
        readonly ICustomerRepository _customerRepository;
        readonly IAnalyticsEngine _rulesEngine;

        public TransactionEndpoints(
            ITransactionRepository transactionRepository,
            ICustomerRepository customerRepository,
            IAnalyticsEngine rulesEngine,
            ILogger<MemberEndpoints> logger)
        {
            _transactionRepository = transactionRepository;
            _customerRepository = customerRepository;
            _rulesEngine = rulesEngine;
            Logger = logger;
            UrlFragment = "api/transaction";
        }

        public override void AddRoutes(WebApplication app)
        {
            app.MapPost($"/{UrlFragment}/createsproc", async (Transaction transaction) => await CreateTransactionSproc(transaction))
                .WithName("CreateTransactionSproc");
            app.MapPost($"/{UrlFragment}/createtbatch", async (Transaction transaction) => await CreateTransactionTBatch(transaction))
                .WithName("CreateTransactionTBatch");
            app.MapGet($"/api/statement/{{accountId}}", async (string accountId, [FromQuery] int? pageSize, [FromQuery] string? continuationToken) => await GetTransactionStatement(accountId, pageSize, continuationToken))
                .WithName("GetTransactionStatement");
            app.MapGet($"/api/statement/{{accountId}}/analyze", async (string accountId, [FromQuery] string query) => await GetTransactionsAnalysis(accountId, query))
                .WithName("GetTransactionsAnalysis");
        }

        protected virtual async Task<IResult> CreateTransactionSproc(Transaction transaction)
        {
            try
            {
                var result = await _transactionRepository.ProcessTransactionSProc(transaction);
                
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);

                return Results.BadRequest();
            }
        }

        protected virtual async Task<IResult> CreateTransactionTBatch(Transaction transaction)
        {
            try
            {
                var (account, statusCode, message) = await _transactionRepository.ProcessTransactionTBatch(transaction);

                if (new HttpResponseMessage(statusCode).IsSuccessStatusCode)
                {
                    return Results.Ok(account);
                }
                else if (statusCode == HttpStatusCode.PreconditionFailed)
                {
                    return Results.StatusCode((int)HttpStatusCode.PreconditionFailed);
                }
                else if (statusCode == HttpStatusCode.NotFound)
                {
                    return Results.NotFound(message);
                }
                else
                {
                    return string.IsNullOrWhiteSpace(message) ? Results.BadRequest() : Results.BadRequest(message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);

                return Results.BadRequest();
            }
        }

        protected virtual async Task<IResult> GetTransactionStatement(string accountId, int? pageSize, string? continuationToken)
        {
            try
            {
                if (pageSize is null || pageSize.Value <= 0)
                {
                    pageSize = 50;
                }

                var (transactions, newContinuationToken) = await _customerRepository.GetPagedTransactionStatement(accountId, pageSize.Value, continuationToken);
                if (transactions == null)
                {
                    return Results.NotFound();
                }
                
                return Results.Ok(new PagedResponse<Transaction>
                {
                    Page = transactions,
                    ContinuationToken = Uri.EscapeDataString(newContinuationToken ?? String.Empty)
                });
            }
            catch (CosmosException ex)
            {
                Logger.LogError(ex.Message, ex);
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                return Results.BadRequest(ex.Message);
            }
        }

        protected virtual async Task<IResult> GetTransactionsAnalysis(string accountId, string query)
        {
            try
            {
                var (transactions, newContinuationToken) = await _customerRepository.GetPagedTransactionStatement(accountId, 50, null);
                if (transactions == null)
                {
                    return Results.NotFound("No transactions fund to evaluate.");
                }

                var analysisResult = await _rulesEngine.ReviewTransactions(transactions, query);

                Logger.LogInformation($"Successfully retrieved analysis for transactions in Account: {accountId}");

                return Results.Ok(analysisResult);
            }
            catch (CosmosException ex)
            {
                Logger.LogError(ex.Message, ex);
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                return Results.BadRequest(ex.Message);
            }
        }

    }
}
