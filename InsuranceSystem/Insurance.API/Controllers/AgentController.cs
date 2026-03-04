using Insurance.Application.DTOs.Policy;
using Insurance.Application.Interfaces;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Insurance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Agent")]
public class AgentController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IPolicyService _policyService;
    private readonly IPolicyApplicationService _policyApplicationService;
    private readonly ICommissionRepository _commissionRepository;

    public AgentController(
        ICustomerRepository customerRepository,
        IPolicyRepository policyRepository,
        IPolicyService policyService,
        IPolicyApplicationService policyApplicationService,
        ICommissionRepository commissionRepository)
    {
        _customerRepository = customerRepository;
        _policyRepository = policyRepository;
        _policyService = policyService;
        _policyApplicationService = policyApplicationService;
        _commissionRepository = commissionRepository;
    }

    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        var agentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var customers = await _customerRepository.GetByAgentIdAsync(agentUserId);
        var apps = await _policyApplicationService.GetAssignedApplicationsAsync(agentUserId);
        var allPolicies = await _policyRepository.GetPoliciesByAgentAsync(agentUserId);

        return Ok(new
        {
            Customers = customers.Count,
            PendingApps = apps.Count,
            ActivePolicies = allPolicies.Count(p => p.Status == PolicyStatus.Active),
            DraftPolicies = allPolicies.Count(p => p.Status == PolicyStatus.Draft)
        });
    }

    [HttpGet("customer/{customerId}/policies")]
    public async Task<IActionResult> GetCustomerPolicies(Guid customerId)
    {
        // For simplicity, we directly fetch via customerId.
        // In a real prod app, you'd verify if the customer belongs to this agent.
        var policies = await _policyRepository.GetPoliciesByCustomerAsync(customerId);
        return Ok(policies);
    }

    // Existing: get customers assigned to this agent (via Customer entity)
    [HttpGet("my-customers")]
    public async Task<IActionResult> GetAssignedCustomers()
    {
        var agentId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var customers = await _customerRepository.GetByAgentIdAsync(agentId);

        return Ok(customers);
    }

    [HttpGet("policies")]
    public async Task<IActionResult> GetMyPolicies(
        string? status = null,
        int page = 1,
        int pageSize = 1000)
    {
        var agentId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var policies = await _policyRepository.GetPoliciesByAgentAsync(agentId);

        if (!string.IsNullOrEmpty(status))
            policies = policies
                .Where(p => p.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var paged = policies
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return Ok(paged);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // NEW ENDPOINTS: PolicyApplication review
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Get all PolicyApplications assigned to the currently logged-in agent.
    /// Status must be AssignedToAgent.
    /// </summary>
    [HttpGet("assigned-applications")]
    public async Task<IActionResult> GetAssignedApplications()
    {
        var agentUserId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var applications = await _policyApplicationService.GetAssignedApplicationsAsync(agentUserId);
        return Ok(applications);
    }

    /// <summary>
    /// Approve a PolicyApplication.
    /// Transitions: AssignedToAgent → ApprovedByAgent, and creates a Draft Policy.
    /// </summary>
    [HttpPost("approve-application/{applicationId}")]
    public async Task<IActionResult> ApproveApplication(Guid applicationId)
    {
        var agentUserId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _policyApplicationService.ApproveApplicationAsync(applicationId, agentUserId);
        return Ok(new { Message = "Application approved. Draft policy has been created." });
    }

    /// <summary>
    /// Reject a PolicyApplication.
    /// Transitions: AssignedToAgent → RejectedByAgent.
    /// </summary>
    [HttpPost("reject-application/{applicationId}")]
    public async Task<IActionResult> RejectApplication(Guid applicationId)
    {
        var agentUserId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _policyApplicationService.RejectApplicationAsync(applicationId, agentUserId);
        return Ok(new { Message = "Application rejected." });
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Commissions
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all commissions earned by the currently logged-in agent,
    /// with associated policy and customer details. Sorted newest first.
    /// </summary>
    [HttpGet("commissions")]
    public async Task<IActionResult> GetMyCommissions()
    {
        var agentUserId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var commissions = await _commissionRepository.GetByAgentIdAsync(agentUserId);

        var result = commissions.Select(c => new
        {
            Id             = c.Id,
            PolicyId       = c.PolicyId,
            PolicyNumber   = c.Policy?.PolicyNumber ?? "-",
            CustomerName   = c.Policy?.Customer?.User?.Name ?? "-",
            Premium        = c.Policy?.Premium ?? 0,
            CommissionRate = c.CommissionRate,
            CommissionAmount = c.CommissionAmount,
            CreatedAt      = c.CreatedAt,
            IsPaid         = c.IsPaid
        });

        return Ok(result);
    }
}