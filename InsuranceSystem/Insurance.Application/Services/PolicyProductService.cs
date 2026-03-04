using Insurance.Application.DTOs.PolicyProduct;
using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;

namespace Insurance.Application.Services
{
    public class PolicyProductService : IPolicyProductService
    {
        private readonly IPolicyProductRepository _productRepository;

        public PolicyProductService(IPolicyProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Guid> CreateProductAsync(CreatePolicyProductDto dto)
        {
            var product = new PolicyProduct
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                PropertyCategory = dto.PropertyCategory,
                BaseRatePercentage = dto.BaseRatePercentage,
                MaxCoverageAmount = dto.MaxCoverageAmount,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();
            return product.Id;
        }

        public async Task<List<PolicyProductDto>> GetAllActiveProductsAsync()
        {
            var products = await _productRepository.GetAllActiveAsync();
            return products.Select(p => new PolicyProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                PropertyCategory = p.PropertyCategory,
                BaseRatePercentage = p.BaseRatePercentage,
                MaxCoverageAmount = p.MaxCoverageAmount,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            }).ToList();
        }
    }
}
