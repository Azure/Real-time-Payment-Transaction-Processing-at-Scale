using CorePayments.Infrastructure.Domain.Entities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using System.Text.Json;

namespace CorePayments.SemanticKernel
{
    public class RulesEngine : IRulesEngine
    {
        public async Task<string> ReviewAccount(AccountSummary account)
        {
            var builder = new KernelBuilder();

            builder.WithAzureTextCompletionService(
                     "completions-003",                  // Azure OpenAI Deployment Name
                     "https://bhm7vnpxv6irq-openai.openai.azure.com/", // Azure OpenAI Endpoint
                     "b3990195e52c4545a6f3a085590d9d56");      // Azure OpenAI Key

            var kernel = builder.Build();

            string skPrompt = @"
            You are an analyst bot that helps staff summarize data about members by processing a members accounts and transactions. 
            You are provided data about the member in the JSON format, as well as the query submitted by the user.
            You can return your results in JSON or the format specified by the user in the query. 

            For example:

            +++

            [INPUT]
            Member Data:
            {
                ""accounts"":[
                    {""account_id"":""123"", ""member_id"":""abcsam123"", ""balance"":""$100""},
                    {""account_id"":""246"", ""member_id"":""abcsam123"", ""balance"":""-$30""}
                ],
                ""transaction"":[
                    {""account_id"":""123"", ""member_id"":""abcsam123"", ""transaction_id"":""1"", ""amount"":""$100""}
                    {""account_id"":""246"", ""member_id"":""abcsam123"", ""transaction_id"":""1"", ""amount"":""$10""}
                    {""account_id"":""246"", ""member_id"":""abcsam123"", ""transaction_id"":""2"", ""amount"":""-$40""}
                ]
            }
            User Query:
            How many transactions does the user ""abcsam123"" have?
            [END INPUT]

            Provide your response by completing the following bullet:
            - Result: 3

            +++

            [INPUT]
            Member Data:
            {{$memberData}}

            User Query:
            {{$query}}
            [END INPUT]

            Provide your response by completing the following bullet:
            - Result:
            ";

            var reviewer = kernel.CreateSemanticFunction(skPrompt, "review", "ReviewSkill", description: "Review the input", maxTokens: 2000, temperature: 0.0);

            JsonSerializerOptions ser_options = new()
            {
                WriteIndented = true,
                MaxDepth = 20,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };

            string input = JsonSerializer.Serialize(account, ser_options);

            string result = (await reviewer.InvokeAsync(input)).Result;

            return result;
        }
    }
}