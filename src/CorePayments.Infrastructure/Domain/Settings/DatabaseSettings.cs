using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePayments.Infrastructure.Domain.Settings
{
    public record DatabaseSettings
    {
        public required string PaymentsDatabase { get; init; }
        public required string TransactionsContainer { get; init; }
        public required string MemberContainer { get; init; }
        public required string GlobalIndexContainer { get; init; }
        public required string CustomerContainer { get; init; }
        public required string CosmosDBConnection { get; init; }
        public required string PreferredRegions { get; init; }
        public bool IsMasterRegion { get; init; }
    }
}
