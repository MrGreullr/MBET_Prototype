using MBET.Core.Entities;
using System.Threading.Tasks;

namespace MBET.Core.Interfaces
{
    public interface ISettingsService
    {
        /// <summary>
        /// Returns the current global settings. 
        /// Should return defaults if database is empty.
        /// </summary>
        Task<GlobalSettings> GetSettingsAsync();

        /// <summary>
        /// Updates the settings and clears any cache.
        /// </summary>
        Task UpdateSettingsAsync(GlobalSettings settings);
    }
}