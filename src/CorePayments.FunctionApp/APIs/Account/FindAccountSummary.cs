using CorePayments.FunctionApp.Models.Response;
using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Drawing.Printing;
using System;
using System.Threading.Tasks;

namespace CorePayments.FunctionApp.APIs.Account
{
    public class FindAccountSummary
    {
        readonly ICustomerRepository _customerRepository;

        public FindAccountSummary(
            ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }
        
        [Function("FindAccountSummary")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "account/find")] HttpRequestData req,
            FunctionContext context)
        {
            var logger = context.GetLogger<FindAccountSummary>();
            string s = req.Query["s"];

            var accounts = await _customerRepository.FindAccountSummary(s);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(accounts);
            return response;
        }
    }
}