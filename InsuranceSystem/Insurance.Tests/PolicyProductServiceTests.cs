using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Insurance.Application.DTOs.PolicyProduct;
using Insurance.Application.Interfaces;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Moq;
using Xunit;

namespace Insurance.Tests
{
    public class PolicyProductServiceTests
    {
        private Mock<IPolicyProductRepository> _productRepositoryMock;
        private PolicyProductService _productService;

        public PolicyProductServiceTests()
        {
            _productRepositoryMock = new Mock<IPolicyProductRepository>();
            _productService = new PolicyProductService(_productRepositoryMock.Object);
        }

        // ─── CreateProductAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task CreateProductAsync_ValidDto_SavesProductAndReturnsId()
        {
            var dto = new CreatePolicyProductDto
            {
                Name = "Flood Insurance",
                Description = "Covers flood damage",
                PropertyCategory = "Residential",
                BaseRatePercentage = 1.5m,
                MaxCoverageAmount = 300_000
            };

            Guid capturedId = Guid.Empty;
            _productRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<PolicyProduct>()))
                .Callback<PolicyProduct>(p => capturedId = p.Id);

            var result = await _productService.CreateProductAsync(dto);

            Assert.NotEqual(Guid.Empty, result);
            _productRepositoryMock.Verify(r => r.AddAsync(It.Is<PolicyProduct>(p =>
                p.Name == dto.Name &&
                p.BaseRatePercentage == dto.BaseRatePercentage &&
                p.MaxCoverageAmount == dto.MaxCoverageAmount &&
                p.IsActive == true
            )), Times.Once);
            _productRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_NewProduct_IsActiveByDefault()
        {
            var dto = new CreatePolicyProductDto { Name = "Test", Description = "X", PropertyCategory = "Any" };

            await _productService.CreateProductAsync(dto);

            _productRepositoryMock.Verify(r => r.AddAsync(It.Is<PolicyProduct>(p => p.IsActive == true)), Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_SetsCreatedAtTimestamp()
        {
            var beforeCall = DateTime.UtcNow;
            var dto = new CreatePolicyProductDto { Name = "T", Description = "D", PropertyCategory = "C" };

            await _productService.CreateProductAsync(dto);

            _productRepositoryMock.Verify(r => r.AddAsync(It.Is<PolicyProduct>(p => p.CreatedAt >= beforeCall)), Times.Once);
        }

        // ─── GetAllActiveProductsAsync ────────────────────────────────────────────

        [Fact]
        public async Task GetAllActiveProductsAsync_ReturnsMappedDtos()
        {
            var products = new List<PolicyProduct>
            {
                new PolicyProduct { Id = Guid.NewGuid(), Name = "Product A", PropertyCategory = "ResA", BaseRatePercentage = 2.0m, MaxCoverageAmount = 200_000, IsActive = true  },
                new PolicyProduct { Id = Guid.NewGuid(), Name = "Product B", PropertyCategory = "ResB", BaseRatePercentage = 1.5m, MaxCoverageAmount = 150_000, IsActive = true  }
            };

            _productRepositoryMock.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(products);

            var result = await _productService.GetAllActiveProductsAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("Product A", result[0].Name);
            Assert.Equal(2.0m, result[0].BaseRatePercentage);
            Assert.Equal("Product B", result[1].Name);
            Assert.True(result[0].IsActive);
        }

        [Fact]
        public async Task GetAllActiveProductsAsync_EmptyRepository_ReturnsEmptyList()
        {
            _productRepositoryMock.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(new List<PolicyProduct>());

            var result = await _productService.GetAllActiveProductsAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllActiveProductsAsync_MapsAllDtoFields()
        {
            var product = new PolicyProduct
            {
                Id = Guid.NewGuid(),
                Name = "P1", Description = "Desc1", PropertyCategory = "Cat1",
                BaseRatePercentage = 3.5m, MaxCoverageAmount = 500_000,
                IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            _productRepositoryMock.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(new List<PolicyProduct> { product });

            var result = await _productService.GetAllActiveProductsAsync();
            var dto = result[0];

            Assert.Equal(product.Id, dto.Id);
            Assert.Equal("Desc1", dto.Description);
            Assert.Equal("Cat1", dto.PropertyCategory);
            Assert.Equal(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), dto.CreatedAt);
        }
    }
}
