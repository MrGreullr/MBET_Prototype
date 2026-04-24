using MBET.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;

namespace MBET.Core.Entities.Identity
{
    public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? ProfilePictureUrl { get; set; }

        // E-Commerce Specifics
        public bool IsActive { get; set; } = true; // Customers are active by default
        public bool IsVIP { get; set; } = false;
        public bool IsBanned { get; set; } = false;
        public DateTime BanDate { get; set; }
        public decimal BanDuratyionDays { get; set; }
        public string PreferredLanguage { get; set; } = "en-US";
        public Guid? TenantId { get; set; } // Kept for scalability/grouping

        // Shipping Address
        public string? ShippingStreet { get; set; }
        public string? ShippingCity { get; set; }
        public string? ShippingState { get; set; }
        public string? ShippingZipCode { get; set; }
        public string? ShippingCountry { get; set; }


        // Billing Address (Separate from Shipping)
        public string? BillingStreet { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingZipCode { get; set; }
        public string? BillingCountry { get; set; }

        // Security / Auth
        public DateTime? LastLogin { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // IAuditableEntity Implementation
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public Guid? LastModifiedBy { get; set; }
    }
}