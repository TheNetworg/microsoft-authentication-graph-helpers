using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MicrosoftGraphHelpers.Helpers;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace MicrosoftGraphHelpers.Services
{
    public class TokenCacheFactory
    {
        private readonly IDistributedCache _distributedCache;
        //Token cache is cached in-memory in this instance to avoid loading data multiple times during the request
        //For this reason this factory should always be registered as Scoped
        private TokenCache _cachedTokenCache;
        private string _objectId;
        private string _tenantId;

        public TokenCacheFactory(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }
        public TokenCache CreateForUser(ClaimsPrincipal user)
        {
            var objectId = user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            var tenantId = user.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;

            if (_cachedTokenCache != null)
            {
                // Guard for accidental re-use across requests
                if (objectId != _objectId && tenantId != _tenantId)
                {
                    throw new Exception("The cached token cache is for a different user! Make sure the token cache factory is registered as Scoped!");
                }

                return _cachedTokenCache;
            }

            _cachedTokenCache = new AdalDistributedTokenCache(_distributedCache, tenantId, objectId);
            _objectId = objectId;
            _tenantId = tenantId;
            return _cachedTokenCache;
        }
        public TokenCache CreateForApplication(string tenantId)
        {
            return new AdalDistributedTokenCache(_distributedCache, tenantId);
        }
    }
}
