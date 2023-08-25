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

namespace CorePayments.WebAPI.Endpoints.Http
{
    public class MemberEndpoints : EndpointsBase
    {
        readonly IMemberRepository _memberRepository;
        readonly IGlobalIndexRepository _globalIndexRepository;
        readonly ICustomerRepository _customerRepository;

        public MemberEndpoints(
            IMemberRepository memberRepository,
            IGlobalIndexRepository globalIndexRepository,
            ICustomerRepository customerRepository,
            ILogger<MemberEndpoints> logger)
        {
            _memberRepository = memberRepository;
            _globalIndexRepository = globalIndexRepository;
            _customerRepository = customerRepository;
            Logger = logger;
            UrlFragment = "api/member";
        }

        public override void AddRoutes(WebApplication app)
        {
            app.MapPost($"/{UrlFragment}", async (Member member) => await CreateMember(member))
                .WithName("CreateMember");
            app.MapGet($"/{UrlFragment}/{{memberId}}/accounts", async (string memberId) => await GetMemberAccounts(memberId))
                .WithName("GetMemberAccounts");
            app.MapGet($"/{UrlFragment}s", async ([FromQuery] int? pageSize, [FromQuery] string? continuationToken) => await GetMembers(pageSize, continuationToken))
                .WithName("GetMembers");
            app.MapGet($"/{UrlFragment}/{{memberId}}", async (string memberId) => await GetMember(memberId))
                .WithName("GetMember");
            app.MapPatch($"/{UrlFragment}/{{memberId}}", async (string memberId, Member member) => await PatchMember(memberId, member))
                .WithName("PatchMember");
            app.MapPost($"/{UrlFragment}/{{memberId}}/accounts/add/{{accountId}}", async (string memberId, string accountId) => await ModifyMemberAccountAssignment(AccountAssignmentOperations.Add, memberId, accountId))
                .WithName("AddAccountToMember");
            app.MapPost($"/{UrlFragment}/{{memberId}}/accounts/remove/{{accountId}}", async (string memberId, string accountId) => await ModifyMemberAccountAssignment(AccountAssignmentOperations.Remove, memberId, accountId))
                .WithName("RemoveAccountFromMember");
        }

        protected virtual async Task<IResult> CreateMember(Member member)
        {
            try
            {
                if (member != null)
                {
                    member.memberId = Guid.NewGuid().ToString();

                    await _memberRepository.CreateItem(member);

                    // Create a global index lookup for this member.
                    var globalIndex = new GlobalIndex
                    {
                        partitionKey = member.memberId,
                        targetDocType = DocumentTypes.Member,
                        id = member.memberId
                    };
                    await _globalIndexRepository.CreateItem(globalIndex);
                }
                else
                {
                    return Results.BadRequest("Invalid member record. Please check the fields and try again.");
                }

                return Results.Accepted();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);

                return Results.BadRequest();
            }
        }

        protected virtual async Task<IResult> PatchMember(string memberId, Member member)
        {
            try
            {
                var patchOpsCount = await _memberRepository.PatchMember(member, memberId);

                if (patchOpsCount == 0)
                {
                    return Results.BadRequest("No attributes provided.");
                }

                return Results.Accepted();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);

                return Results.BadRequest();
            }
        }

        protected virtual async Task<IResult> GetMemberAccounts(string memberId)
        {
            try
            {
                // Perform a lookup using the global index:
                var accountsForMember = await _globalIndexRepository.GetAccountsForMember(memberId);

                var accounts = await _customerRepository.GetAccountSummaries(accountsForMember.Select(x => x.id));

                return Results.Ok(accounts);
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

        protected virtual async Task<IResult> GetMembers(int? pageSize, string? continuationToken)
        {
            if (pageSize is null || pageSize.Value <= 0)
            {
                pageSize = 50;
            }

            var (members, newContinuationToken) = await _memberRepository.GetPagedMembers(pageSize.Value, continuationToken);
            if (members == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new PagedResponse<Member>
            {
                Page = members,
                ContinuationToken = Uri.EscapeDataString(newContinuationToken ?? String.Empty)
            });
        }

        protected virtual async Task<IResult> GetMember(string memberId)
        {
            var member = await _memberRepository.GetMember(memberId);

            return member == null ? Results.NotFound() : Results.Ok(member);
        }

        protected virtual async Task<IResult> ModifyMemberAccountAssignment(AccountAssignmentOperations operation, string memberId, string accountId)
        {
            try
            {
                await _globalIndexRepository.ProcessAccountAssignment(operation, memberId, accountId);

                return Results.Ok();
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
