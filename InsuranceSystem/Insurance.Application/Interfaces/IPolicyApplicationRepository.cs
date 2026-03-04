using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface IPolicyApplicationRepository
    {
        Task AddAsync(PolicyApplication application);
        Task<PolicyApplication?> GetByIdAsync(Guid id);
        Task<List<PolicyApplication>> GetByAgentIdAsync(Guid agentUserId);
        Task<List<PolicyApplication>> GetByCustomerIdAsync(Guid customerUserId);
        Task<List<PolicyApplication>> GetAllAsync();
        Task SaveChangesAsync();
    }
}
