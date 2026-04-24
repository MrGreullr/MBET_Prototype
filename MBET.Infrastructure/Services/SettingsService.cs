using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MBET.Infrastructure.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IDbContextFactory<MBETDbContext> _contextFactory;

        // Simple in-memory cache
        private static GlobalSettings? _cachedSettings;

        public SettingsService(IDbContextFactory<MBETDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<GlobalSettings> GetSettingsAsync()
        {
            // Optional: You can invalidate cache by setting _cachedSettings = null elsewhere if needed
            if (_cachedSettings != null) return _cachedSettings;

            using var context = await _contextFactory.CreateDbContextAsync();

            // FIX: Added .Include(x => x.Features) to load the list
            var settings = await context.GlobalSettings
                                        .Include(x => x.Features)
                                        .FirstOrDefaultAsync();

            if (settings == null)
            {
                // Create defaults if missing (Auto-seed logic)
                settings = new GlobalSettings();
                context.GlobalSettings.Add(settings);
                await context.SaveChangesAsync();
            }

            _cachedSettings = settings;
            return _cachedSettings;
        }

        public async Task UpdateSettingsAsync(GlobalSettings settings)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // 1. Fetch the EXISTING entity from DB to track it properly
            var existing = await context.GlobalSettings
                                        .Include(x => x.Features)
                                        .FirstOrDefaultAsync(x => x.Id == 1);

            if (existing != null)
            {
                // 2. Update scalar properties (Branding, Toggles, etc.)
                // This copies simple values from 'settings' (UI) to 'existing' (DB context)
                context.Entry(existing).CurrentValues.SetValues(settings);

                // 3. Handle Collection Updates (The tricky part)

                // A. Delete items: Removed from UI? Remove from DB.
                foreach (var existingFeature in existing.Features.ToList())
                {
                    if (!settings.Features.Any(f => f.Id == existingFeature.Id && f.Id != 0))
                    {
                        context.Remove(existingFeature);
                    }
                }

                // B. Add/Update items
                foreach (var featureModel in settings.Features)
                {
                    // Is this a new item (Id == 0) or existing?
                    var existingFeature = existing.Features
                        .SingleOrDefault(f => f.Id == featureModel.Id && f.Id != 0);

                    if (existingFeature != null)
                    {
                        // Update existing child
                        context.Entry(existingFeature).CurrentValues.SetValues(featureModel);
                    }
                    else
                    {
                        // Insert new child
                        var newFeature = new SiteFeature
                        {
                            Title = featureModel.Title,
                            Description = featureModel.Description,
                            Icon = featureModel.Icon,
                            Order = featureModel.Order
                        };
                        existing.Features.Add(newFeature);
                    }
                }

                await context.SaveChangesAsync();

                // Update the static cache with the fresh data
                _cachedSettings = existing;
            }
            else
            {
                // Fallback: Just update blindly if row 1 doesn't exist (Unlikely)
                settings.Id = 1;
                context.GlobalSettings.Update(settings);
                await context.SaveChangesAsync();
                _cachedSettings = settings;
            }
        }
    }
}