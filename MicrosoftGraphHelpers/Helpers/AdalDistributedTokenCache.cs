using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicrosoftGraphHelpers.Helpers
{
    /// <summary>
    /// Caches access and refresh tokens for Azure AD
    /// </summary>
    public class AdalDistributedTokenCache : TokenCache
    {
        private readonly IDistributedCache _distributedCache;
        private readonly string _objectId;
        private readonly string _tenantId;

        /// <summary>
        /// Constructs a token cache
        /// </summary>
        /// <param name="distributedCache">Distributed cache used for storing tokens</param>
        /// <param name="dataProtectionProvider">The protector provider for encrypting/decrypting the cached data</param>
        /// <param name="userId">The user's unique identifier</param>
        public AdalDistributedTokenCache(IDistributedCache distributedCache, string tenantId, string objectId = "")
        {
            _distributedCache = distributedCache;
            _objectId = objectId;
            _tenantId = tenantId;
            BeforeAccess = BeforeAccessNotification;
            AfterAccess = AfterAccessNotification;
        }

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            //Called before ADAL tries to access the cache,
            //so this is where we should read from the distibruted cache
            //It sucks that ADAL's API is synchronous, so we must do a blocking call here
            byte[] cachedData = _distributedCache.Get(GetCacheKey());

            if (cachedData != null)
            {
                //Decrypt and deserialize the cached data
                Deserialize(cachedData);
            }
            else
            {
                //Ensures the cache is cleared in TokenCache
                Deserialize(null);
            }
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            //Called after ADAL is done accessing the token cache
            if (HasStateChanged)
            {
                //In this case the cache state has changed, maybe a new token was written
                //So we encrypt and write the data to the distributed cache
                var data = Serialize();

                _distributedCache.Set(GetCacheKey(), data, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                });

                HasStateChanged = false;
            }
        }

        private string GetCacheKey() => $"TokenCache.{_tenantId}.{_objectId}";
    }
}
