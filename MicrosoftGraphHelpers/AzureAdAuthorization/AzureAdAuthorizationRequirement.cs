using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicrosoftGraphHelpers.AzureAdAuthorization
{
    public class AzureAdAuthorizationRequirement : IAuthorizationRequirement
    {
        public string[] Groups { get; private set; }
        public string[] Roles { get; private set; }

        public AzureAdAuthorizationRequirement(string[] groups = null, string[] roles = null)
        {
            Groups = groups;
            Roles = roles;
        }
    }
}
