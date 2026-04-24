using MBET.Core.Entities;
using MBET.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using MBET.Core.Entities.Identity;

namespace MBET.Core.Entities
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        Returned
    }

    public enum PaymentStatus
    {
        Unpaid,
        Paid,
        Refunded,
        Failed
    }


    public class Order : BaseModel, IAuditableEntity
    {
        public string OrderNumber { get; set; } = string.Empty;

        // PII - Encrypted in Infrastructure via ValueConverter
        public string? ShippingAddress { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }

        // NEW: Stores "Home" or "Pickup"
        public string DeliveryMethod { get; set; } = "Home";

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotal { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem : BaseModel
    {
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Price at time of purchase
    }
}
