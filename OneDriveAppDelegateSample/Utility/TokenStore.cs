using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace OneDriveAppDelegateSample.Utility
{
    public static class TokenStore
    {
        private static readonly Dictionary<string, Models.OpenIdToken> CachedTokens =
            new Dictionary<string, Models.OpenIdToken>();


        public static CookieHeaderValue CookieForToken(Models.OpenIdToken token)
        {
            string lookupId = null;
            if (null != token)
            {
                lookupId = Guid.NewGuid().ToString();
                CachedTokens[lookupId] = token;
            }

            var nv = new NameValueCollection();
            nv["id"] = lookupId;

            var cookie = new CookieHeaderValue("session", nv);
            //cookie.Secure = true;
            cookie.HttpOnly = true;
            cookie.Expires = DateTimeOffset.Now.AddMinutes(120);
            cookie.Path = "/";

            return cookie;
        }

        public static Models.OpenIdToken TokenFromCookie(HttpCookieCollection cookies)
        {
            var sessionCookie = cookies["session"];
            if (null == sessionCookie)
                return null;

            return TokenFromCookie(sessionCookie.Values, true);
        }

        public static Models.OpenIdToken TokenFromCookie(NameValueCollection storedCookieValue, bool shouldDecode)
        {
            string storedValue = storedCookieValue["id"];
            if (shouldDecode && null != storedValue)
            {
                storedValue = HttpUtility.UrlDecode(storedValue);
            }

            if (null != storedValue)
            {
                Models.OpenIdToken storedToken = null;
                if (CachedTokens.TryGetValue(storedValue, out storedToken))
                {
                    if (storedToken.IsExpired())
                    {
                        // Don't require expired tokens.
                        CachedTokens.Remove(storedValue);
                        storedToken = null;
                    }
                    return storedToken;
                }
            }

            return null;
        }
    }
}