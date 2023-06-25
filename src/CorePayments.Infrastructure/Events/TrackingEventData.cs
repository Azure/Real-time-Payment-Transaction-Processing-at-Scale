using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePayments.Infrastructure.Events
{
    public class TrackingEventData
    {
        public DateTime Timestamp { get; set; }
        public object? Data { get; set; }
    }
}
