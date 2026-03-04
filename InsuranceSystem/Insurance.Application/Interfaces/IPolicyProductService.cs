using Insurance.Application.DTOs.PolicyProduct;

namespace Insurance.Application.Interfaces
{
    public interface IPolicyProductService
    {
        Task<Guid> CreateProductAsync(CreatePolicyProductDto dto);
        Task<List<PolicyProductDto>> GetAllActiveProductsAsync();
    }
}
