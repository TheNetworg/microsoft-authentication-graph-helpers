using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MicrosoftGraphHelpers.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace MicrosoftGraphHelpers.Common
{
    public interface IAzureADProtectedResourceFactory<T>
    {
        T GetClientForUser(ClaimsPrincipal user);
        T GetClientForApplication(string tenantId);
        T GetClientForApiUser(string accessToken, ClaimsPrincipal user);
    }
}
