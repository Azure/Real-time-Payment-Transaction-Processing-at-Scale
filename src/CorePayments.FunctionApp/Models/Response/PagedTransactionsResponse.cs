using CorePayments.Infrastructure.Domain.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CorePayments.FunctionApp.Models.Response
{
    public class PagedTransactionsResponse
    {
        [JsonProperty("page")]
        public IEnumerable<Transaction> Page { get; set; }

        [JsonProperty("countinuationToken")]
        public string ContinuationToken { get; set; }
    }
}
