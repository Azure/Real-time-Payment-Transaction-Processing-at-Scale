using System;

namespace CorePayments.Infrastructure.Domain.Entities
{
    public class AccountSummary
    {
        public string id { get; set; }
        public string accountId { get { return this.id; } }
        public string customerGreetingName { get; set; }
        public decimal balance { get; set; }
        public string accountType { get; set; }
        public string type { get; set; }
        public decimal limit { get; set; }
        public DateTime memberSince { get; set; }
        public int ttl { get { return -1; } }
    }
}
