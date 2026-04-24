using System;

namespace MBET.Core.Interfaces
{
    /// <summary>
    /// Interface to enforce Soft Delete capabilities on Entities.
    /// </summary>
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTimeOffset? DeletedAt { get; set; }
    }
}