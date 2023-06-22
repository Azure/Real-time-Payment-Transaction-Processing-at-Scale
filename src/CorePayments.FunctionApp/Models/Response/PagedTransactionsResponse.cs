using CorePayments.Infrastructure.Domain.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CorePayments.FunctionApp.Models.Response
{
    public class PagedResponse<T>
    {
        [JsonProperty("page")]
        public IEnumerable<T> Page { get; set; }

        [JsonProperty("countinuationToken")]
        public string ContinuationToken { get; set; }
    }
}
