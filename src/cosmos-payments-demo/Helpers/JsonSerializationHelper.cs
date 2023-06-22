using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cosmos_payments_demo.Helpers
{
    public static class JsonSerializationHelper
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Converters =
            {
                new StringEnumConverter()
            }
        };

        public static string SerializeItem<T>(T item)
        {
            return JsonConvert.SerializeObject(item, Settings);
        }

        public static T DeserializeItem<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data, Settings);
        }
    }
}
