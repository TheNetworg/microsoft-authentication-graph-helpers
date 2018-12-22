using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace MicrosoftGraphHelpers.Services
{
    public class MicrosoftGraphFactory
    {
        public const string Resource = "https://graph.microsoft.com";

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
                var result = await authenticationContext.AcquireTokenSilentAsync(Resource, _clientCredential, new UserIdentifier(objectId, UserIdentifierType.UniqueId));

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(result.AccessTokenType, result.AccessToken);
            }));
        }
        public GraphServiceClient GetClientForApplication(string tenantId)
        {
            var authenticationContext = _adalFactory.GetAuthenticationContextForApplication(tenantId);

            return new GraphServiceClient(new DelegateAuthenticationProvider(async requestMessage =>
            {
                var result = await authenticationContext.AcquireTokenAsync(Resource, _clientCredential);

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(result.AccessTokenType, result.AccessToken);
            }));
        }
        public GraphServiceClient GetClientForApiUser(string accessToken, ClaimsPrincipal user)
        {
            string userName = user.FindFirst(ClaimTypes.Upn)?.Value ?? user.FindFirst(ClaimTypes.Email)?.Value;
            //TODO: Validate whether the token cache works properly or not...
            UserAssertion userAssertion = new UserAssertion(accessToken);
            
            var authenticationContext = _adalFactory.GetAuthenticationContextForUser(user);
            
            return new GraphServiceClient(new DelegateAuthenticationProvider(async requestMessage =>
            {
                var result = await authenticationContext.AcquireTokenAsync(Resource, _clientCredential, userAssertion);

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(result.AccessTokenType, result.AccessToken);
            }));
        }
    }
}
