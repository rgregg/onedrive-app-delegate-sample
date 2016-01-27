using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OneDriveAppDelegateSample.Models
{
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("userPrincipalName")]
        public string UserPrincipalName { get; set; }

        [JsonProperty("mySite")]
        public string MySiteUrl { get; set; }
    }


    public class GraphCollectionResponse<T> where T : class
    {
        [JsonProperty("value")]
        public List<T> Value { get; set; }

        [JsonProperty("@odata.nextLink", DefaultValueHandling=DefaultValueHandling.Ignore)]
        public string NextPageUrl { get; set; }
    }
}