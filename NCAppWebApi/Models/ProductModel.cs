using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
namespace NCAppWebApi.Models
{
    public class ProductModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        
        [JsonProperty(PropertyName = "item_name")]
        public string ItemName { get; set; }
        
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }
        
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "examples")]
        public string[] Examples { get; set; }

        [JsonProperty(PropertyName = "env_impact")]
        public string ImpactStatement { get; set; }


        [JsonProperty(PropertyName = "recycle_score")]
        public int Score { get; set; }

        [JsonProperty(PropertyName = "img_path")]
        public string Image { get; set; }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}