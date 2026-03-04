using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface IPolicyProductRepository
    {
        Task AddAsync(PolicyProduct product);
        Task<PolicyProduct?> GetByIdAsync(Guid id);
        Task<List<PolicyProduct>> GetAllActiveAsync();
        Task SaveChangesAsync();
    }
}
