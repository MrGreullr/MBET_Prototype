using MBET.Core.Entities;
using MBET.Core.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MBET.Web.Services
{
    public class ProductEditorService : IProductEditorService
    {
        private readonly IProductRepository _productRepo;
        private readonly IRepository<ProductImage> _imageRepo;
        private readonly IStorageService _storageService;
        private readonly ISnackbar _snackbar;

        // State
        public Product Model { get; private set; } = new Product();
        public List<Category> Categories { get; private set; } = new();
        public bool IsUploading { get; private set; }
        public bool IsEditMode { get; private set; }

        // UI Inputs
        public string PrimaryUrlInput { get; set; } = "";
        public string GalleryUrlInput { get; set; } = "";

        // Internal Tracking
        private List<ProductImage> _imagesToDelete = new();

        public ProductEditorService(
            IProductRepository productRepo,
            IRepository<ProductImage> imageRepo,
            IStorageService storageService,
            ISnackbar snackbar)
        {
            _productRepo = productRepo;
            _imageRepo = imageRepo;
            _storageService = storageService;
            _snackbar = snackbar;
        }

        public async Task InitializeAsync(Guid? productId)
        {
            Categories = (await _productRepo.GetCategoriesAsync()).ToList();

            if (productId.HasValue && productId.Value != Guid.Empty)
            {
                IsEditMode = true;
                var existing = await _productRepo.GetByIdAsync(productId.Value);
                if (existing != null)
                {
                    Model = existing;
                    var existingPrimary = Model.Images.FirstOrDefault(x => x.IsPrimary);
                    if (existingPrimary != null) PrimaryUrlInput = existingPrimary.ImageUrl;
                }
            }
            else
            {
                IsEditMode = false;
                Model = new Product();
                if (Categories.Any()) Model.CategoryId = Categories.First().Id;
            }
        }

        public async Task UploadPrimaryImageAsync(IBrowserFile file)
        {
            IsUploading = true;
            try
            {
                using var stream = file.OpenReadStream(15 * 1024 * 1024);
                // FIX: Added "products" folder name
                var url = await _storageService.UploadFileAsync(stream, file.Name, file.ContentType, "products");
                PrimaryUrlInput = url;
                _snackbar.Add("Image uploaded successfully", Severity.Success);
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Upload failed: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsUploading = false;
            }
        }

        public async Task UploadGalleryImagesAsync(IReadOnlyList<IBrowserFile> files)
        {
            IsUploading = true;
            try
            {
                foreach (var file in files)
                {
                    if (Model.Images.Count(x => !x.IsPrimary) < 5)
                    {
                        using var stream = file.OpenReadStream(15 * 1024 * 1024);
                        // FIX: Added "products" folder name
                        var url = await _storageService.UploadFileAsync(stream, file.Name, file.ContentType, "products");
                        Model.Images.Add(new ProductImage { ImageUrl = url, IsPrimary = false });
                    }
                }
                _snackbar.Add("Gallery images uploaded", Severity.Success);
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Upload failed: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsUploading = false;
            }
        }

        public void AddGalleryUrl()
        {
            if (!string.IsNullOrWhiteSpace(GalleryUrlInput))
            {
                Model.Images.Add(new ProductImage { ImageUrl = GalleryUrlInput, IsPrimary = false });
                GalleryUrlInput = "";
            }
        }

        public void RemoveGalleryImage(ProductImage img)
        {
            _imagesToDelete.Add(img);
            Model.Images.Remove(img);
        }

        public void ClearPrimaryImage()
        {
            PrimaryUrlInput = "";
        }

        public void AddSpec() => Model.Specifications.Add(new ProductSpecification());
        public void RemoveSpec(ProductSpecification spec) => Model.Specifications.Remove(spec);

        public async Task<Product> SaveAsync()
        {
            // 1. Logic: Handle Primary Image Updates
            var existingPrimary = Model.Images.FirstOrDefault(x => x.IsPrimary);

            if (existingPrimary != null)
            {
                if (string.IsNullOrWhiteSpace(PrimaryUrlInput))
                {
                    // Removed
                    _imagesToDelete.Add(existingPrimary);
                    Model.Images.Remove(existingPrimary);
                }
                else if (existingPrimary.ImageUrl != PrimaryUrlInput)
                {
                    // Changed
                    _imagesToDelete.Add(new ProductImage { ImageUrl = existingPrimary.ImageUrl });
                    existingPrimary.ImageUrl = PrimaryUrlInput;
                }
            }
            else if (!string.IsNullOrWhiteSpace(PrimaryUrlInput))
            {
                // New
                Model.Images.Add(new ProductImage { ImageUrl = PrimaryUrlInput, IsPrimary = true });
            }

            // 2. Database Persistence
            try
            {
                // A. Hard Delete Orphans
                foreach (var img in _imagesToDelete)
                {
                    if (img.Id != Guid.Empty) await _imageRepo.HardDeleteAsync(img.Id);
                }

                // B. Save Product
                if (IsEditMode)
                {
                    await _productRepo.UpdateProductAsync(Model);
                    _snackbar.Add("Product updated successfully", Severity.Success);
                }
                else
                {
                    await _productRepo.AddProductAsync(Model);
                    _snackbar.Add("Product created successfully", Severity.Success);
                }

                // 3. Disk Cleanup (Post-Save)
                foreach (var img in _imagesToDelete)
                {
                    if (!string.IsNullOrEmpty(img.ImageUrl))
                    {
                        await _storageService.DeleteFileAsync(img.ImageUrl);
                    }
                }

                return Model;
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error saving product: {ex.Message}", Severity.Error);
                throw;
            }
        }
    }
}