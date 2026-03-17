using System;

namespace Insurance.Application.DTOs.Claim
{
    public class AiProspectInputDto
    {
        public int PolicyCount { get; set; }
        public decimal TotalPremiumPaid { get; set; }
        public int ClaimCount { get; set; }
        public int CustomerTenureDays { get; set; }
    }

    public class AiProspectOutputDto
    {
        public int RenewalScore { get; set; }
        public string Likelihood { get; set; } = string.Empty;
        public double ChurnProbability { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public bool IsFallback { get; set; }
    }

    public class SmartProspectDto
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int PolicyCount { get; set; }
        public decimal TotalPremiumPaid { get; set; }
        public int ClaimCount { get; set; }
        public int CustomerTenureDays { get; set; }
        public DateTime? LastAnalyzedAt { get; set; }
        public string? Source { get; set; } // "Cache" or "AI"
        public AiProspectOutputDto? AiAnalysis { get; set; }
    }
}
