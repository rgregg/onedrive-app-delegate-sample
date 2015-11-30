using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OneDriveAppDelegateSample.Models
{
    public class OpenIdToken
    {
        [JsonProperty("tid")]
        public string TenantId { get; set; }

        [JsonProperty("oid")]
        public string UserId{ get; set; }

        [JsonProperty("upn")]
        public string UserPrincipalName { get; set; }

        [JsonProperty("name")]
        public string DisplayName { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        [JsonProperty("exp")]
        public long ExpirationSeconds { get; set; }


        public bool IsExpired()
        {
            DateTimeOffset expirationDate =
                new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(ExpirationSeconds);

            return expirationDate <= DateTimeOffset.UtcNow;
        }
    }
}