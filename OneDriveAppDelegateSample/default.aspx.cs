using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OneDriveAppDelegateSample
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PageAsyncTask asynctask = new PageAsyncTask(PageLoadAsync);
            Page.RegisterAsyncTask(asynctask);
        }

        private static bool UseDogfoodEnvironemnt(HttpRequest request, HttpResponse response)
        {
            string ppeQueryString = request.QueryString["ppe"];
            if (ppeQueryString == null)
            {
                var cookie = request.Cookies["ppe"];
                if (cookie != null)
                {
                    string storedValue = cookie.Value;
                    if (storedValue != null && storedValue.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }

            bool useDogfood = false;
            if (ppeQueryString.Equals("1"))
            {
                useDogfood = true;
            }

            // Set a cookie to store this preference
            response.Cookies.Add(new HttpCookie("ppe", useDogfood.ToString()));
            return useDogfood;
        }

        private async Task PageLoadAsync()
        {
            if (Request.QueryString["logout"] == "1")
            {
                Response.SetCookie(new HttpCookie(Utility.TokenStore.TokenCookieName, ""));
                Response.Redirect("~/");
                return;
            }

            bool useDogfood = UseDogfoodEnvironemnt(Request, Response);
            var storedToken = Utility.TokenStore.TokenFromCookie(Request.Cookies);
            if (null == storedToken || string.IsNullOrEmpty(storedToken.TenantId))
            {
                // User isn't signed in
                signInLink.NavigateUrl = GenerateLoginUrl(useDogfood);
                panelSignIn.Visible = true;
                panelAuthenticated.Visible = false;
                navBarMenu.Visible = false;
            }
            else
            {
                // User is signed in!
                panelSignIn.Visible = false;
                panelAuthenticated.Visible = true;
                navBarMenu.Visible = true;
            }
            labelServiceTarget.Text = "Target: " + (useDogfood ? "Dogfood" : "Production");
        }

        private string GenerateLoginUrl(bool useDogfood)
        {
            IAppConfig app = useDogfood ? new DogfoodAppConfig() : new ProductionAppConfig();

            var baseUrl = app.AuthorizationServiceUri;
            OAuth2.QueryStringBuilder builder = new OAuth2.QueryStringBuilder();
            builder.Add("client_id", app.ClientId);
            builder.Add("response_type", "code+id_token", false);
            builder.Add("scope", "openid");
            builder.Add("redirect_uri", app.RedirectUri);
            builder.Add("prompt", "admin_consent");
            builder.Add("nonce", Guid.NewGuid().ToString());
            builder.Add("response_mode", "form_post");

            return baseUrl + builder.ToString();
        }

        protected async void buttonGetAccessToken_Click(object sender, EventArgs e)
        {
            bool useDogfood = UseDogfoodEnvironemnt(Request, Response);
            var targetResourceUri = textBoxResourceUri.Text;
            var storedToken = Utility.TokenStore.TokenFromCookie(Request.Cookies);

            try
            {
                Models.AccessTokens tokenCache = Models.AccessTokens.AccessTokensForToken(storedToken);
                var token = await tokenCache.GetAccessTokenForResourceAsync(targetResourceUri, useDogfood);
                accessToken.Text = token;
            }
            catch (Exception ex)
            {
                accessToken.Text = ex.ToString();
            }
        }
    }
}