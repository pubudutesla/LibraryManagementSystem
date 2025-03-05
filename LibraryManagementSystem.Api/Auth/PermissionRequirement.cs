using System;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagementSystem.Api.Auth
{

    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}

