using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using OAuth2;
using System.Net.Http.Headers;
using System.Collections.Specialized;
using System.Web;

namespace OneDriveAppDelegateSample.Controllers
{

    public static class OneDriveAppConfiguration
    {
        public static string ClientId { get { return "bd9da400-84c8-4ed5-9479-2f192b206f7c"; } }

        public static string ClientSecret { get { return "tB6PB/O/nyEUnMrj5WFVNWFOzOiM/g0ehEInpqA5mOI="; } }

        public static string RedirectUri { get { return "https://onedrive-app-delegate.azurewebsites.net/authredirect"; } }
    }

    public class OAuth2ResponseController : ApiController
    {

        private static string TokenServiceUri = "https://login.microsoftonline.com/common/oauth2/token";
        private static string AuthorizationServiceUri = "https://login.microsoftonline.com/common/oauth2/authorize";

        private static string CookiePassword = "";

        [HttpGet, Route("authredirect")]
        public async Task<IHttpActionResult> AuthRedirect(string code)
        {
            // Redeem authorization code for account information

            OAuth2Helper helper = new OAuth2Helper(TokenServiceUri,
                OneDriveAppConfiguration.ClientId,
                OneDriveAppConfiguration.ClientSecret,
                OneDriveAppConfiguration.RedirectUri);

            var token = await helper.RedeemAuthorizationCodeAsync(code);
            if (null == token)
            {
                // No token means we had an error when redeeming the token, so let's figure out what to do about it.
                return null;
            }

            // Otherwise, if we got a token back, then we should set a secure cookie and use that from now on.
            return null;
        }


        [HttpGet, Route("signout")]
        public HttpResponseMessage SignOut()
        {
            var message = new HttpResponseMessage(HttpStatusCode.Redirect);
            message.Headers.Add("Location", "/");
            message.Headers.AddCookies(new CookieHeaderValue[] { CookieForAccount(null) });

            return message;
        }

        public static CookieHeaderValue CookieForAccount(Models.OneDriveAccount account)
        {
            var nv = new NameValueCollection();
            nv["id"] = null != account ? account.Id.Encrypt(CookiePassword) : "";

            var cookie = new CookieHeaderValue("session", nv);
            cookie.Secure = true;
            cookie.HttpOnly = true;
            cookie.Expires = null != account ? DateTimeOffset.Now.AddMinutes(120) : DateTimeOffset.Now;
            cookie.Path = "/";

            return cookie;
        }

        public static async Task<Models.OneDriveAccount> AccountFromCookie(HttpCookieCollection cookies)
        {
            var sessionCookie = cookies["session"];
            if (null == sessionCookie)
                return null;

            return await AccountFromCookie(sessionCookie.Values, true);
        }

        public static async Task<Models.OneDriveAccount> AccountFromCookie(NameValueCollection storedCookieValue, bool shouldDecode)
        {
            string accountId = null;
            string encryptedAccountId = storedCookieValue["id"];
            if (shouldDecode && null != encryptedAccountId)
            {
                encryptedAccountId = HttpUtility.UrlDecode(encryptedAccountId);
            }

            if (null != encryptedAccountId)
            {
                accountId = encryptedAccountId.Decrypt(WebAppConfig.Default.CookiePassword);
            }

            if (null != accountId)
            {
                var account = await AzureStorage.LookupAccountAsync(accountId);
                return account;
            }

            return null;
        }

    }
}
