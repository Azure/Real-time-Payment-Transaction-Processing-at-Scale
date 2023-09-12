using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CorePayments.Infrastructure.Domain.Entities
{
    public class Member
    {
        public string id => memberId;
        public string memberId { get; set; }
        public string type => Constants.DocumentTypes.Member;
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zipcode { get; set; }
        public string country { get; set; }
        public DateTime memberSince { get; set; }
    }
}
