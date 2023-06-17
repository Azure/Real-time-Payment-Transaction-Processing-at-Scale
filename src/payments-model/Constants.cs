using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace payments_model
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
    }
}
