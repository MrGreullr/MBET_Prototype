using MBET.Core.Entities;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MBET.Web.Services
{
    public interface IProductEditorService
    {
        // State Properties
        Product Model { get; }
        List<Category> Categories { get; }
        bool IsUploading { get; }
        bool IsEditMode { get; }

        // UI Bindings
        string PrimaryUrlInput { get; set; }
        string GalleryUrlInput { get; set; }

        // Lifecycle
        Task InitializeAsync(Guid? productId);

        // Actions
        Task UploadPrimaryImageAsync(IBrowserFile file);
        Task UploadGalleryImagesAsync(IReadOnlyList<IBrowserFile> files);
        void AddGalleryUrl();
        void RemoveGalleryImage(ProductImage img);
        void ClearPrimaryImage();

        void AddSpec();
        void RemoveSpec(ProductSpecification spec);

        // Persistence
        /// <summary>
        /// Saves the product and handles image cleanup. Returns the saved Product on success.
        /// </summary>
        Task<Product> SaveAsync();
    }
}