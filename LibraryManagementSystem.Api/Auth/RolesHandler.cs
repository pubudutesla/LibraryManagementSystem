using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagementSystem.Api.Auth
{

    public class RolesHandler : AuthorizationHandler<RolesRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesRequirement requirement)
        {
            var userRoles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);

            if (userRoles.Any(role => requirement.Roles.Contains(role)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

