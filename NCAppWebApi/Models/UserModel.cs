using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NCAppWebApi.Models
{
    public class UserModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string EmailId { get; set; }

        [JsonProperty(PropertyName = "contact_num")]
        public Int64 Examples { get; set; }

        [JsonProperty(PropertyName = "addresses")]
        public string[] Addresses { get; set; }


        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }


        [JsonProperty(PropertyName = "role")]
        public int RoleId { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}