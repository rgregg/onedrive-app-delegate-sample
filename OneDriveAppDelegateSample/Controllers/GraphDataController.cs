using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;

namespace OneDriveAppDelegateSample.Controllers
{
    public class GraphDataController : ApiController
    {

        public Models.AccessTokens ReadAccessTokenFromCookie()
        {
            var storedToken = Utility.TokenStore.TokenFromCookie(Request.Headers);
            return Models.AccessTokens.AccessTokensForToken(storedToken);
        }

        [HttpGet, Route("api/users")]
        public async Task<Models.GraphCollectionResponse<Models.User>> GetTenantUsers()
        {
            const string graphUrl = "https://graph.microsoft.com/v1.0/users";
            var accessTokens = ReadAccessTokenFromCookie();
            string accessToken = await accessTokens.GetAccessTokenForResourceAsync("https://graph.microsoft.com", false);
            return await GetCollectionResponse<Models.User>(graphUrl, accessToken);
        }

        [HttpGet, Route("api/users")]
        public async Task<Models.GraphCollectionResponse<Models.User>> GetNextPageOfUsers(string pageUrl)
        {
            var accessTokens = ReadAccessTokenFromCookie();
            string accessToken = await accessTokens.GetAccessTokenForResourceAsync("https://graph.microsoft.com", false);
            return await GetCollectionResponse<Models.User>(pageUrl, accessToken);
        }

        [HttpGet, Route("api/driveContents")]
        public async Task<Models.DriveItem> GetDriveRootContents(string driveId, string itemId)
        {
            var accessTokens = ReadAccessTokenFromCookie();
            string accessToken = await accessTokens.GetAccessTokenForResourceAsync("https://graph.microsoft.com", false);

            if (null == itemId)
                itemId = "root";
            string graphUrl = string.Format("https://graph.microsoft.com/v1.0/users/{0}/drive/items/{1}?expand=children", driveId, itemId);
            return await GetResponse<Models.DriveItem>(graphUrl, accessToken);
        }

        [HttpGet, Route("api/spDriveContents")]
        public async Task<Models.DriveItem> GetSharePointDriveRootContents(string userUpn, string itemId)
        {
            var accessTokens = ReadAccessTokenFromCookie();

            var sharePointBaseUrl = await GetSharePointPersonalSiteResourceUrlAsync(userUpn, accessTokens);
            var accessToken = await accessTokens.GetAccessTokenForResourceAsync(sharePointBaseUrl, false);

            if (null == itemId)
            {
                itemId = "root";
            }
            string requestUrl = string.Format("{0}/_api/v2.0/drives/{1}/items/{2}?expand=children", sharePointBaseUrl, userUpn, itemId);
            return await GetResponse<Models.DriveItem>(requestUrl, accessToken);
        }

        internal static async Task<string> GetSharePointPersonalSiteResourceUrlAsync(string userId, Models.AccessTokens accessTokens)
        {
            if (accessTokens.SharePointMySiteUrl != null)
                return accessTokens.SharePointMySiteUrl;

            string accessToken = await accessTokens.GetAccessTokenForResourceAsync("https://graph.microsoft.com", false);
            string graphUrl = string.Format("https://graph.microsoft.com/v1.0/users/{0}?$select=mySite", userId);
            var userData = await GetResponse<Models.User>(graphUrl, accessToken);

            var uri = new Uri(userData.MySiteUrl);
            var baseUrl = string.Format("{0}://{1}", uri.Scheme, uri.Authority);

            accessTokens.SharePointMySiteUrl = baseUrl;
            return accessTokens.SharePointMySiteUrl;
        }

        internal static async Task<T> GetResponse<T>(string queryUrl, string accessToken)
        {
            HttpWebRequest request = WebRequest.CreateHttp(queryUrl);
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            request.Method = "GET";

            var response = await request.GetResponseAsync();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                T data = JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync());
                return data;
            }
        }

        internal static async Task<Models.GraphCollectionResponse<T>> GetCollectionResponse<T>(string queryUrl, string accessToken) where T : class
        {
            return await GetResponse<Models.GraphCollectionResponse<T>>(queryUrl, accessToken);
        }



    }
}