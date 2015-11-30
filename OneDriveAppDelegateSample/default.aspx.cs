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

        private async Task PageLoadAsync()
        {
            var storedToken = Utility.TokenStore.TokenFromCookie(Request.Cookies);
            if (null == storedToken || string.IsNullOrEmpty(storedToken.TenantId))
            {
                signInLink.NavigateUrl = GenerateLoginUrl();
                panelSignIn.Visible = true;
                panelAuthenticated.Visible = false;
            }
            else
            {
                panelSignIn.Visible = false;
                panelAuthenticated.Visible = true;

                accessToken.Text = await GenerateAccessTokenAsync(storedToken.TenantId);
            }
        }

        private string GenerateLoginUrl()
        {
            var baseUrl = Controllers.OneDriveAppConfiguration.AuthorizationServiceUri;
            OAuth2.QueryStringBuilder builder = new OAuth2.QueryStringBuilder();
            builder.Add("client_id", Controllers.OneDriveAppConfiguration.ClientId);
            builder.Add("response_type", "code+id_token", false);
            builder.Add("scope", "openid");
            builder.Add("redirect_uri", Controllers.OneDriveAppConfiguration.RedirectUri);
            builder.Add("prompt", "admin_consent");
            builder.Add("nonce", Guid.NewGuid().ToString());
            builder.Add("response_mode", "form_post");

            return baseUrl + builder.ToString();
        }

        private async Task<string> GenerateAccessTokenAsync(string tenantId)
        {
            var token = await Utility.AuthHelper.GetAccessTokenAsync(tenantId, "https://seattleappworks.sharepoint.com");
            return token;
        }


    }
}