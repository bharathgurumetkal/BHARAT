using Insurance.Application.DTOs.Policy;
using Insurance.Application.DTOs.Auth;
using Insurance.Application.DTOs.PolicyProduct;
using Insurance.Application.DTOs.PolicyApplication;
using Insurance.Application.DTOs.Claim;
using System.Security.Claims;
using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Insurance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAgentRepository _agentRepository;
    private readonly IClaimsOfficerRepository _claimsOfficerRepository;
    private readonly IPolicyProductService _policyProductService;
    private readonly IPolicyApplicationService _policyApplicationService;
    private readonly IPolicyService _policyService;
    private readonly IClaimService _claimService;

    public AdminController(
        IAdminService adminService,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAgentRepository agentRepository,
        IClaimsOfficerRepository claimsOfficerRepository,
        IPolicyProductService policyProductService,
        IPolicyApplicationService policyApplicationService,
        IPolicyService policyService,
        IClaimService claimService)
    {
        _adminService = adminService;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _agentRepository = agentRepository;
        _claimsOfficerRepository = claimsOfficerRepository;
        _policyProductService = policyProductService;
        _policyApplicationService = policyApplicationService;
        _policyService = policyService;
        _claimService = claimService;
    }

    [HttpGet("agents")]
    public async Task<IActionResult> GetAllAgents()
    {
        var agents = await _agentRepository.GetAllAsync();
        return Ok(agents);
    }

    [HttpGet("claimsofficers")]
    public async Task<IActionResult> GetAllClaimsOfficers()
    {
        var officers = await _claimsOfficerRepository.GetAllAsync();
        var result = officers.Select(o => new {
            Id = o.Id,
            UserId = o.UserId,
            Department = o.Department,
            User = new {
                Name = o.User?.Name ?? "—",
                Email = o.User?.Email ?? "—"
            }
        });
        return Ok(result);
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _policyProductService.GetAllActiveProductsAsync();
        return Ok(products);
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetAllApplications()
    {
        // We'll need a method in IPolicyApplicationService to get all applications for admin
        var applications = await _policyApplicationService.GetAllApplicationsAsync();
        return Ok(applications);
    }

    [HttpGet("policies")]
    public async Task<IActionResult> GetAllPolicies()
    {
        var policies = await _policyService.GetAllPoliciesAsync();
        return Ok(policies);
    }


    [HttpGet("customers")]
    public async Task<IActionResult> GetAllCustomers()
    {
        var customers = await _adminService.GetAllCustomersAsync();
        var result = customers.Select(c => new
        {
            id = c.Id,
            userId = c.UserId,
            name = c.User?.Name ?? "—",
            email = c.User?.Email ?? "—",
            phoneNumber = c.User?.PhoneNumber ?? "—",
            assignedAgentId = c.AssignedAgentId,
            status = c.Status
        });
        return Ok(result);
    }

    [HttpPost("assign-customer")]
    public async Task<IActionResult> AssignCustomer(Guid customerId, Guid agentId)
    {
        await _adminService.AssignCustomerAsync(customerId, agentId);
        return Ok(new { Message = "Customer assigned successfully." });
    }

    [HttpPost("add-agent")]
    public async Task<IActionResult> AddAgent([FromBody] RegisterRequestDto request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
            return BadRequest("User already exists.");

        var user = new Insurance.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            PhoneNumber = request.PhoneNumber,
            Role = Insurance.Domain.Enums.RoleType.Agent
        };

        await _userRepository.AddAsync(user);

        var agent = new Insurance.Domain.Entities.Agent
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            LicenseNumber = "LIC-" + DateTime.UtcNow.Ticks,
            JoinedDate = DateTime.UtcNow
        };

        await _agentRepository.AddAsync(agent);

        await _userRepository.SaveChangesAsync();

        return Ok(new { Message = "Agent created successfully.", UserId = user.Id });
    }

    [HttpPost("add-claimsofficer")]
    public async Task<IActionResult> AddClaimsOfficer([FromBody] RegisterRequestDto request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
            return BadRequest("User already exists.");

        var user = new Insurance.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            PhoneNumber = request.PhoneNumber,
            Role = Insurance.Domain.Enums.RoleType.ClaimsOfficer
        };

        await _userRepository.AddAsync(user);

        var officer = new Insurance.Domain.Entities.ClaimsOfficer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Department = request.Name + " Dept"
        };

        await _claimsOfficerRepository.AddAsync(officer);

        await _userRepository.SaveChangesAsync();

        return Ok(new { Message = "Claims Officer created successfully.", UserId = user.Id });
    }

    // All claims with officer assignment info — for Admin claims management page
    [HttpGet("claims")]
    public async Task<IActionResult> GetAllClaims()
    {
        var claims = await _claimService.GetAllClaimsAsync();
        return Ok(claims);
    }

    // Claims by status
    [Authorize(Roles = "Admin")]
    [HttpGet("claims-report")]

    public async Task<IActionResult> ClaimsReport(
    [FromServices] AppDbContext context)
    {
        var report = await context.Claims
            .GroupBy(c => c.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(x => x.ClaimAmount)
            }).ToListAsync();

        return Ok(report);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("revenue-report")]
    public async Task<IActionResult> RevenueReport(
    [FromServices] AppDbContext context)
    {
        var report = await context.Payments
            .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                TotalRevenue = g.Sum(x => x.Amount)
            })
            .ToListAsync();

        return Ok(report);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("agent-performance")]
    public async Task<IActionResult> AgentPerformance(
    [FromServices] AppDbContext context)
    {
        var report = await context.Policies
            .GroupBy(p => p.Customer.AssignedAgentId)
            .Select(g => new
            {
                AgentId = g.Key,
                PolicyCount = g.Count(),
                TotalPremium = g.Sum(p => p.Premium)
            })
            .ToListAsync();

        return Ok(report);
    }

    // Create policy (legacy admin flow)
    [HttpPost("create-policy")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePolicy(
    [FromBody] CreatePolicyDto dto)
    {
        var adminId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var policyId = await _policyService.CreatePolicyAsync(dto, adminId);

        return Ok(new { PolicyId = policyId });
    }

    // ──────────────────────────────────────────────────────────────────────────
    // NEW ENDPOINTS: PolicyProduct & PolicyApplication
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Create a reusable PolicyProduct template that customers can apply for.
    /// </summary>
    [HttpPost("create-product")]
    public async Task<IActionResult> CreateProduct([FromBody] CreatePolicyProductDto dto)
    {
        var productId = await _policyProductService.CreateProductAsync(dto);
        return Ok(new { Message = "Policy product created successfully.", ProductId = productId });
    }

    /// <summary>
    /// Assign an agent to a pending PolicyApplication; transitions status Submitted → AssignedToAgent.
    /// </summary>
    [HttpPost("assign-agent-to-application/{applicationId}")]
    public async Task<IActionResult> AssignAgentToApplication(
        Guid applicationId,
        [FromBody] AssignAgentToApplicationDto dto)
    {
        await _policyApplicationService.AssignAgentAsync(applicationId, dto.AgentId);
        return Ok(new { Message = "Agent assigned to application successfully." });
    }

    /// <summary>
    /// Assign a ClaimsOfficer to a specific Claim.
    /// The officer will be notified and will see this claim in their queue.
    /// </summary>
    [HttpPost("assign-officer-to-claim/{claimId}")]
    public async Task<IActionResult> AssignOfficerToClaim(
        Guid claimId,
        [FromBody] AssignOfficerToClaimDto dto)
    {
        await _claimService.AssignOfficerAsync(claimId, dto.OfficerUserId);
        return Ok(new { Message = "Claims Officer assigned to claim successfully." });
    }

    // ──────────────────────────────────────────────────────────────────────────
    // FEATURE 2 — AI Intelligence Analytics
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// System-wide AI risk analytics for the Admin dashboard.
    /// </summary>
    [HttpGet("ai-analytics")]
    public async Task<IActionResult> GetAiAnalytics([FromServices] AppDbContext context)
    {
        var claims = await context.Claims
            .Where(c => c.AiRiskScore.HasValue)
            .ToListAsync();

        var allClaims = await context.Claims.CountAsync();

        var highRisk   = claims.Count(c => c.AiRiskScore >= 70);
        var mediumRisk = claims.Count(c => c.AiRiskScore >= 40 && c.AiRiskScore < 70);
        var lowRisk    = claims.Count(c => c.AiRiskScore < 40);
        var avgScore   = claims.Any() ? Math.Round(claims.Average(c => (double)c.AiRiskScore!.Value), 1) : 0;

        // Monthly trend — last 6 months
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var monthly = await context.Claims
            .Where(c => c.AiRiskScore.HasValue && c.CreatedAt >= sixMonthsAgo)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new
            {
                Year    = g.Key.Year,
                Month   = g.Key.Month,
                AvgRisk = Math.Round(g.Average(x => (double)x.AiRiskScore!.Value), 1)
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                       "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        return Ok(new
        {
            TotalClaims      = allClaims,
            ScoredClaims     = claims.Count,
            HighRiskClaims   = highRisk,
            MediumRiskClaims = mediumRisk,
            LowRiskClaims    = lowRisk,
            AverageRiskScore = avgScore,
            RiskTrendMonthly = monthly.Select(m => new
            {
                Month   = monthNames[m.Month],
                AvgRisk = m.AvgRisk
            })
        });
    }

    // ──────────────────────────────────────────────────────────────────────────
    // FEATURE 3 — Agent Performance Intelligence
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Agent-level risk exposure and claim outcome analytics.
    /// </summary>
    [HttpGet("agent-performance-analytics")]
    public async Task<IActionResult> GetAgentPerformanceAnalytics([FromServices] AppDbContext context)
    {
        // Load agents with their user names
        var agents = await context.Agents
            .Include(a => a.User)
            .ToListAsync();

        var result = new List<object>();

        foreach (var agent in agents)
        {
            // Policies sold by this agent (via customer assignment)
            var agentPolicies = await context.Policies
                .Where(p => p.Customer.AssignedAgentId == agent.UserId)
                .ToListAsync();

            var totalPolicies = agentPolicies.Count;

            // Claims from those customers
            var policyIds = agentPolicies.Select(p => p.Id).ToHashSet();
            var claims = await context.Claims
                .Where(c => policyIds.Contains(c.PolicyId))
                .ToListAsync();

            var totalClaims = claims.Count;
            if (totalClaims == 0)
            {
                result.Add(new
                {
                    AgentId           = agent.UserId,
                    AgentName         = agent.User?.Name ?? "—",
                    TotalPolicies     = totalPolicies,
                    TotalClaims       = 0,
                    HighRiskPercent   = 0.0,
                    ApprovalRate      = 0.0,
                    RiskExposureScore = 0.0
                });
                continue;
            }

            var highRiskClaims   = claims.Count(c => (c.AiRiskScore ?? 0) >= 70);
            var mediumRiskClaims = claims.Count(c => (c.AiRiskScore ?? 0) >= 40 && (c.AiRiskScore ?? 0) < 70);
            var approvedClaims   = claims.Count(c => c.Status == Insurance.Domain.Enums.ClaimStatus.Approved
                                                  || c.Status == Insurance.Domain.Enums.ClaimStatus.Settled);

            var highRiskPct   = Math.Round((double)highRiskClaims / totalClaims * 100, 1);
            var approvalRate  = Math.Round((double)approvedClaims  / totalClaims * 100, 1);

            // RiskExposureScore formula
            var riskExposure = totalClaims > 0
                ? Math.Round((double)(highRiskClaims * 2 + mediumRiskClaims) / totalClaims * 100, 1)
                : 0;

            result.Add(new
            {
                AgentId           = agent.UserId,
                AgentName         = agent.User?.Name ?? "—",
                TotalPolicies     = totalPolicies,
                TotalClaims       = totalClaims,
                HighRiskPercent   = highRiskPct,
                ApprovalRate      = approvalRate,
                RiskExposureScore = riskExposure
            });
        }

        return Ok(result.OrderByDescending(r => ((dynamic)r).RiskExposureScore));
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs([FromServices] IAuditLogService auditLogService)
    {
        var logs = await auditLogService.GetAllLogsAsync();
        return Ok(logs);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // SMART ANALYTICS DASHBOARD — Single unified endpoint
    // ──────────────────────────────────────────────────────────────────────────
    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetDashboardSummary([FromServices] AppDbContext context)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = monthStart.AddMonths(-1);

        // ── Core Counts ──
        var totalPolicies    = await context.Policies.CountAsync();
        var activePolicies   = await context.Policies.CountAsync(p => p.Status == Insurance.Domain.Enums.PolicyStatus.Active);
        var draftPolicies    = await context.Policies.CountAsync(p => p.Status == Insurance.Domain.Enums.PolicyStatus.Draft);
        var expiredPolicies  = await context.Policies.CountAsync(p => p.Status == Insurance.Domain.Enums.PolicyStatus.Expired);
        var totalCustomers   = await context.Customers.CountAsync();
        var totalAgents      = await context.Agents.CountAsync();
        var totalClaims      = await context.Claims.CountAsync();

        // ── Claim Status Breakdown ──
        var claimsByStatus = await context.Claims
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count(), Total = g.Sum(x => x.ClaimAmount) })
            .ToListAsync();

        var pendingClaims  = claimsByStatus.FirstOrDefault(c => c.Status == "Submitted")?.Count ?? 0;
        var underReview    = claimsByStatus.FirstOrDefault(c => c.Status == "UnderReview")?.Count ?? 0;
        var approvedClaims = claimsByStatus.FirstOrDefault(c => c.Status == "Approved")?.Count ?? 0;
        var settledClaims  = claimsByStatus.FirstOrDefault(c => c.Status == "Settled")?.Count ?? 0;
        var rejectedClaims = claimsByStatus.FirstOrDefault(c => c.Status == "Rejected")?.Count ?? 0;

        // ── Revenue (from Payments table = actual collected premiums) ──
        var hasPayments = await context.Payments.AnyAsync();
        var totalRevenue     = hasPayments ? await context.Payments.SumAsync(p => p.Amount) : 0m;
        var monthRevenue     = hasPayments ? await context.Payments
                                  .Where(p => p.PaymentDate >= monthStart)
                                  .SumAsync(p => p.Amount) : 0m;
        var lastMonthRevenue = hasPayments ? await context.Payments
                                  .Where(p => p.PaymentDate >= lastMonthStart && p.PaymentDate < monthStart)
                                  .SumAsync(p => p.Amount) : 0m;

        // If no payments, fall back to sum of active policy premiums (these ARE collected)
        if (totalRevenue == 0)
        {
            totalRevenue = await context.Policies
                .Where(p => p.Status == Insurance.Domain.Enums.PolicyStatus.Active)
                .SumAsync(p => p.Premium);
        }
        if (monthRevenue == 0)
        {
            monthRevenue = await context.Policies
                .Where(p => p.Status == Insurance.Domain.Enums.PolicyStatus.Active
                         && p.StartDate.HasValue && p.StartDate >= monthStart)
                .SumAsync(p => p.Premium);
        }
        if (lastMonthRevenue == 0)
        {
            lastMonthRevenue = await context.Policies
                .Where(p => p.Status == Insurance.Domain.Enums.PolicyStatus.Active
                         && p.StartDate.HasValue && p.StartDate >= lastMonthStart && p.StartDate < monthStart)
                .SumAsync(p => p.Premium);
        }

        var revenueMoMChange = lastMonthRevenue > 0
            ? Math.Round((double)(monthRevenue - lastMonthRevenue) / (double)lastMonthRevenue * 100, 1)
            : 0;

        // ── 6-month Revenue Trend (from Payments, or from Policy activation months) ──
        var sixMonthsAgo = now.AddMonths(-6);
        var revenueTrend = hasPayments
            ? await context.Payments
                .Where(p => p.PaymentDate >= sixMonthsAgo)
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(x => x.Amount) })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync()
            : await context.Policies
                .Where(p => p.Status == Insurance.Domain.Enums.PolicyStatus.Active
                         && p.StartDate.HasValue && p.StartDate >= sixMonthsAgo)
                .GroupBy(p => new { p.StartDate!.Value.Year, p.StartDate!.Value.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(x => x.Premium) })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

        var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                       "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        // ── Total Sum Insured & Claim Exposure ──
        var totalSumInsured  = await context.Policies.Where(p => p.Status == Insurance.Domain.Enums.PolicyStatus.Active).SumAsync(p => p.CoverageAmount);
        var totalClaimAmount = await context.Claims
            .Where(c => c.Status != Insurance.Domain.Enums.ClaimStatus.Rejected)
            .SumAsync(c => c.ClaimAmount);
        var lossRatio = totalRevenue > 0 ? Math.Round((double)totalClaimAmount / (double)totalRevenue * 100, 1) : 0;

        // ── AI Risk Distribution ──
        var aiClaims   = await context.Claims.Where(c => c.AiRiskScore.HasValue).ToListAsync();
        var highRisk   = aiClaims.Count(c => c.AiRiskScore >= 70);
        var mediumRisk = aiClaims.Count(c => c.AiRiskScore >= 40 && c.AiRiskScore < 70);
        var lowRisk    = aiClaims.Count(c => c.AiRiskScore < 40);
        var avgScore   = aiClaims.Any() ? Math.Round(aiClaims.Average(c => (double)c.AiRiskScore!.Value), 1) : 0;

        // ── New Policies This Month ──
        var newPoliciesThisMonth = await context.Policies.CountAsync(p => p.StartDate >= monthStart);
        var newCustomersThisMonth = await context.Customers
            .CountAsync(c => c.User != null && c.User.CreatedAt >= monthStart);

        // ── Policy Expiring Soon (next 30 days) ──
        var expiringIn30Days = await context.Policies
            .CountAsync(p => p.Status == Insurance.Domain.Enums.PolicyStatus.Active
                          && p.EndDate <= now.AddDays(30)
                          && p.EndDate >= now);

        // ── Claims submitted this month ──
        var claimsThisMonth = await context.Claims.CountAsync(c => c.CreatedAt >= monthStart);

        // ── Claim Approval Rate ──
        var decidedClaims = approvedClaims + settledClaims + rejectedClaims;
        var approvalRate = decidedClaims > 0
            ? Math.Round((double)(approvedClaims + settledClaims) / decidedClaims * 100, 1)
            : 0;

        // ── Top Policy Products by count (via PolicyApplication -> PolicyProduct) ──
        var topProducts = await context.Policies
            .Where(p => p.ApplicationId != null)
            .Include(p => p.Application!)
                .ThenInclude(a => a.Product)
            .ToListAsync();

        var topProductsByName = topProducts
            .GroupBy(p => p.Application?.Product?.Name ?? "Unknown")
            .Select(g => new { ProductName = g.Key, PolicyCount = g.Count(), TotalPremium = g.Sum(x => x.Premium) })
            .OrderByDescending(g => g.PolicyCount)
            .Take(5)
            .ToList();

        return Ok(new
        {
            // KPIs
            TotalPolicies          = totalPolicies,
            ActivePolicies         = activePolicies,
            DraftPolicies          = draftPolicies,
            ExpiredPolicies        = expiredPolicies,
            TotalCustomers         = totalCustomers,
            TotalAgents            = totalAgents,
            TotalClaims            = totalClaims,
            PendingClaims          = pendingClaims,
            UnderReviewClaims      = underReview,
            ApprovedClaims         = approvedClaims,
            SettledClaims          = settledClaims,
            RejectedClaims         = rejectedClaims,
            NewPoliciesThisMonth   = newPoliciesThisMonth,
            NewCustomersThisMonth  = newCustomersThisMonth,
            ClaimsThisMonth        = claimsThisMonth,
            ExpiringIn30Days       = expiringIn30Days,

            // Financial
            TotalRevenue           = totalRevenue,
            MonthRevenue           = monthRevenue,
            LastMonthRevenue       = lastMonthRevenue,
            RevenueMoMChange       = revenueMoMChange,
            TotalSumInsured        = totalSumInsured,
            TotalClaimExposure     = totalClaimAmount,
            LossRatio              = lossRatio,
            ClaimApprovalRate      = approvalRate,

            // Revenue Trend
            RevenueTrend           = revenueTrend.Select(r => new
            {
                Month   = monthNames[r.Month] + " " + r.Year,
                Revenue = r.Revenue
            }),

            // AI Risk
            ScoredClaims           = aiClaims.Count,
            HighRiskClaims         = highRisk,
            MediumRiskClaims       = mediumRisk,
            LowRiskClaims          = lowRisk,
            AverageRiskScore       = avgScore,

            // Top Products
            TopProducts            = topProductsByName.Select(p => new
            {
                Product = p.ProductName,
                Count   = p.PolicyCount,
                Premium = p.TotalPremium
            }),

            // Claims Breakdown 
            ClaimsBreakdown        = claimsByStatus.Select(c => new
            {
                c.Status,
                c.Count,
                c.Total
            })
        });
    }
}