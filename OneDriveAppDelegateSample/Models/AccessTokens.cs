using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OneDriveAppDelegateSample.Models
{
    public class AccessTokens
    {
        private static Dictionary<string, AccessTokens> GlobalTokenCache = new Dictionary<string, AccessTokens>();

        public OpenIdToken OpenId { get; private set; }

        public string SharePointResourceUri { get; set; }
        public string SharePointMySiteUri { get; set; }

        private Dictionary<string, CachedAccessToken> KnownAccessTokens { get; set; }

        public AccessTokens(OpenIdToken openId)
        {
            this.OpenId = openId;
            this.KnownAccessTokens = new Dictionary<string, CachedAccessToken>();
        }

        public async Task<string> GetAccessTokenForResourceAsync(string resourceUri, bool useDogfood = false)
        {
            CachedAccessToken accessToken;
            if (!KnownAccessTokens.TryGetValue(resourceUri, out accessToken) || accessToken.IsValid)
            {
                accessToken = await Utility.AuthHelper.GetAccessTokenAsync(this.OpenId.TenantId, resourceUri, useDogfood);
                KnownAccessTokens[resourceUri] = accessToken;
            }

            return accessToken.AccessToken;
        }

        /// <summary>
        /// See if we already have an access token cache for the user, otherwise create a new one.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static AccessTokens AccessTokensForToken(OpenIdToken token)
        {
            AccessTokens value;
            if (!GlobalTokenCache.TryGetValue(token.UserId, out value) || null == value)
            {
                // Create a new one and add to cache
                value = new AccessTokens(token);
                GlobalTokenCache[token.UserId] = value;
            }

            return value;
        }
    }
    internal class CachedAccessToken
    {
        public CachedAccessToken(string token, DateTimeOffset expiration)
        {
            this.AccessToken = token;
            this.Expiration = expiration;
        }

        public string AccessToken { get; set; }
        public DateTimeOffset Expiration { get; set; }

        public bool IsValid
        {
            get { return DateTimeOffset.UtcNow >= this.Expiration; }
        }
    }
}