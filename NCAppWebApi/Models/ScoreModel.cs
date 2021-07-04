using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace NCAppWebApi.Models
{
    public class ScoreModel
    {
        [JsonProperty(PropertyName = "score")]
        public int Score { get; set; }


        [JsonProperty(PropertyName = "customer_email")]
        public string CustomerEmailId { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}