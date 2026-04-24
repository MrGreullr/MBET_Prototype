using MBET.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MBET.Infrastructure.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LocalStorageService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalStorageService(
            IWebHostEnvironment environment,
            ILogger<LocalStorageService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folderName = "misc")
        {
            try
            {
                // 1. Sanitize folder name to prevent directory traversal
                var safeFolder = folderName.Replace("..", "").Trim('/', '\\');

                // 2. Create structure: uploads/{folderName}/{YYYY-MM}/
                var dateFolder = DateTime.UtcNow.ToString("yyyy-MM");
                var subFolder = Path.Combine("uploads", safeFolder, dateFolder);
                var storagePath = Path.Combine(_environment.WebRootPath, subFolder);

                if (!Directory.Exists(storagePath))
                    Directory.CreateDirectory(storagePath);

                // 3. Generate Unique Filename
                var extension = Path.GetExtension(fileName);
                var uniqueName = $"{Guid.NewGuid()}{extension}";
                var fullPath = Path.Combine(storagePath, uniqueName);

                // 4. Stream to disk
                using (var outputStream = new FileStream(fullPath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(outputStream);
                }

                // 5. Return Root-Relative URL (Added leading slash back)
                // This ensures the browser looks in the website root (e.g., domain.com/uploads/...)
                // regardless of whether the user is on /admin/settings or /catalog.
                var relativePath = Path.Combine(subFolder, uniqueName).Replace("\\", "/");
                return $"/{relativePath}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Local upload failed");
                throw;
            }
        }

        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl)) return Task.FromResult(false);

                // Handle both relative "uploads/..." and absolute "/uploads/..." input formats
                var relativePath = fileUrl.TrimStart('/', '\\');
                var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"Deleted file: {fullPath}");
                    return Task.FromResult(true);
                }

                _logger.LogWarning($"File not found for deletion: {fullPath}");
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {fileUrl}");
                return Task.FromResult(false);
            }
        }
    }
}