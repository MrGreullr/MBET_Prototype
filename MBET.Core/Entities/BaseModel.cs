using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MBET.Core.Entities
{
    public abstract class BaseModel
    {
        /// <summary>
        /// Unique identifier for the entity (GUID, generated automatically).
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// Indicates whether the entity is active.
        /// Soft-delete (archived) if set to false.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // IAuditableEntity Implementation
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public Guid? LastModifiedBy { get; set; }
    }
}
