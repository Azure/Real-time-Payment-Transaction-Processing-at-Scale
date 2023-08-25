using Newtonsoft.Json;

namespace CorePayments.WebAPI.Models.Response
{
    public class PagedResponse<T>
    {
        [JsonProperty("page")]
        public IEnumerable<T> Page { get; set; }

        [JsonProperty("continuationToken")]
        public string ContinuationToken { get; set; }
    }
}
