using Insurance.Application.DTOs.Payment;
using Insurance.Application.DTOs.Claim;
using Insurance.Application.DTOs.PolicyApplication;
using Insurance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Insurance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer")]
public class CustomerController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IPolicyProductService _policyProductService;
    private readonly IPolicyApplicationService _policyApplicationService;

    public CustomerController(
        IPaymentService paymentService,
        IPolicyProductService policyProductService,
        IPolicyApplicationService policyApplicationService)
    {
        _paymentService = paymentService;
        _policyProductService = policyProductService;
        _policyApplicationService = policyApplicationService;
    }

    // Pay for policy (existing)
    [HttpPost("pay")]
    public async Task<IActionResult> MakePayment([FromBody] MakePaymentDto dto)
    {
        await _paymentService.ProcessPaymentAsync(dto);
        return Ok(new { message = "Payment successful. Policy activated." });
    }

    // Submit claims (existing)
    [Authorize(Roles = "Customer")]
    [HttpPost("submit-claim")]
    public async Task<IActionResult> SubmitClaim(
    [FromForm] SubmitClaimDto dto,
    [FromForm] List<IFormFile>? files,
    [FromServices] IClaimService claimService,
    [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
    {
        var uploadPath = Path.Combine(env.ContentRootPath, "uploads");
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var savedFilePaths = new List<string>();

        if (files != null && files.Count > 0)
        {
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    savedFilePaths.Add($"/uploads/{fileName}");
                }
            }
        }

        dto.Documents = savedFilePaths;

        await claimService.SubmitClaimAsync(dto);
        return Ok(new { message = "Claim submitted successfully." });
    }

    // ──────────────────────────────────────────────────────────────────────────
    // NEW ENDPOINTS: PolicyProduct browsing & PolicyApplication submission
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Browse all active insurance products available for application.
    /// </summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _policyProductService.GetAllActiveProductsAsync();
        return Ok(products);
    }

    /// <summary>
    /// Apply for a PolicyProduct. Status is set to Submitted.
    /// CustomerId is extracted from the JWT token.
    /// </summary>
    [HttpPost("apply-product")]
    public async Task<IActionResult> ApplyForProduct([FromBody] ApplyForProductDto dto)
    {
        var customerUserId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var applicationId = await _policyApplicationService.ApplyForProductAsync(customerUserId, dto);
        return Ok(new { message = "Application submitted successfully.", applicationId = applicationId });
    }

    /// <summary>
    /// Pay the premium for a Draft policy, activating it.
    /// </summary>
    [HttpPost("pay-premium/{policyId}")]
    public async Task<IActionResult> PayPremium(Guid policyId)
    {
        var dto = new MakePaymentDto { PolicyId = policyId, Amount = 0 };
        await _paymentService.ProcessPaymentAsync(dto);
        return Ok(new { message = "Premium paid. Policy is now Active." });
    }

    [HttpGet("policies")]
    public async Task<IActionResult> GetMyPolicies([FromServices] IPolicyService policyService)
    {
        var customerUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var policies = await policyService.GetPoliciesByCustomerAsync(customerUserId);
        return Ok(policies);
    }

    [HttpGet("claims")]
    public async Task<IActionResult> GetMyClaims([FromServices] IClaimService claimService)
    {
        var customerUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var claims = await claimService.GetClaimsByCustomerAsync(customerUserId);
        return Ok(claims);
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetMyApplications()
    {
        var customerUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var applications = await _policyApplicationService.GetApplicationsByCustomerAsync(customerUserId);
        return Ok(applications);
    }

    [HttpPost("renew-policy/{policyId}")]
    public async Task<IActionResult> RenewPolicy(Guid policyId, [FromServices] IPolicyService policyService)
    {
        var customerUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await policyService.RenewPolicyAsync(policyId, customerUserId);
        return Ok(result);
    }

    [HttpGet("download-policy/{policyId}")]
    public async Task<IActionResult> DownloadPolicyDocument(Guid policyId, [FromQuery] string type, [FromServices] IPolicyDocumentService docService)
    {
        var content = await docService.GeneratePolicyScheduleAsync(policyId, type ?? "Schedule");
        return File(content, "application/pdf", $"{type ?? "Policy"}_{policyId.ToString().Substring(0, 8)}.pdf");
    }
}