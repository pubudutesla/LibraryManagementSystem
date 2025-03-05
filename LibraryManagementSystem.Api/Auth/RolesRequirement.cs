using System;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagementSystem.Api.Auth
{
    public class RolesRequirement : IAuthorizationRequirement
    {
        public string[] Roles { get; }

        public RolesRequirement(params string[] roles)
        {
            Roles = roles;
        }
    }
}

