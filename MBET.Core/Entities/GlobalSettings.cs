using MBET.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MBET.Core.Entities
{
    public enum ProductDisplayMode
    {
        Newest,
        Priority,
        Random
    }

    public class GlobalSettings : IAuditableEntity
    {
        public int Id { get; set; } // Always 1

        // --- 1. BRANDING & SEO ---
        [Required, MaxLength(100)]
        public string SiteName { get; set; } = "Modular Blazor E-Commerce Template Solution";
        [MaxLength(300)]
        public string SiteDescription { get; set; } = "Premium Hardware & Electronics Store";
        public string LogoUrl { get; set; } = "NDShop.png";
        public int LogoHeight { get; set; } = 80;
        public string FaviconUrl { get; set; } = "/favicon.ico";

        // --- 2. HERO SECTION ---
        public string HeroTitle { get; set; } = "HeroTitleStart";
        public string HeroHighlightText { get; set; } = "HeroTitleEnd";
        public string HeroSubtitle { get; set; } = "HeroSubtitle";
        public string HeroDescription { get; set; } = "HeroDescription";
        public string HeroImageUrl { get; set; } = "https://lh3.googleusercontent.com/aida-public/AB6AXuC8A4PfQ1hkNh1EQ8dN3yI71oGPB-tTvlCrAgxEM_154ZueFyD78Qp2VNUE9jOFvUIgnhvgjgYouHhtQiI1-t4ryUHTYd-AhqXqy2tsrucGKEbYXFi__hUbjTEaQmD_iTRijOPt4pWIKn8J85PZ6_C-bOOPESXHnFy0Rb_YCFYsErfHeDT22iv3akM918bT_5TInQSQgUIt7hYujIH16TVDZ1RJC7WrQaASYKw2aO4lBM3P-Q1NWwfnihbyy4FoM4zgfAQtfWFlJFeE";
        public string HeroCtaText { get; set; } = "Explore";
        public string HeroCtaLink { get; set; } = "/catalog";
        public bool EnableHeroOverlay { get; set; } = true;

        // --- 3. LANDING PAGE & FEATURES ---
        public bool ShowFeaturesSection { get; set; } = true;
        public bool ShowFreshDropsSection { get; set; } = true; // NEW: Independent control

        // Dynamic Features List
        public virtual List<SiteFeature> Features { get; set; } = new();

        public ProductDisplayMode LandingDisplayMode { get; set; } = ProductDisplayMode.Priority;
        public int LandingProductCount { get; set; } = 4;

        // --- 4. CONTACT & SOCIAL ---
        [EmailAddress]
        public string SupportEmail { get; set; } = "support@mbet.io";
        [Phone]
        public string SupportPhone { get; set; } = "+213 555 000 000";
        public string PhysicalAddress { get; set; } = "Tech District, Algiers, Algeria";
        public string FooterBio { get; set; } = "Description";
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; } = "https://instagram.com";
        public string? TwitterUrl { get; set; } = "https://twitter.com";
        public string? LinkedInUrl { get; set; }
        public string? YoutubeUrl { get; set; }
        public string? GitHubUrl { get; set; } = "https://github.com";

        // --- 5. BUSINESS LOGIC ---
        public string CurrencySymbol { get; set; } = "DZD";
        public decimal DefaultTaxRate { get; set; } = 0.19m;
        public decimal FreeShippingThreshold { get; set; }
        public bool IsMaintenanceMode { get; set; } = false;
        public bool AllowNewRegistrations { get; set; } = true;

        // --- 6. EMAIL ---
        public string SenderName { get; set; } = "MBET Notifications";
        [EmailAddress]
        public string NoReplyEmail { get; set; } = "no-reply@mbet.io";

        // --- 7. SHIPPING & DELIVERY ---
        public bool EnableHomeDelivery { get; set; } = true;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseHomeDeliveryFee { get; set; } = 800m;
        public bool EnablePickupDelivery { get; set; } = true;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePickupDeliveryFee { get; set; } = 400m;

        [Column(TypeName = "decimal(18,2)")] 
        public decimal WeightMultiplierFee { get; set; } = 50m; // Fee per Kg

        [Column(TypeName = "decimal(18,2)")]
        public decimal DistanceMultiplierFee { get; set; } = 10m; // Fee per Km

        // --- AUDIT ---
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public Guid? LastModifiedBy { get; set; }
    }

    public class SiteFeature
    {
        public int Id { get; set; }
        public string Title { get; set; } = "Feature Title";
        public string Description { get; set; } = "Feature Description";
        // Stores the MudBlazor Icon String (e.g. "Icons.Material.Filled.Bolt")
        public string Icon { get; set; } = "Icons.Material.Filled.Bolt";
        public int Order { get; set; }
    }
}