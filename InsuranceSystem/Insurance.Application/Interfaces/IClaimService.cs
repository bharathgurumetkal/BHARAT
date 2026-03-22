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

        /// <summary>Atomically locks the claim to this officer and moves it to UnderReview.</summary>
        Task StartReviewAsync(Guid claimId, Guid officerUserId);

        /// <summary>Officer approves or rejects. Only the officer who started review can do this.</summary>
        Task ReviewClaimAsync(Guid claimId, Guid officerUserId, bool approve, string? remarks = null);

        Task SettleClaimAsync(Guid claimId);
        Task<List<ClaimDto>> GetClaimsByCustomerAsync(Guid customerUserId);
        Task<List<ClaimDto>> GetAllClaimsAsync();
        Task<List<ClaimDto>> GetClaimsByOfficerAsync(Guid officerUserId);
        Task AssignOfficerAsync(Guid claimId, Guid officerUserId);
        Task<ClaimsOfficerDashboardSummaryDto> GetClaimsOfficerDashboardSummaryAsync(Guid officerUserId);
    }
}
