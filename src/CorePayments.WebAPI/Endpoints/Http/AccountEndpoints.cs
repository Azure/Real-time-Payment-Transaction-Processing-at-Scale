using CorePayments.Infrastructure;
using CorePayments.Infrastructure.Domain.Entities;
using CorePayments.Infrastructure.Repository;
using CorePayments.WebAPI.Components;
using CorePayments.WebAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CorePayments.WebAPI.Endpoints.Http
{
    public class AccountEndpoints : EndpointsBase
    {
        readonly ICustomerRepository _customerRepository;
        readonly ITransactionRepository _transactionRepository;

        public AccountEndpoints(
            ICustomerRepository customerRepository,
            ITransactionRepository transactionRepository,
            ILogger<AccountEndpoints> logger)
        {
            _customerRepository = customerRepository;
            _transactionRepository = transactionRepository;
            Logger = logger;
            UrlFragment = "api/account";
        }

        public override void AddRoutes(WebApplication app)
        {
            app.MapPost($"/{UrlFragment}", async (AccountSummary account) => await CreateAccount(account))
                .WithName("CreateAccount");
            app.MapGet($"/{UrlFragment}/find", async ([FromQuery] string s) => await FindAccountSummary(s))
                .WithName("FindAccountSummary");
            app.MapGet($"/{UrlFragment}s", async ([FromQuery] int? pageSize, [FromQuery] string? continuationToken) => await GetAccounts(pageSize, continuationToken))
                .WithName("GetAccounts");
            app.MapGet($"/{UrlFragment}/{{accountId}}", async (string accountId) => await GetAccount(accountId))
                .WithName("GetAccountSummary");
        }

        protected virtual async Task<IResult> CreateAccount(AccountSummary account)
        {
            try
            {
                await _transactionRepository.CreateItem(account);

                return Results.Ok(account);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);

                return Results.BadRequest();
            }
        }

        protected virtual async Task<IResult> FindAccountSummary(string searchString)
        {
            var accounts = await _customerRepository.FindAccountSummary(searchString);

            return Results.Ok(accounts);
        }

        protected virtual async Task<IResult> GetAccounts(int? pageSize, string? continuationToken)
        {
            if (pageSize is null || pageSize.Value <= 0)
            {
                pageSize = 50;
            }

            var (accounts, newContinuationToken) = await _customerRepository.GetPagedAccountSummary(pageSize.Value, continuationToken);
            if (accounts == null)
            {
                return Results.NotFound();
            }
            
            return Results.Ok(new PagedResponse<AccountSummary>
            {
                Page = accounts,
                ContinuationToken = Uri.EscapeDataString(newContinuationToken ?? String.Empty)
            });
        }

        protected virtual async Task<IResult> GetAccount(string accountId)
        {
            var account = await _customerRepository.GetAccountSummary(accountId);

            return account == null ? Results.NotFound() : Results.Ok(account);
        }
    }
}
