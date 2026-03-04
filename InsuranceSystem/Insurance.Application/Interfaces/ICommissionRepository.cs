using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface ICommissionRepository
    {
        Task AddAsync(Commission commission);
        Task<bool> ExistsForPolicyAsync(Guid policyId);
        Task<List<Commission>> GetByAgentIdAsync(Guid agentUserId);
        Task SaveChangesAsync();
    }
}
