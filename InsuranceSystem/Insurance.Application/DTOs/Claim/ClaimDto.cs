using System;

namespace Insurance.Application.DTOs.Claim
{
    public class AssignOfficerToClaimDto
    {
        public Guid OfficerUserId { get; set; }
    }

    public class ClaimDto

    {
        public Guid Id { get; set; }
        public decimal ClaimAmount { get; set; }
        public string Reason { get; set; } = default!;
        public string Status { get; set; } = default!;
        public DateTime CreatedAt { get; set; }

        // Officer Assignment
        public Guid? AssignedOfficerId { get; set; }
        public string? AssignedOfficerName { get; set; }


        // Policy Details
        public Guid PolicyId { get; set; }
        public string PolicyNumber { get; set; } = default!;
        public decimal PolicyCoverageAmount { get; set; }
        public decimal PolicyPremium { get; set; }
        public string PolicyStatus { get; set; } = default!;
        public DateTime? PolicyStartDate { get; set; }
        public DateTime? PolicyEndDate { get; set; }
        public string PolicyProductName { get; set; } = default!;

        // Customer Details
        public string CustomerName { get; set; } = default!;
        public string CustomerEmail { get; set; } = default!;
        public string CustomerPhone { get; set; } = default!;

        // Property Details
        public string PropertyCategory { get; set; } = default!;
        public string PropertySubCategory { get; set; } = default!;
        public string PropertyAddress { get; set; } = default!;
        public int PropertyYearBuilt { get; set; }
        public decimal PropertyMarketValue { get; set; }
        public string PropertyRiskZone { get; set; } = default!;
        public bool PropertyHasSecuritySystem { get; set; }

        // AI Analysis
        public int? AiRiskScore { get; set; }
        public string? AiRiskLevel { get; set; }
        public double? AiFraudProbability { get; set; }
        public string? AiExplanation { get; set; }
        public string? AiRecommendation { get; set; }
        public string? AiSource { get; set; }

        // Documents
        public List<ClaimDocumentDto> Documents { get; set; } = new();
    }

    public class ClaimDocumentDto
    {
        public string FileName { get; set; } = default!;
        public string FilePath { get; set; } = default!;
    }
}
