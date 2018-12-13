using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace MicrosoftGraphHelpers.Services
{
    public class MicrosoftGraphFactory
    {
        private readonly AdalFactory _adalFactory;
        private readonly ClientCredential _clientCredential;
        public MicrosoftGraphFactory(AdalFactory adalFactory, ClientCredential clientCredential)
        {
            _adalFactory = adalFactory;
            _clientCredential = clientCredential;
        }
        public GraphServiceClient GetClientForUser(ClaimsPrincipal user)
        {
            var authenticationContext = _adalFactory.GetAuthenticationContextForUser(user);
            var objectId = user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            return new GraphServiceClient(new DelegateAuthenticationProvider(async requestMessage =>
            {
                var result = await authenticationContext.AcquireTokenSilentAsync("https://graph.microsoft.com", _clientCredential, new UserIdentifier(objectId, UserIdentifierType.UniqueId));

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(result.AccessTokenType, result.AccessToken);
            }));
        }
        public GraphServiceClient GetClientForApplication(string tenantId)
        {
            var authenticationContext = _adalFactory.GetAuthenticationContextForApplication(tenantId);

            return new GraphServiceClient(new DelegateAuthenticationProvider(async requestMessage =>
            {
                var result = await authenticationContext.AcquireTokenAsync("https://graph.microsoft.com", _clientCredential);

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(result.AccessTokenType, result.AccessToken);
            }));
        }
    }
}
