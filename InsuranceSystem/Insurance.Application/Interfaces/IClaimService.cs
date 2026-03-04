using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insurance.Application.DTOs.Claim;

namespace Insurance.Application.Interfaces
{
    public interface IClaimService
    {
        Task SubmitClaimAsync(SubmitClaimDto dto);
        Task StartReviewAsync(Guid claimId);
        Task ReviewClaimAsync(Guid claimId, bool approve);
        Task SettleClaimAsync(Guid claimId);
        Task<List<ClaimDto>> GetClaimsByCustomerAsync(Guid customerUserId);
        Task<List<ClaimDto>> GetAllClaimsAsync();
    }
}
