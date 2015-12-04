using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;

namespace OneDriveAppDelegateSample.Utility
{
    public static class AuthHelper
    {
        public static async Task<string> GetAccessTokenAsync(string tenantId, string resource, bool useDogfood)
        {
            IAppConfig app = useDogfood ? new DogfoodAppConfig() : new ProductionAppConfig();

            string authority = app.AuthorizationServiceUri.Replace("common", tenantId);
            AuthenticationContext authContext = new AuthenticationContext(authority, false);

            X509Certificate2 cert = new X509Certificate2(
                Properties.Resources.OneDriveAppCertPrivate,
                // Password for the PFX file
                "OneDriveApp",
                X509KeyStorageFlags.MachineKeySet);

            ClientAssertionCertificate cac =
                new ClientAssertionCertificate(app.ClientId, cert);

            var authenticationResult = await authContext.AcquireTokenAsync(resource, cac);

            string accessToken = authenticationResult.AccessToken;
            return accessToken;
        }

    }
}