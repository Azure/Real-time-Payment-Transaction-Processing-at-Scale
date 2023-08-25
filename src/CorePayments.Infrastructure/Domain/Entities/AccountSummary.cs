namespace CorePayments.Infrastructure.Domain.Entities

{
    public class AccountSummary
    {
        public string id { get; set; }
        public string accountId => id;
        public string customerGreetingName { get; set; }
        public double balance { get; set; }
        public string accountType { get; set; }
        public string type => Constants.DocumentTypes.AccountSummary;
        public double overdraftLimit { get; set; }
        public DateTime memberSince { get; set; }
        public int ttl { get { return -1; } }
    }
}
