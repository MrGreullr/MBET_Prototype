using Microsoft.AspNetCore.Identity;
using System;

namespace MBET.Core.Entities.Identity
{
    /// <summary>
    /// Represents a security role (e.g., "SuperAdmin", "ShopManager", "Customer").
    /// </summary>
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string Description { get; set; } = string.Empty;
        public bool IsSystemRole { get; set; } = false; // If true, cannot be deleted via UI

        public ApplicationRole() : base() { }

        public ApplicationRole(string roleName, string description = "", bool isSystem = false) : base(roleName)
        {
            Description = description;
            IsSystemRole = isSystem;
        }
    }
}