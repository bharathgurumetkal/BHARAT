using System;

namespace Insurance.Domain.Entities
{
    public class PolicyApplication
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }
        public PolicyProduct Product { get; set; } = default!;

        public Guid CustomerId { get; set; }
        public User Customer { get; set; } = default!;

        public Guid? AssignedAgentId { get; set; }
        public User? AssignedAgent { get; set; }

        public string PropertySubCategory { get; set; } = default!;
        public string Address { get; set; } = default!;
        public int YearBuilt { get; set; }
        public decimal MarketValue { get; set; }
        public string RiskZone { get; set; } = default!;
        public bool HasSecuritySystem { get; set; }
        public decimal RequestedCoverageAmount { get; set; }
        public decimal CalculatedPremium { get; set; }

        // Status: Submitted | AssignedToAgent | ApprovedByAgent | RejectedByAgent
        public string Status { get; set; } = "Submitted";

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AssignedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
