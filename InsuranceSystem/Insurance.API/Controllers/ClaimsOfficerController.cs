using Insurance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Insurance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ClaimsOfficer")]
public class ClaimsOfficerController : ControllerBase
{
    private readonly IClaimService _claimService;

    public ClaimsOfficerController(IClaimService claimService)
    {
        _claimService = claimService;
    }

    /// <summary>
    /// Returns only the claims assigned to the currently logged-in officer.
    /// </summary>
    [HttpGet("claims")]
    public async Task<IActionResult> GetClaims()
    {
        var officerUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var claims = await _claimService.GetClaimsByOfficerAsync(officerUserId);
        return Ok(claims);
    }

    /// <summary>
    /// Dashboard summary — counts only this officer's assigned claims.
    /// </summary>
    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        var officerUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var summary = await _claimService.GetClaimsOfficerDashboardSummaryAsync(officerUserId);
        return Ok(summary);
    }

    /// <summary>
    /// Move a claim from Submitted → UnderReview.
    /// Must be called before review (approve/reject).
    /// </summary>
    [HttpPost("start-review")]
    public async Task<IActionResult> StartReview([FromQuery] Guid claimId)
    {
        await _claimService.StartReviewAsync(claimId);
        return Ok(new { Message = "Claim is now under review." });
    }

    /// <summary>
    /// Review claim (approve or reject). Claim must be UnderReview.
    /// Transitions: UnderReview → Approved | Rejected
    /// </summary>
    [HttpPost("review")]
    public async Task<IActionResult> ReviewClaim(
        [FromQuery] Guid claimId,
        [FromQuery] bool approve)
    {
        await _claimService.ReviewClaimAsync(claimId, approve);

        return Ok(new
        {
            Message = approve
                ? "Claim approved successfully."
                : "Claim rejected successfully."
        });
    }

    /// <summary>
    /// Settle an approved claim. Only Approved claims can be settled.
    /// Transitions: Approved → Settled
    /// </summary>
    [HttpPost("settle")]
    public async Task<IActionResult> Settle([FromQuery] Guid claimId)
    {
        await _claimService.SettleClaimAsync(claimId);
        return Ok(new { Message = "Claim settled successfully." });
    }
}