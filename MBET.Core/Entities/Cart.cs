using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MBET.Core.Entities;
using MBET.Core.Entities.Identity;

namespace MBET.Infrastructure.Entities
{
    /// <summary>
    /// Represents a user's shopping cart.
    /// </summary>
    public class Cart : BaseModel
    {
        public Guid UserId { get; set; }
        // Optional: Link to ApplicationUser if needed
        public ApplicationUser? User { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }

    /// <summary>
    /// Represents an individual line item in the cart.
    /// </summary>
    public class CartItem : BaseModel
    {
        public Guid CartId { get; set; }
        public Cart? Cart { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }

        /// <summary>
        /// Snapshot of the price at the time of adding to cart.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}