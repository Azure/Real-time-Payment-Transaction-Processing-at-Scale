using System;
using System.Text.Json.Serialization;

<<<<<<< HEAD:src/CorePayments.Infrastructure/Domain/Entities/Transaction.cs
namespace CorePayments.Infrastructure.Domain.Entities
=======
namespace payments_model
>>>>>>> f482f0adcc278b6f40833ae5755a42e4c19aa5c3:src/payments-model/Model/Transaction.cs
{
    public class Transaction
    {
        public Transaction()
        {
            this.id = Guid.NewGuid().ToString();
            this.timestamp= DateTime.UtcNow;
        }
        public string id { get; set; }
        public string accountId { get; set; }
        public string description { get; set; }
        public string merchant { get; set; }
        public string type { get; set; }
        public double amount { get; set; }
        public DateTime timestamp { get; set; }
    }
}
