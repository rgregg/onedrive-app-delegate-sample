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
        public virtual string ClientId { get { return System.Configuration.ConfigurationManager.AppSettings["ClientAppId"]; } }

        public virtual string ClientSecret { get { return System.Configuration.ConfigurationManager.AppSettings["ClientAppSecret"]; } }

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

}