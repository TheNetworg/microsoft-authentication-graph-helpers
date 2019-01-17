using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MicrosoftGraphHelpers.Common;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace MicrosoftGraphHelpers.Services
{
    public class AzureAdGraphFactory : IAzureADProtectedResourceFactory<ActiveDirectoryClient>
    {
        public const string Resource = "https://graph.windows.net/";

        private readonly AdalFactory _adalFactory;
        private readonly ClientCredential _clientCredential;
        public AzureAdGraphFactory(AdalFactory adalFactory, ClientCredential clientCredential)
        {
            _adalFactory = adalFactory;
            _clientCredential = clientCredential;
        }
        public ActiveDirectoryClient GetClientForUser(ClaimsPrincipal user)
        {
            var authenticationContext = _adalFactory.GetAuthenticationContextForUser(user);
            var objectId = user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            var tenantId = user.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;

            var servicePointUri = new Uri(Resource);
            var serviceRoot = new Uri(servicePointUri, tenantId);

            return new ActiveDirectoryClient(serviceRoot, async () =>
            {
                var result = await authenticationContext.AcquireTokenSilentAsync(Resource, _clientCredential, new UserIdentifier(objectId, UserIdentifierType.UniqueId));

                return result.AccessToken;
            });
        }
        public ActiveDirectoryClient GetClientForApplication(string tenantId)
        {
            var authenticationContext = _adalFactory.GetAuthenticationContextForApplication(tenantId);

            var servicePointUri = new Uri($"{Resource}{tenantId}");

            return new ActiveDirectoryClient(servicePointUri, async () =>
            {
                var result = await authenticationContext.AcquireTokenAsync(Resource, _clientCredential);

                return result.AccessToken;
            });
        }
        public ActiveDirectoryClient GetClientForApiUser(string accessToken, ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }
    }
}
