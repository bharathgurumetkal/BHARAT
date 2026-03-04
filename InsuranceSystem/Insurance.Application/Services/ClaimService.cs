using Insurance.Application.DTOs.Claim;
using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;


namespace Insurance.Application.Services;

public class ClaimService : IClaimService
{
    private readonly IClaimRepository _claimRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IAiClaimClient _aiClaimClient;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;

    public ClaimService(
        IClaimRepository claimRepository,
        IPolicyRepository policyRepository,
        IAiClaimClient aiClaimClient,
        INotificationService notificationService,
        IUserRepository userRepository)
    {
        _claimRepository = claimRepository;
        _policyRepository = policyRepository;
        _aiClaimClient = aiClaimClient;
        _notificationService = notificationService;
        _userRepository = userRepository;
    }

    public async Task SubmitClaimAsync(SubmitClaimDto dto)
    {
        var policy = await _policyRepository.GetByIdAsync(dto.PolicyId);

        if (policy == null || policy.Status != PolicyStatus.Active)
            throw new Exception("Policy not active.");

        var claim = new Claim
        {
            Id = Guid.NewGuid(),
            PolicyId = dto.PolicyId,
            ClaimAmount = dto.ClaimAmount,
            Reason = dto.Reason,
            Status = ClaimStatus.Submitted
        };

        // Basic Fraud Check: flag for review if amount is suspiciously high
        if (dto.ClaimAmount > policy.CoverageAmount * 0.8m)
            claim.Status = ClaimStatus.UnderReview;

        await _claimRepository.AddAsync(claim);
        
        foreach (var doc in dto.Documents)
        {
            var document = new ClaimDocument
            {
                Id = Guid.NewGuid(),
                ClaimId = claim.Id,
                FileName = $"Document_{Guid.NewGuid().ToString().Substring(0,8)}", 
                FilePath = doc // Store full Cloudinary URL
            };

            await _claimRepository.AddDocumentAsync(document);
        }
        
        await _claimRepository.SaveChangesAsync();

        // Notify Customer
        await _notificationService.CreateAsync(
            policy.CustomerId,
            "Claim Submitted",
            $"Your claim CLM-{claim.Id.ToString().Substring(0, 8)} for {dto.ClaimAmount:C} has been submitted.",
            "Info"
        );

        // Notify Claims Officers
        var officers = await _userRepository.GetByRoleAsync("ClaimsOfficer");
        foreach (var officer in officers)
        {
            await _notificationService.CreateAsync(
                officer.Id,
                "New Claim for Review",
                $"A new claim has been submitted by {policy.Customer?.User?.Name ?? "Customer"} for {dto.ClaimAmount:C}.",
                "Info"
            );
        }

        // AI Analysis after save (non-blocking for the customer)
        try 
        {
            var request = new AiClaimRequestDto
            {
                ClaimAmount = dto.ClaimAmount,
                CoverageAmount = policy.CoverageAmount,
                PolicyAgeDays = policy.StartDate.HasValue ? (DateTime.UtcNow - policy.StartDate.Value).Days : 0,
                RiskZone = policy.Property?.RiskZone ?? "Unknown",
                MarketValue = policy.Property?.MarketValue ?? 0,
                HasSecuritySystem = policy.Property?.HasSecuritySystem ?? false,
                YearBuilt = policy.Property?.YearBuilt ?? 0,
                ClaimReason = dto.Reason
            };

            var aiResult = await _aiClaimClient.AnalyzeClaimAsync(request);

            if (aiResult != null)
            {
                claim.AiRiskScore = aiResult.RiskScore;
                claim.AiRiskLevel = aiResult.RiskLevel;
                claim.AiFraudProbability = aiResult.FraudProbability;
                claim.AiExplanation = aiResult.Explanation;
                claim.AiRecommendation = aiResult.Recommendation;
                claim.AiSource = aiResult.IsFallback ? "Heuristic" : "LLM";
            }
            else
            {
                claim.AiRiskLevel = "Unknown";
                claim.AiExplanation = "AI analysis unreachable.";
                claim.AiSource = "None";
            }
            
            await _claimRepository.SaveChangesAsync();

            // High Risk AI Scoring Notification
            if (claim.AiRiskScore >= 70)
            {
                var admins = await _userRepository.GetByRoleAsync("Admin");
                foreach (var admin in admins)
                {
                    await _notificationService.CreateAsync(
                        admin.Id,
                        "🚨 Fraud Risk Alert",
                        $"AI scored claim CLM-{claim.Id.ToString().Substring(0, 8)} with a Risk Score of {claim.AiRiskScore}.",
                        "Risk"
                    );
                }

                var officers2 = await _userRepository.GetByRoleAsync("ClaimsOfficer");
                foreach (var officer in officers2)
                {
                    await _notificationService.CreateAsync(
                        officer.Id,
                        "High Risk Claim Detected",
                        $"Claim CLM-{claim.Id.ToString().Substring(0, 8)} requires urgent review (Score: {claim.AiRiskScore}).",
                        "Warning"
                    );
                }
            }
        }
        catch (Exception)
        {
             // Log error if needed, but don't block
             claim.AiRiskLevel = "Unknown";
             claim.AiExplanation = "Unexpected error during AI analysis.";
             claim.AiSource = "Fallback";
             await _claimRepository.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Move a claim from Submitted → UnderReview.
    /// </summary>
    public async Task StartReviewAsync(Guid claimId)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId);

        if (claim == null)
            throw new Exception("Claim not found.");

        if (claim.Status != ClaimStatus.Submitted)
            throw new Exception($"Cannot start review. Current status: {claim.Status}. Expected: Submitted.");

        claim.Status = ClaimStatus.UnderReview;

        await _claimRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Approve or reject a claim that is UnderReview.
    /// </summary>
    public async Task ReviewClaimAsync(Guid claimId, bool approve)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId);

        if (claim == null)
            throw new Exception("Claim not found.");

        if (claim.Status != ClaimStatus.UnderReview)
            throw new Exception($"Cannot review. Current status: {claim.Status}. Expected: UnderReview.");

        claim.Status = approve
            ? ClaimStatus.Approved
            : ClaimStatus.Rejected;

        await _claimRepository.SaveChangesAsync();

        // Notify Customer
        await _notificationService.CreateAsync(
            claim.Policy?.CustomerId ?? Guid.Empty,
            approve ? "Claim Approved" : "Claim Rejected",
            approve 
                ? $"Your claim for {claim.ClaimAmount:C} has been approved."
                : $"Your claim for {claim.ClaimAmount:C} has been rejected.",
            approve ? "Success" : "Risk"
        );
    }

    /// <summary>
    /// Settle an approved claim. Only Approved claims can be settled.
    /// </summary>
    public async Task SettleClaimAsync(Guid claimId)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId);

        if (claim == null)
            throw new Exception("Claim not found.");

        if (claim.Status != ClaimStatus.Approved)
            throw new Exception("Only approved claims can be settled.");

        claim.Status = ClaimStatus.Settled;

        await _claimRepository.SaveChangesAsync();

        // Notify Customer
        await _notificationService.CreateAsync(
            claim.Policy?.CustomerId ?? Guid.Empty,
            "Claim Settled",
            $"Funds for your claim have been released.",
            "Success"
        );
    }

    public async Task<List<ClaimDto>> GetClaimsByCustomerAsync(Guid customerUserId)
    {
        var claims = await _claimRepository.GetClaimsByCustomerAsync(customerUserId);
        return claims.Select(c => MapToDto(c)).ToList();
    }

    public async Task<List<ClaimDto>> GetAllClaimsAsync()
    {
        var claims = await _claimRepository.GetAllAsync();
        return claims.Select(c => MapToDto(c)).ToList();
    }

    private static ClaimDto MapToDto(Claim c)
    {
        return new ClaimDto
        {
            Id = c.Id,
            ClaimAmount = c.ClaimAmount,
            Reason = c.Reason,
            Status = c.Status.ToString(),
            CreatedAt = c.CreatedAt,

            // Policy Details
            PolicyId = c.PolicyId,
            PolicyNumber = c.Policy?.PolicyNumber ?? "N/A",
            PolicyCoverageAmount = c.Policy?.CoverageAmount ?? 0,
            PolicyPremium = c.Policy?.Premium ?? 0,
            PolicyStatus = c.Policy?.Status.ToString() ?? "Unknown",
            PolicyStartDate = c.Policy?.StartDate,
            PolicyEndDate = c.Policy?.EndDate,
            PolicyProductName = c.Policy?.Application?.Product?.Name ?? "General Insurance",

            // Customer Details
            CustomerName = c.Policy?.Customer?.User?.Name ?? "Unknown",
            CustomerEmail = c.Policy?.Customer?.User?.Email ?? "N/A",
            CustomerPhone = c.Policy?.Customer?.User?.PhoneNumber ?? "N/A",

            // Property Details
            PropertyCategory = c.Policy?.Property?.Category ?? "N/A",
            PropertySubCategory = c.Policy?.Property?.SubCategory ?? "N/A",
            PropertyAddress = c.Policy?.Property?.Address ?? "N/A",
            PropertyYearBuilt = c.Policy?.Property?.YearBuilt ?? 0,
            PropertyMarketValue = c.Policy?.Property?.MarketValue ?? 0,
            PropertyRiskZone = c.Policy?.Property?.RiskZone ?? "N/A",
            PropertyHasSecuritySystem = c.Policy?.Property?.HasSecuritySystem ?? false,

            // Documents
            Documents = c.Documents.Select(d => new ClaimDocumentDto
            {
                FileName = d.FileName,
                FilePath = d.FilePath
            }).ToList(),

            // AI Data
            AiRiskScore = c.AiRiskScore,
            AiRiskLevel = c.AiRiskLevel,
            AiFraudProbability = c.AiFraudProbability,
            AiExplanation = c.AiExplanation,
            AiRecommendation = c.AiRecommendation,
            AiSource = c.AiSource
        };
    }
}