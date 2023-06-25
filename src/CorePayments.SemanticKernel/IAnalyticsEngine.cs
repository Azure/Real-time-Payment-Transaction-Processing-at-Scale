using CorePayments.Infrastructure.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePayments.SemanticKernel
{
    public interface IAnalyticsEngine
    {
        Task<string> ReviewTransactions(IEnumerable<Transaction> transactions, string query);
    }
}
