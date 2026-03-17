using Insurance.Application.DTOs.Policy;
using Insurance.Application.DTOs.Claim;
using Insurance.Application.Interfaces;
using Insurance.Domain.Enums;
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
    private readonly IAiClaimClient _aiClaimClient;
    private readonly IClaimRepository _claimRepository;

    public AgentController(
        ICustomerRepository customerRepository,
        IPolicyRepository policyRepository,
        IPolicyService policyService,
        IPolicyApplicationService policyApplicationService,
        ICommissionRepository commissionRepository,
        IAiClaimClient aiClaimClient,
        IClaimRepository claimRepository)
    {
        _customerRepository = customerRepository;
        _policyRepository = policyRepository;
        _policyService = policyService;
        _policyApplicationService = policyApplicationService;
        _commissionRepository = commissionRepository;
        _aiClaimClient = aiClaimClient;
        _claimRepository = claimRepository;
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
        var policies = await _policyRepository.GetPoliciesByCustomerAsync(customerId);
        return Ok(policies);
    }

    [HttpGet("my-customers")]
    public async Task<IActionResult> GetAssignedCustomers()
    {
        var agentId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var customers = await _customerRepository.GetByAgentIdAsync(agentId);
        return Ok(customers);
    }

    [HttpGet("policies")]
    public async Task<IActionResult> GetMyPolicies(
        string? status = null,
        int page = 1,
        int pageSize = 1000)
    {
        var agentId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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

    [HttpGet("assigned-applications")]
    public async Task<IActionResult> GetAssignedApplications()
    {
        var agentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var applications = await _policyApplicationService.GetAssignedApplicationsAsync(agentUserId);
        return Ok(applications);
    }

    [HttpPost("approve-application/{applicationId}")]
    public async Task<IActionResult> ApproveApplication(Guid applicationId)
    {
        var agentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _policyApplicationService.ApproveApplicationAsync(applicationId, agentUserId);
        return Ok(new { Message = "Application approved. Draft policy has been created." });
    }

    [HttpPost("reject-application/{applicationId}")]
    public async Task<IActionResult> RejectApplication(Guid applicationId)
    {
        var agentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _policyApplicationService.RejectApplicationAsync(applicationId, agentUserId);
        return Ok(new { Message = "Application rejected." });
    }

    [HttpGet("commissions")]
    public async Task<IActionResult> GetMyCommissions()
    {
        var agentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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

    [HttpGet("smart-prospecting")]
    public async Task<IActionResult> GetSmartProspecting([FromQuery] bool forceRefresh = false)
    {
        var agentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // 1. Get assigned customers
        var customers = await _customerRepository.GetByAgentIdAsync(agentUserId);
        
        // Optimize: Fetch all relevant data in a few queries instead of many
        var customerUserIds = customers.Select(c => c.UserId).ToList();
        var customerIds = customers.Select(c => c.Id).ToList();
        
        // Note: For large systems, we'd use a repository method that takes List<Guid>
        // For now, filtering the list of all items is faster than N database roundtrips
        var allPolicies = await _policyRepository.GetPoliciesByAgentAsync(agentUserId);
        var allClaims = await _claimRepository.GetAllAsync(); // Ideally filter by customerUserIds
        
        var resultList = new List<SmartProspectDto>();
        var analysisTasks = new List<Task<(Guid CustomerId, AiProspectOutputDto? Analysis)>>();

        foreach (var customer in customers)
        {
            var customerPolicies = allPolicies.Where(p => p.CustomerId == customer.Id || p.Customer.UserId == customer.UserId).ToList();
            var customerClaims = allClaims.Where(c => c.Policy?.CustomerId == customer.Id || c.Policy?.Customer?.UserId == customer.UserId).ToList();

            int tenureDays = 0;
            if (customerPolicies.Any())
            {
                var earliest = customerPolicies.Min(p => p.StartDate ?? DateTime.UtcNow);
                tenureDays = (DateTime.UtcNow - earliest).Days;
            }

            var prospectData = new SmartProspectDto
            {
                CustomerId = customer.Id,
                CustomerName = customer.User?.Name ?? "Unknown",
                Email = customer.User?.Email ?? "-",
                PolicyCount = customerPolicies.Count,
                TotalPremiumPaid = customerPolicies.Sum(p => p.Premium), 
                ClaimCount = customerClaims.Count,
                CustomerTenureDays = tenureDays,
                LastAnalyzedAt = customer.AiLastAnalyzedAt
            };

            // SMART CACHE CHECK
            bool needsAnalysis = forceRefresh || !customer.AiLastAnalyzedAt.HasValue || customer.AiLastAnalyzedAt < DateTime.UtcNow.AddDays(-1);

            if (!needsAnalysis && customer.AiRenewalScore.HasValue)
            {
                prospectData.Source = "Database Cache";
                prospectData.AiAnalysis = new AiProspectOutputDto
                {
                    RenewalScore = customer.AiRenewalScore.Value,
                    Likelihood = customer.AiLikelihood ?? "Unknown",
                    ChurnProbability = customer.AiChurnProbability ?? 0,
                    Explanation = customer.AiExplanation ?? "Retrieved from memory.",
                    RecommendedAction = customer.AiRecommendedAction ?? "Follow standard protocol.",
                    IsFallback = false
                };
            }
            else
            {
                prospectData.Source = "AI Service (Analyzing...)";
                var aiInput = new AiProspectInputDto
                {
                    PolicyCount = prospectData.PolicyCount,
                    TotalPremiumPaid = prospectData.TotalPremiumPaid,
                    ClaimCount = prospectData.ClaimCount,
                    CustomerTenureDays = prospectData.CustomerTenureDays
                };

                analysisTasks.Add(Task.Run(async () => {
                    try {
                        var res = await _aiClaimClient.PredictProspectAsync(aiInput);
                        return (customer.Id, res);
                    } catch {
                        return (customer.Id, (AiProspectOutputDto?)null);
                    }
                }));
            }

            resultList.Add(prospectData);
        }

        // 2. Parallel AI execution
        if (analysisTasks.Any())
        {
            var analyses = await Task.WhenAll(analysisTasks);
            
            foreach (var (cid, analysis) in analyses)
            {
                var prospect = resultList.First(r => r.CustomerId == cid);
                var customerObj = customers.First(c => c.Id == cid);

                if (analysis != null)
                {
                    prospect.AiAnalysis = analysis;
                    prospect.Source = "AI Service (Fresh)";
                    prospect.LastAnalyzedAt = DateTime.UtcNow;

                    // Update Cache
                    customerObj.AiRenewalScore = analysis.RenewalScore;
                    customerObj.AiLikelihood = analysis.Likelihood;
                    customerObj.AiChurnProbability = analysis.ChurnProbability;
                    customerObj.AiExplanation = analysis.Explanation;
                    customerObj.AiRecommendedAction = analysis.RecommendedAction;
                    customerObj.AiLastAnalyzedAt = DateTime.UtcNow;
                }
                else if (customerObj.AiRenewalScore.HasValue)
                {
                    // If AI fails, fallback to old cache if available
                    prospect.Source = "Cache (Fallback)";
                    prospect.AiAnalysis = new AiProspectOutputDto {
                        RenewalScore = customerObj.AiRenewalScore.Value,
                        Likelihood = customerObj.AiLikelihood ?? "Unknown",
                        Explanation = "AI Service failed, showing last cached data.",
                        RecommendedAction = customerObj.AiRecommendedAction ?? "Check back later."
                    };
                }
            }
            
            await _customerRepository.SaveChangesAsync();
        }

        return Ok(resultList);
    }
}