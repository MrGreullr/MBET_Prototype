using MBET.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MBET.Core.Entities
{
    /// <summary>
    /// Represents a product category in the MBET ecosystem.
    /// </summary>
    public class Category : BaseModel
    {

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Icon { get; set; } // MudBlazor Icon string

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
    /// <summary>
    /// The primary product entity.
    /// Note: PII encryption attribute [Encrypted] is reserved for User entities.
    /// </summary>
    /// 
    public class Brand : BaseModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public class ProductSpecification : BaseModel
    {
        [Required, StringLength(100)]
        public string Key { get; set; } = string.Empty; // e.g., "VRAM"
        [Required, StringLength(500)]
        public string Value { get; set; } = string.Empty; // e.g., "24GB G6X"

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
    }

    public class Product : BaseModel, IAuditableEntity
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        public int StockQuantity { get; set; }

        public bool IsPriority { get; set; } // For "Priority Stream" badging
        public bool IsVisible { get; set; } = true;
        public bool IsOutOfStock { get; set; } = false;
        
        public double Rating { get; set; } = 5.0;
        public int ReviewCount { get; set; }
        // Foreign Keys
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }

        public Guid? BrandId { get; set; }
        public Brand? Brand { get; set; }

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();

        [NotMapped]
        public bool IsNew => (DateTime.UtcNow - CreatedAt).TotalDays <= 7;
        [NotMapped]
        public bool IsOnSale => OriginalPrice.HasValue && OriginalPrice > Price;
    }

    /// <summary>
    /// Gallery images for products.
    /// </summary>
    public class ProductImage : BaseModel, IAuditableEntity
    {
        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
