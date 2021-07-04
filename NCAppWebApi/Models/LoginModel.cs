using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NCAppWebApi.Models
{
    public class LoginModel
    {
        [JsonProperty(PropertyName = "username")]

        public string Username { get; set; }
        [JsonProperty(PropertyName = "password")]

        public string Password { get; set; }
    }
}