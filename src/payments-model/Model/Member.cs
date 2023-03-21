using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace payments_model.Model
{
    public class Member
    {
        public string id {  get; set; }
        public string memberId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zipcode { get; set; }
        public string country { get; set; }
    }
}
