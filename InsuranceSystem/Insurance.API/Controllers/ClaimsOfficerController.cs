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

    private Guid CurrentOfficerUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>
    /// Returns claims visible to this officer:
    ///  - All Submitted and UnderReview claims (available to process)
    ///  - Any claims explicitly assigned to this officer
    /// </summary>
    [HttpGet("claims")]
    public async Task<IActionResult> GetClaims()
    {
        var claims = await _claimService.GetClaimsByOfficerAsync(CurrentOfficerUserId);
        return Ok(claims);
    }

    /// <summary>
    /// Dashboard summary for this officer's workload.
    /// </summary>
    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        var summary = await _claimService.GetClaimsOfficerDashboardSummaryAsync(CurrentOfficerUserId);
        return Ok(summary);
    }

    /// <summary>
    /// Atomically locks the claim to this officer and moves it to UnderReview.
    /// Handles both Submitted and auto-fraud-flagged (already UnderReview) claims.
    /// Prevents race conditions — if another officer has already locked it, returns 409.
    /// </summary>
    [HttpPost("start-review")]
    public async Task<IActionResult> StartReview([FromQuery] Guid claimId)
    {
        try
        {
            await _claimService.StartReviewAsync(claimId, CurrentOfficerUserId);
            return Ok(new { Message = "Claim locked to you and is now under review." });
        }
        catch (Exception ex) when (ex.Message.Contains("another officer"))
        {
            return Conflict(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Approve or reject a claim. Must be UnderReview.
    /// Only the assigned officer can make this decision.
    /// Rejection requires a non-empty remarks/reason (regulatory requirement).
    /// </summary>
    [HttpPost("review")]
    public async Task<IActionResult> ReviewClaim(
        [FromQuery] Guid claimId,
        [FromQuery] bool approve,
        [FromQuery] string? remarks = null)
    {
        try
        {
            await _claimService.ReviewClaimAsync(claimId, CurrentOfficerUserId, approve, remarks);
            return Ok(new
            {
                Message = approve
                    ? "Claim approved successfully."
                    : "Claim rejected successfully."
            });
        }
        catch (Exception ex) when (ex.Message.Contains("not the assigned officer"))
        {
            return Forbid();
        }
        catch (Exception ex) when (ex.Message.Contains("rejection reason"))
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Settle an approved claim. Transitions: Approved → Settled.
    /// </summary>
    [HttpPost("settle")]
    public async Task<IActionResult> Settle([FromQuery] Guid claimId)
    {
        await _claimService.SettleClaimAsync(claimId);
        return Ok(new { Message = "Claim settled successfully." });
    }
}