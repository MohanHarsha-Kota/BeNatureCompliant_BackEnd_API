using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NCAppWebApi.Models
{
    public class LoginUserModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string EmailId { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string[] Address { get; set; }

        [JsonProperty(PropertyName = "role")]
        public int RoleId { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}