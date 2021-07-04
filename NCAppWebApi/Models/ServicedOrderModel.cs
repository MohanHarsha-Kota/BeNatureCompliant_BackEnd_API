using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NCAppWebApi.Models
{
    public class ServicedOrderModel
    {
        [JsonProperty(PropertyName = "id")]
        public string ServicePickupId { get; set; }

        [JsonProperty(PropertyName = "scheduled_order_Id")]
        public string OrderScheduleId { get; set; }

        [JsonProperty(PropertyName = "customer_email")]
        public string CustomerEmailId { get; set; }

        [JsonProperty(PropertyName = "eco_score")]
        public int EcoScore { get; set; }

        [JsonProperty(PropertyName = "serviced_date")]
        public string ServicedDate { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}