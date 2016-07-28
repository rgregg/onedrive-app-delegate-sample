using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;

namespace OneDriveAppDelegateSample.Utility
{
    internal static class AuthHelper
    {
        public static async Task<Models.CachedAccessToken> GetAccessTokenAsync(string tenantId, string resource, bool useDogfood)
        {
            IAppConfig app = useDogfood ? new DogfoodAppConfig() : new ProductionAppConfig();

            string authority = app.AuthorizationServiceUri.Replace("common", tenantId);
            AuthenticationContext authContext = new AuthenticationContext(authority, false);


            var certDataBase64 = ConfigurationManager.AppSettings["PrivateCertificateBase64"];
            var certBinaryData = Convert.FromBase64String(certDataBase64);
            var password = ConfigurationManager.AppSettings["CertificatePassword"];

            X509Certificate2 cert = new X509Certificate2(certBinaryData, password, X509KeyStorageFlags.MachineKeySet);

            var clientAppId = ConfigurationManager.AppSettings["ClientAppId"];

            ClientAssertionCertificate cac = new ClientAssertionCertificate(clientAppId, cert);

            var authenticationResult = await authContext.AcquireTokenAsync(resource, cac);
            return new Models.CachedAccessToken(authenticationResult.AccessToken, authenticationResult.ExpiresOn);
        }

    }
}