using Insurance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("claims")]
    public async Task<IActionResult> GetClaims()
    {
        var claims = await _claimService.GetAllClaimsAsync();
        return Ok(claims);
    }

    [HttpGet("policies")]
    public async Task<IActionResult> GetPolicies([FromServices] IPolicyService policyService)
    {
        var policies = await policyService.GetAllPoliciesAsync();
        return Ok(policies);
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