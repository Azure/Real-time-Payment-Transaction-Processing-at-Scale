using System;
<<<<<<< HEAD:src/CorePayments.Infrastructure/Domain/Entities/AccountSummary.cs

namespace CorePayments.Infrastructure.Domain.Entities
=======
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace payments_model
>>>>>>> f482f0adcc278b6f40833ae5755a42e4c19aa5c3:src/payments-model/Model/AccountSummary.cs
{
    public class AccountSummary
    {
        public string id { get; set; }
        public string accountId => id;
        public string customerGreetingName { get; set; }
        public double balance { get; set; }
        public string accountType { get; set; }
        public string type { get; set; }
        public double overdraftLimit { get; set; }
        public DateTime memberSince { get; set; }
        public int ttl { get { return -1; } }
    }
}
