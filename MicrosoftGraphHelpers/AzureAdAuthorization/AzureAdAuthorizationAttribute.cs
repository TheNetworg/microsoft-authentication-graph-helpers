using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftGraphHelpers.AzureAdAuthorization
{
    public class AzureAdAuthorizationAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string[] Groups { get; private set; }
        public string[] Roles { get; private set; }
        public AzureAdAuthorizationAttribute(string[] groups = null, string[] roles = null)
        {
            Groups = groups;
            Roles = roles;
        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            var authorizationRequirement = new AzureAdAuthorizationRequirement(Groups, Roles);

            var result = await authorizationService.AuthorizeAsync(context.HttpContext.User, null, authorizationRequirement);

            if (result.Failure != null)
            {
                context.Result = new ChallengeResult();
            }
            else if (!result.Succeeded)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
