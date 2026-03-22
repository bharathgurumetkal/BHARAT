namespace Insurance.Application.DTOs.Claim;

public class AiClaimRequestDto
{
    public decimal ClaimAmount { get; set; }
    public decimal CoverageAmount { get; set; }
    public int PolicyAgeDays { get; set; }
    public string RiskZone { get; set; } = default!;
    public decimal MarketValue { get; set; }
    public bool HasSecuritySystem { get; set; }
    public int YearBuilt { get; set; }
    public string ClaimReason { get; set; } = default!;

    // New incident detail fields
    public string? DamageType { get; set; }
    public string? IncidentLocation { get; set; }
    public DateTime? IncidentDate { get; set; }
}

public class AiClaimResponseDto
{
    public int RiskScore { get; set; }
    public string RiskLevel { get; set; } = default!;
    public double FraudProbability { get; set; }
    public string Explanation { get; set; } = default!;
    public string Recommendation { get; set; } = default!;
    public bool IsFallback { get; set; }
}
