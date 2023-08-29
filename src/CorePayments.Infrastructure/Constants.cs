using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePayments.Infrastructure
{
    public static class Constants
    {
        public const int DefaultPageSize = 50;

        public static class DocumentTypes
        {
            public const string AccountSummary = "accountSummary";
            public const string Member = "member";
            public const string TransactionDebit = "debit";
            public const string TransactionDeposit = "deposit";
        }

        public enum AccountAssignmentOperations
        {
            Add,
            Remove
        }

        public static class EventHubs
        {
            public const string PaymentEvents = "paymentEvents";
        }

        
        public static class Identity
        {
            public const string ClientId = "ClientId";
        }
    }
}
