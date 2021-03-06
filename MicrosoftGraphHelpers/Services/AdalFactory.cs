﻿using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace MicrosoftGraphHelpers.Services
{

    public class AdalFactory
    {
        private readonly TokenCacheFactory _tokenCacheFactory;
        private readonly string _authority;
        public AdalFactory(TokenCacheFactory tokenCacheFactory)
        {
            _tokenCacheFactory = tokenCacheFactory;
            _authority = "https://login.microsoftonline.com/common/";
        }
        public AuthenticationContext GetAuthenticationContextForUser(ClaimsPrincipal user)
        {
            var tokenCache = _tokenCacheFactory.CreateForUser(user);
            var tenantId = user.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");
            return new AuthenticationContext(_authority.Replace("common", tenantId), tokenCache);
        }
        public AuthenticationContext GetAuthenticationContextForApplication(string tenantId)
        {
            var tokenCache = _tokenCacheFactory.CreateForApplication(tenantId);
            return new AuthenticationContext(_authority.Replace("common", tenantId), tokenCache);
        }
    }
}
