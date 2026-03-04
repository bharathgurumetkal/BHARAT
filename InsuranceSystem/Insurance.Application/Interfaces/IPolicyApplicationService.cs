using Insurance.Application.DTOs.PolicyApplication;

namespace Insurance.Application.Interfaces
{
    public interface IPolicyApplicationService
    {
        Task<Guid> ApplyForProductAsync(Guid customerUserId, ApplyForProductDto dto);
        Task AssignAgentAsync(Guid applicationId, Guid agentUserId);
        Task ApproveApplicationAsync(Guid applicationId, Guid agentUserId);
        Task RejectApplicationAsync(Guid applicationId, Guid agentUserId);
        Task<List<PolicyApplicationDto>> GetAssignedApplicationsAsync(Guid agentUserId);
        Task<List<PolicyApplicationDto>> GetApplicationsByCustomerAsync(Guid customerUserId);
        Task<List<PolicyApplicationDto>> GetAllApplicationsAsync();
    }
}
