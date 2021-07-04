using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace NCAppWebApi.Models
{
    public class ScheduleHistoryModel
    {
        [JsonProperty(PropertyName = "id")]
        public string OrderScheduleId { get; set; }
        
        [JsonProperty(PropertyName = "email")]
        public string EmailId { get; set; }

        [JsonProperty(PropertyName = "scheduled_date")]
        public string ScheduledDate { get; set; }

        [JsonProperty(PropertyName = "slot")]
        public string Slot { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string PickupAddress { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}