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
using OneDriveAppDelegateSample.Models;

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

            var results = await GetCollectionResponse<Models.User>(graphUrl, accessToken);
            foreach (var user in results.Value.ToArray())
            {
                if (user.UserPrincipalName != null && user.UserPrincipalName.Contains("#EXT#"))
                    results.Value.Remove(user);
            }
            return results;
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

            var parentItemWithChildren = await GetResponse<Models.DriveItem>(requestUrl, accessToken);

            await HackHackAddMissingFacetsAsync(parentItemWithChildren, accessToken, sharePointBaseUrl);

            return parentItemWithChildren;
        }

        /// <summary>
        /// Returns thumbnails and permissions information for every file in the itemWithChildren's children collection
        /// as long as the total number of items is between 1 and 20.
        /// </summary>
        /// <param name="itemWithChildren"></param>
        /// <param name="accessToken"></param>
        /// <param name="sharePointBaseUrl"></param>
        /// <returns></returns>
        internal static async Task HackHackAddMissingFacetsAsync(DriveItem itemWithChildren, string accessToken, string sharePointBaseUrl, int CAP = 20)
        {
            DriveItem[] children = itemWithChildren.Children;
            List<DriveItem> newChildren = new List<DriveItem>();

            if (null != children && children.Length > 0 && children.Length < CAP)
            {
                // Request more data from SharePoint about these items by making individual API 
                // calls per item and replace the element in the children collection. In the future
                // this won't be necessary because the API will support returning more information

                await children.ForEachAsync(4, async (child) => 
                {
                    string requestUrl = string.Format("{0}/_api/v2.0/drives/{1}/items/{2}?expand=thumbnails,permissions", sharePointBaseUrl, child.ParentReference.DriveId, child.Id);
                    try
                    {
                        var newItemData = await GetResponse<Models.DriveItem>(requestUrl, accessToken);
                        newItemData.Shared = SharedFacet.CreateFromPermissions(newItemData.Permissions);
                        newChildren.Add(newItemData);
                    }
                    catch (Exception ex)
                    {
                        newChildren.Add(child);
                    }
                });

                itemWithChildren.Children = newChildren.ToArray();
            }
        }

        [HttpGet, Route("api/download")]
        public async Task<HttpResponseMessage> DownloadFile(string userUpn, string fileId)
        {
            var accessTokens = ReadAccessTokenFromCookie();

            var sharePointResourceUri = await GetSharePointPersonalSiteResourceUrlAsync(userUpn, accessTokens);
            var accessToken = await accessTokens.GetAccessTokenForResourceAsync(sharePointResourceUri, false);

            // Workaround: Retrieve the webUrl for the file being requested
            string requestUrl = string.Format("{0}/_api/v2.0/drives/{1}/items/{2}", sharePointResourceUri, userUpn, fileId);

            // Grab the webUrl property from the file
            Models.DriveItem fileMetadata = await GetResponse<Models.DriveItem>(requestUrl, accessToken);
            var webUrl = new UriBuilder(fileMetadata.WebUrl);

            // Download the file using the SharePoint 2013 REST API
            var relativePath = webUrl.Path;
            string downloadUrl = string.Format("{0}/_api/web/GetFileByServerRelativeUrl('{1}')/$value", accessTokens.SharePointMySiteUri, relativePath);

            var contentStream = await GetResponseStreamAsync(downloadUrl, accessToken);

            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StreamContent(contentStream);
            message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = fileMetadata.Name };
            return message;
        }

        internal static async Task<string> GetSharePointPersonalSiteResourceUrlAsync(string userId, Models.AccessTokens accessTokens)
        {
            if (accessTokens.SharePointResourceUri != null)
                return accessTokens.SharePointResourceUri;

            string accessToken = await accessTokens.GetAccessTokenForResourceAsync("https://graph.microsoft.com", false);
            string graphUrl = string.Format("https://graph.microsoft.com/v1.0/users/{0}?$select=mySite", userId);
            var userData = await GetResponse<Models.User>(graphUrl, accessToken);

            var uri = new Uri(userData.MySiteUrl);
            var baseUrl = string.Format("{0}://{1}", uri.Scheme, uri.Authority);

            accessTokens.SharePointResourceUri = baseUrl;
            accessTokens.SharePointMySiteUri = userData.MySiteUrl;

            return accessTokens.SharePointResourceUri;
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

        internal static async Task<Stream> GetResponseStreamAsync(string url, string accessToken)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            request.Method = "GET";

            var response = await request.GetResponseAsync();
            return response.GetResponseStream();
        }

        
        

        internal static async Task<Models.GraphCollectionResponse<T>> GetCollectionResponse<T>(string queryUrl, string accessToken) where T : class
        {
            return await GetResponse<Models.GraphCollectionResponse<T>>(queryUrl, accessToken);
        }



    }
}