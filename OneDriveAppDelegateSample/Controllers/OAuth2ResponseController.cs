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
using Newtonsoft.Json.Linq;
using System.Net.Http.Formatting;

namespace OneDriveAppDelegateSample.Controllers
{

    public static class OneDriveAppConfiguration
    {
        public static string ClientId { get { return "bd9da400-84c8-4ed5-9479-2f192b206f7c"; } }

        public static string ClientSecret { get { return "tB6PB/O/nyEUnMrj5WFVNWFOzOiM/g0ehEInpqA5mOI="; } }

        public static string RedirectUri { get { return "http://localhost:2007/adminauthresponse"; } }

        public static string TokenServiceUri
        {
            get { return "https://login.microsoftonline.com/common/oauth2/token"; }
        }

        public static string AuthorizationServiceUri
        {
            get { return "https://login.microsoftonline.com/common/oauth2/authorize"; }
        }

    }

    public class OAuth2ResponseController : ApiController
    {

        /// <summary>
        /// Convert form data into a name value collection
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        private static NameValueCollection ConvertFormData(FormDataCollection formData)
        {
            IEnumerator<KeyValuePair<string, string>> pairs = formData.GetEnumerator();
            NameValueCollection collection = new NameValueCollection();
            while (pairs.MoveNext())
            {
                KeyValuePair<string, string> pair = pairs.Current;
                collection.Add(pair.Key, pair.Value);
            }
            return collection;
        }



        [HttpPost, Route("adminauthresponse")]
        public HttpResponseMessage AdminAuthRedirect(FormDataCollection formData)
        {
            // Redeem authorization code for account information
            var values = ConvertFormData(formData);

            var idToken = values["id_token"];

            JWT.JsonWebToken.JsonSerializer = new Utility.CustomJsonSerializer();
            var openIdToken = JWT.JsonWebToken.DecodeToObject<Models.OpenIdToken>(idToken, "", false);
            
            // App is now authorized, and we can use the tenant ID to get access tokens
            var message = new HttpResponseMessage(HttpStatusCode.Found);
            message.Headers.Location = new Uri("/", UriKind.Relative);

            var cookie = Utility.TokenStore.CookieForToken(openIdToken);
            message.Headers.AddCookies(new CookieHeaderValue[] { cookie });
            return message;
        }


        [HttpGet, Route("signout")]
        public HttpResponseMessage SignOut()
        {
            var cookie = Utility.TokenStore.CookieForToken(null);

            var response = new HttpResponseMessage(HttpStatusCode.Redirect);
            response.Headers.Location = new Uri("/", UriKind.Relative);
            response.Headers.AddCookies(new CookieHeaderValue[] { cookie });
            return response;
        }

        

    }
}
