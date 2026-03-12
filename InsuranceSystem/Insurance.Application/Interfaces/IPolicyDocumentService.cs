using System;
using System.Threading.Tasks;

namespace Insurance.Application.Interfaces
{
    public interface IPolicyDocumentService
    {
        Task<byte[]> GeneratePolicyScheduleAsync(Guid policyId, string docType = "Schedule");
    }
}
