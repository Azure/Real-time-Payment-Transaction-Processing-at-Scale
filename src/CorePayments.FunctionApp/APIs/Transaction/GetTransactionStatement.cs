using Azure;
using CorePayments.FunctionApp.Models.Response;
using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model = CorePayments.Infrastructure.Domain.Entities;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using CorePayments.SemanticKernel;

namespace CorePayments.FunctionApp.APIs.Transaction
{
    public class GetTransactionStatement
    {
        readonly ICustomerRepository _customerRepository;
        readonly IAnalyticsEngine _rulesEngine;

        public GetTransactionStatement(
            ICustomerRepository customerRepository,
            IAnalyticsEngine rulesEngine)
        {
            _customerRepository = customerRepository;
            _rulesEngine = rulesEngine;
        }

        [Function("GetTransactionStatement")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "statement/{accountId}")] HttpRequestData req,
            string accountId,
            FunctionContext context)
        {
            var logger = context.GetLogger<GetTransactionStatement>();
            int pageSize = -1;
            int.TryParse(req.Query["pageSize"], out pageSize);
            if (pageSize <= 0)
            {
                pageSize = 50;
            }

            string continuationToken = req.Query["continuationToken"];

            var (transactions, newContinuationToken) = await _customerRepository.GetPagedTransactionStatement(accountId, pageSize, continuationToken);
            if (transactions == null)
            {
                return req.CreateResponse(System.Net.HttpStatusCode.NotFound);
            }
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new PagedResponse<Model.Transaction>
            {
                Page = transactions,
                ContinuationToken = Uri.EscapeDataString(newContinuationToken ?? String.Empty)
            });
            return response;
        }


        [Function("GetTransactionsAnalyis")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "statement/{accountId}/analyze")] HttpRequestData req,
            string accountId,
            string query,
            FunctionContext context)
        {
            var log = context.GetLogger<GetTransactionStatement>();
            using (log.BeginScope("HttpTrigger: GetTransactionsAnalyis"))
            {
                try
                {

                    int pageSize = -1;
                    int.TryParse(req.Query["pageSize"], out pageSize);
                    if (pageSize <= 0)
                    {
                        pageSize = 50;
                    }

                    string continuationToken = req.Query["continuationToken"];

                    var (transactions, newContinuationToken) = await _customerRepository.GetPagedTransactionStatement(accountId, pageSize, continuationToken);
                    if (transactions == null)
                    {
                        return req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                    }

                    var analysisResult = await _rulesEngine.ReviewTransactions(transactions, query);

                    log.LogInformation($"Successfully retrieved analysis for transactions in Account: {accountId}");

                    var response = req.CreateResponse(HttpStatusCode.OK);
                    await response.WriteAsJsonAsync(analysisResult);
                    return response;
                }
                catch (Exception ex)
                {
                    var response = req.CreateResponse(HttpStatusCode.BadRequest);
                    await response.WriteStringAsync(ex.Message);
                    return response;
                }
            }
        }
    }
}
