using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Web.Services;
using Microsoft.AspNetCore.Components.Forms;
using Moq;
using MudBlazor;
using Xunit;

namespace MBET.Tests.Services
{
    public class ProductEditorServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepo;
        private readonly Mock<IRepository<ProductImage>> _mockImageRepo;
        private readonly Mock<IStorageService> _mockStorageService;
        private readonly Mock<ISnackbar> _mockSnackbar;
        private readonly ProductEditorService _service;

        public ProductEditorServiceTests()
        {
            _mockProductRepo = new Mock<IProductRepository>();
            _mockImageRepo = new Mock<IRepository<ProductImage>>();
            _mockStorageService = new Mock<IStorageService>();
            _mockSnackbar = new Mock<ISnackbar>();

            _service = new ProductEditorService(
                _mockProductRepo.Object,
                _mockImageRepo.Object,
                _mockStorageService.Object,
                _mockSnackbar.Object
            );
        }

        [Fact]
        public async Task InitializeAsync_NewProduct_SetsDefaultCategory()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "GPUs" }
            };
            _mockProductRepo.Setup(x => x.GetCategoriesAsync()).ReturnsAsync(categories);

            // Act
            await _service.InitializeAsync(null);

            // Assert
            Assert.False(_service.IsEditMode);
            Assert.NotNull(_service.Model);
            Assert.Equal(categories.First().Id, _service.Model.CategoryId);
            Assert.Single(_service.Categories);
        }

        [Fact]
        public async Task InitializeAsync_EditMode_LoadsExistingData()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var existingProduct = new Product
            {
                Id = productId,
                Title = "RTX 4090",
                Images = new List<ProductImage>
                {
                    new ProductImage { ImageUrl = "old.jpg", IsPrimary = true }
                }
            };

            _mockProductRepo.Setup(x => x.GetCategoriesAsync()).ReturnsAsync(new List<Category>());
            _mockProductRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

            // Act
            await _service.InitializeAsync(productId);

            // Assert
            Assert.True(_service.IsEditMode);
            Assert.Equal("RTX 4090", _service.Model.Title);
            Assert.Equal("old.jpg", _service.PrimaryUrlInput); // Input should bind to existing primary
        }

        [Fact]
        public async Task SaveAsync_SmartUpdate_UpdatesEntityAndCleansDisk()
        {
            // Arrange: Edit mode with an existing image
            var productId = Guid.NewGuid();
            var imageId = Guid.NewGuid();
            var existingProduct = new Product
            {
                Id = productId,
                Images = new List<ProductImage>
                {
                    new ProductImage { Id = imageId, ImageUrl = "old_image.jpg", IsPrimary = true }
                }
            };

            _mockProductRepo.Setup(x => x.GetCategoriesAsync()).ReturnsAsync(new List<Category>());
            _mockProductRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

            // Initialize logic
            await _service.InitializeAsync(productId);

            // Act: Change the image input
            _service.PrimaryUrlInput = "new_image.jpg";
            await _service.SaveAsync();

            // Assert 1: Database Logic
            // It should NOT delete the entity (Smart Update)
            _mockImageRepo.Verify(x => x.HardDeleteAsync(It.IsAny<Guid>()), Times.Never);

            // It should Update the product with the new URL in the SAME entity object
            _mockProductRepo.Verify(x => x.UpdateProductAsync(It.Is<Product>(p =>
                p.Images.First().ImageUrl == "new_image.jpg" &&
                p.Images.First().Id == imageId // ID preserved
            )), Times.Once);

            // Assert 2: Disk Cleanup
            // It should delete the OLD file from disk
            _mockStorageService.Verify(x => x.DeleteFileAsync("old_image.jpg"), Times.Once);
        }

        [Fact]
        public async Task SaveAsync_RemoveImage_DeletesEntityAndFile()
        {
            // Arrange: Existing image
            var productId = Guid.NewGuid();
            var imageId = Guid.NewGuid();
            var existingProduct = new Product
            {
                Id = productId,
                Images = new List<ProductImage>
                {
                    new ProductImage { Id = imageId, ImageUrl = "old_image.jpg", IsPrimary = true }
                }
            };

            _mockProductRepo.Setup(x => x.GetCategoriesAsync()).ReturnsAsync(new List<Category>());
            _mockProductRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(existingProduct);
            await _service.InitializeAsync(productId);

            // Act: Clear the input
            _service.PrimaryUrlInput = "";
            await _service.SaveAsync();

            // Assert
            // Should delete entity from DB
            _mockImageRepo.Verify(x => x.HardDeleteAsync(imageId), Times.Once);

            // Should delete file from disk
            _mockStorageService.Verify(x => x.DeleteFileAsync("old_image.jpg"), Times.Once);

            // Product update should be called (to save other fields)
            _mockProductRepo.Verify(x => x.UpdateProductAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task SaveAsync_NewProduct_AddsToRepo()
        {
            // Arrange
            _mockProductRepo.Setup(x => x.GetCategoriesAsync()).ReturnsAsync(new List<Category>());
            await _service.InitializeAsync(null);
            _service.PrimaryUrlInput = "brand_new.jpg";

            // Act
            await _service.SaveAsync();

            // Assert
            _mockProductRepo.Verify(x => x.AddProductAsync(It.Is<Product>(p =>
                p.Images.First().ImageUrl == "brand_new.jpg" &&
                p.Images.First().IsPrimary == true
            )), Times.Once);
        }
    }
}