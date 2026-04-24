using System;
using System.Collections.Generic;
using System.Text;

namespace MBET.Core.Interfaces
{
    /// <summary>
    /// Abstraction to get the current logged-in user's ID without binding Core to ASP.NET Context.
    /// This will be implemented in the Web project.
    /// </summary>
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? UserName { get; }
    }

    /// <summary>
    /// Entities implementing this interface will automatically have their timestamps 
    /// and user tracking fields updated by the DbContext.
    /// </summary>
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        Guid? CreatedBy { get; set; }
        DateTime? LastModifiedAt { get; set; }
        Guid? LastModifiedBy { get; set; }
    }
}
