using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface IPolicyRepository
    {
        Task AddPolicyAsync(Policy policy);
        Task<Policy?> GetByIdAsync(Guid id);
        Task<List<Policy>> GetPoliciesByAgentAsync(Guid agentUserId);
        Task<List<Policy>> GetPoliciesByCustomerAsync(Guid customerUserId);
        Task<List<Policy>> GetAllAsync();
        Task SaveChangesAsync();
    }
}
