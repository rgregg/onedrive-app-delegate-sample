using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OneDriveAppDelegateSample
{
    public interface IAppConfig
    {
        string ClientId { get; }
        string ClientSecret { get; }
        string RedirectUri { get; }
        string TokenServiceUri { get; }
        string AuthorizationServiceUri { get; }
    }

    public class ProductionAppConfig : IAppConfig
    {
        public virtual string ClientId { get { return "bd9da400-84c8-4ed5-9479-2f192b206f7c"; } }

        public virtual string ClientSecret { get { return "tB6PB/O/nyEUnMrj5WFVNWFOzOiM/g0ehEInpqA5mOI="; } }

        public virtual string RedirectUri
        {
            get
            {
#if DEBUG
                return "http://localhost:2007/adminauthresponse";
#else
                return "https://onedrive-app-delegate.azurewebsites.net/adminauthresponse";
#endif
            }
        }

        public virtual string TokenServiceUri
        {
            get { return "https://login.microsoftonline.com/common/oauth2/token"; }
        }

        public virtual string AuthorizationServiceUri
        {
            get { return "https://login.microsoftonline.com/common/oauth2/authorize"; }
        }
    }

    public class DogfoodAppConfig : ProductionAppConfig
    {
        public override string ClientId
        {
            get { return "3c6a4be0-4de6-4ee7-a4ed-be4f18ebdbe7"; }
        }

        public override string ClientSecret
        {
            get { return "+BaW3T+OnCZVc4Lg79tByErPOaQRvKtJVnCQsAJcLU8="; }
        }

        public override string TokenServiceUri
        {
            get { return "https://login.windows-ppe.net/common/oauth2/token"; }
        }

        public override string AuthorizationServiceUri
        {
            get { return "https://login.windows-ppe.net/common/oauth2/authorize"; }
        }
    }


}