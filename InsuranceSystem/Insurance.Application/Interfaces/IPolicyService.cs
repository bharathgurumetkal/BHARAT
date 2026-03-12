using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insurance.Application.DTOs.Policy;

namespace Insurance.Application.Interfaces
{
    public interface IPolicyService
    {
        Task<Guid> CreatePolicyAsync(CreatePolicyDto dto, Guid agentId);
        Task<List<PolicyDto>> GetPoliciesByCustomerAsync(Guid customerUserId);
        Task<List<PolicyDto>> GetAllPoliciesAsync();
        Task<RenewPolicyResponseDto> RenewPolicyAsync(Guid policyId, Guid customerUserId);
        Task<PolicyDto?> GetPolicyByIdAsync(Guid policyId);
    }
}
