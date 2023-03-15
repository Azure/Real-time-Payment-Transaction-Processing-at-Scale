using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace payments_model
{
    public class AccountSummary
    {
        public string id { get; set; }
        public string accountId { get { return this.id; } }
        public string customerGreetingName { get; set; }
        public double balance { get; set; }
        public string accountType { get; set; }
        public string type { get; set; }
        public double overdraftLimit { get; set; }
        public DateTime memberSince { get; set; }
        public int ttl { get { return -1; } }
    }
}
