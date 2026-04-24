using System.IO;
using System.Threading.Tasks;

namespace MBET.Core.Interfaces
{
    public interface IStorageService
    {
        /// <summary>
        /// Uploads a file to the storage provider.
        /// </summary>
        /// <param name="fileStream">The file stream.</param>
        /// <param name="fileName">Original file name (for extension).</param>
        /// <param name="contentType">MIME type.</param>
        /// <param name="folderName">The target sub-folder (e.g., "products", "settings").</param>
        /// <returns>The relative URL to the file.</returns>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folderName = "misc");

        Task<bool> DeleteFileAsync(string fileUrl);
    }
}