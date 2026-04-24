using MBET.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace MBET.Tests.Core
{
    public class ModelValidationTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void Product_ShouldBeInvalid_WhenTitleIsMissing()
        {
            // Arrange
            var product = new Product
            {
                Price = 100,
                StockQuantity = 10,
                Title = "" // Invalid: Required
            };

            // Act
            var errors = ValidateModel(product);

            // Assert
            Assert.Contains(errors, v => v.MemberNames.Contains(nameof(Product.Title)));
        }

        [Fact]
        public void Product_ShouldBeInvalid_WhenTitleExceedsLength()
        {
            // Arrange
            var product = new Product
            {
                Title = new string('A', 201), // Max is 200
                Price = 100
            };

            // Act
            var errors = ValidateModel(product);

            // Assert
            Assert.NotEmpty(errors);
        }

        [Fact]
        public void GlobalSettings_ShouldBeInvalid_WhenSiteNameIsMissing()
        {
            // Arrange
            var settings = new GlobalSettings
            {
                SiteName = null!, // Invalid
                SupportEmail = "valid@test.com"
            };

            // Act
            var errors = ValidateModel(settings);

            // Assert
            Assert.Contains(errors, v => v.MemberNames.Contains(nameof(GlobalSettings.SiteName)));
        }

        [Fact]
        public void GlobalSettings_ShouldBeInvalid_WhenEmailIsMalformed()
        {
            // Arrange
            var settings = new GlobalSettings
            {
                SiteName = "Test Site",
                SupportEmail = "not-an-email" // Invalid Email Format
            };

            // Act
            var errors = ValidateModel(settings);

            // Assert
            Assert.Contains(errors, v => v.MemberNames.Contains(nameof(GlobalSettings.SupportEmail)));
        }
    }
}