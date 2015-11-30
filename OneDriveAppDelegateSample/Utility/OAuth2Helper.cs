using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace OAuth2
{
    public class OAuth2Helper
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public Uri TokenService { get; set; }

        public OAuth2Helper(string tokenService, string clientId, string clientSecret = null, string redirectUri = null)
        {
            this.TokenService = new Uri(tokenService);
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.RedirectUri = redirectUri;
        }

        public async Task<OAuth2Token> RedeemRefreshTokenAsync(string refreshToken, string resource = null)
        {
            var queryBuilder = new QueryStringBuilder { StartCharacter = null };

            queryBuilder.Add("grant_type", "refresh_token");
            queryBuilder.Add("refresh_token", refreshToken);
            queryBuilder.Add("client_id", this.ClientId);

            if (!string.IsNullOrEmpty(this.RedirectUri))
            {
                queryBuilder.Add("redirect_uri", this.RedirectUri);
            }
            if (!string.IsNullOrEmpty(this.ClientSecret))
            {
                queryBuilder.Add("client_secret", this.ClientSecret);
            }
            if (!string.IsNullOrEmpty(resource))
            {
                queryBuilder.Add("resource", resource);
            }

            return await PostToTokenEndPoint(queryBuilder);
        }

        public async Task<OAuth2Token> RedeemAuthorizationCodeAsync(string authCode, string resource = null)
        {
            var queryBuilder = new QueryStringBuilder { StartCharacter = null };

            queryBuilder.Add("grant_type", "authorization_code");
            queryBuilder.Add("code", authCode);
            queryBuilder.Add("client_id", this.ClientId);

            if (!string.IsNullOrEmpty(this.RedirectUri))
            {
                queryBuilder.Add("redirect_uri", this.RedirectUri);
            }
            if (!string.IsNullOrEmpty(this.ClientSecret))
            {
                queryBuilder.Add("client_secret", this.ClientSecret);
            }
            if (!string.IsNullOrEmpty(resource))
            {
                queryBuilder.Add("resource", resource);
            }

            return await PostToTokenEndPoint(queryBuilder);
        }

        private async Task<OAuth2Token> PostToTokenEndPoint(QueryStringBuilder queryBuilder)
        {
            HttpWebRequest request = WebRequest.CreateHttp(this.TokenService);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter requestWriter = new StreamWriter(await request.GetRequestStreamAsync()))
            {
                await requestWriter.WriteAsync(queryBuilder.ToString());
                await requestWriter.FlushAsync();
            }

            HttpWebResponse httpResponse;
            try
            {
                var response = await request.GetResponseAsync();
                httpResponse = response as HttpWebResponse;
            }
            catch (WebException webex)
            {
                httpResponse = webex.Response as HttpWebResponse;
            }
            catch (Exception ex)
            {
                throw new OAuth2Exception("Http request error.", ex);
            }

            if (httpResponse == null)
            {
                throw new OAuth2Exception("Http response was null");
            }

            try
            {
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    if (httpResponse.ContentType.StartsWith("application/json"))
                    {
                        var errorDetails = await ParseResponseJson<AzureActiveDirectoryErrorResponse>(httpResponse);
                        if (errorDetails != null)
                        {
                            throw new OAuth2Exception(errorDetails.Description) { Details = errorDetails };
                        }
                        else
                        {
                            throw new OAuth2Exception(
                                "Http response was invalid: statusCode=" + httpResponse.StatusCode);
                        }
                    }
                }

                return await ParseResponseJson<OAuth2Token>(httpResponse);
            }
            catch (Exception ex)
            {
                throw new OAuth2Exception("General error occured.", ex);
            }
            finally
            {
                httpResponse.Dispose();
            }
        }

        private async Task<T> ParseResponseJson<T>(HttpWebResponse httpResponse)
        {
            using (var responseBodyStreamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseBody = await responseBodyStreamReader.ReadToEndAsync();
                var tokenResult = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseBody);

                httpResponse.Dispose();
                return tokenResult;
            }
        }
    }

   

    public class OAuth2Exception : Exception
    {

        public OAuth2Exception()
        {

        }

        public OAuth2Exception(string message) : base(message)
        {

        }

        public OAuth2Exception(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public AzureActiveDirectoryErrorResponse Details { get; set; }
    }

    public class AzureActiveDirectoryErrorResponse
    {
        [JsonProperty("correlation_id")]
        public string CorrelationId { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_codes")]
        public long[] ErrorCodes { get; set; }

        [JsonProperty("error_description")]
        public string Description { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("trace_id")]
        public string TraceId { get; set; }
    }


    public class OAuth2Token
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int AccessTokenExpirationDuration { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string Scopes { get; set; }

        [JsonProperty("authentication_token")]
        public string AuthenticationToken { get; set; }
    }
}