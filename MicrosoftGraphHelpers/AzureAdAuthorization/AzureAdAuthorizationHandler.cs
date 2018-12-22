using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MicrosoftGraphHelpers.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftGraphHelpers.AzureAdAuthorization
{
    public class AzureAdAuthorizationHandler : AuthorizationHandler<AzureAdAuthorizationRequirement>
    {
        private readonly MicrosoftGraphFactory _microsoftGraphFactory;
        public AzureAdAuthorizationHandler(MicrosoftGraphFactory microsoftGraphFactory)
        {
            _microsoftGraphFactory = microsoftGraphFactory;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AzureAdAuthorizationRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                context.Fail();
            }

            var userId = context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            var graphClient = _microsoftGraphFactory.GetClientForUser(context.User);

            var result = false;

            if (requirement.Roles != null)
            {
                var graphRoles = await graphClient.DirectoryRoles.Request().GetAsync();
                foreach (var role in requirement.Roles)
                {
                    var graphRole = graphRoles.Where(x => x.RoleTemplateId == role).FirstOrDefault();
                    if (graphRole != null)
                    {
                        var graphRoleMembers = await graphClient.DirectoryRoles[graphRole.Id].Members.Request().GetAsync();
                        if (graphRoleMembers.Where(x => x.Id == userId).Any())
                        {
                            result = true;
                        }
                    }
                }
            }

            if (requirement.Groups != null)
            {
                var memberGroups = await graphClient.Users[userId].CheckMemberGroups(requirement.Groups).Request().PostAsync();
                if (memberGroups.Count > 0)
                {
                    result = true;
                }
            }

            if (result)
            {
                context.Succeed(requirement);
            }
        }
    }
}
