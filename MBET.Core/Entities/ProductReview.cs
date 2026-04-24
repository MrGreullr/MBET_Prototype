using MBET.Core.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace MBET.Core.Entities
{
    public class ProductReview : BaseModel, IAuditableEntity
    {
        public Guid ProductId { get; set; }
        // Optional navigation property if needed, but keeping it light avoids cycle issues
        // public Product? Product { get; set; } 

        public Guid UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = "Anonymous";

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(2000)]
        public string? Comment { get; set; }
    }
}