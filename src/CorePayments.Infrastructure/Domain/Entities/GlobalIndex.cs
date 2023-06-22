using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CorePayments.Infrastructure.Domain.Entities
{
    /// <summary>
    /// The Global Index maps different entity relationships based on the partition key and id combination,
    /// along with the targetDocType for flexible lookups and pseudo-joins in NoSQL for 1:few relationships.
    /// </summary>
    public class GlobalIndex
    {
        public string id { get; set; }
        public string partitionKey { get; set; }
        public string targetDocType { get; set; }
    }
}
