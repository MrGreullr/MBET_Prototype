using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MBET.Core.Entities
{
    /// <summary>
    /// Represents a detailed record of a change in the system.
    /// Used for critical e-commerce events (e.g., Price changes, Order status updates).
    /// </summary>
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Guid { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public string Type { get; set; } = string.Empty; // "Create", "Update", "Delete"
        public string TableName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? OldValues { get; set; } // JSON
        public string? NewValues { get; set; } // JSON
        public string? AffectedColumns { get; set; } // JSON array
        public string? PrimaryKey { get; set; } // JSON
    }
}
