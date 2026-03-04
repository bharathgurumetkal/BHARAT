namespace Insurance.Application.DTOs.PolicyApplication
{
    public class ApplyForProductDto
    {
        public Guid ProductId { get; set; }
        public string PropertySubCategory { get; set; } = default!;
        public string Address { get; set; } = default!;
        public int YearBuilt { get; set; }
        public decimal MarketValue { get; set; }
        public string RiskZone { get; set; } = default!;
        public bool HasSecuritySystem { get; set; }
        public decimal RequestedCoverageAmount { get; set; }
    }

    public class AssignAgentToApplicationDto
    {
        public Guid AgentId { get; set; }
    }

    public class PolicyApplicationDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = default!;
        public Guid? AssignedAgentId { get; set; }
        public string? AssignedAgentName { get; set; }
        public string PropertySubCategory { get; set; } = default!;
        public string Address { get; set; } = default!;
        public int YearBuilt { get; set; }
        public decimal MarketValue { get; set; }
        public string RiskZone { get; set; } = default!;
        public bool HasSecuritySystem { get; set; }
        public decimal RequestedCoverageAmount { get; set; }
        public decimal CalculatedPremium { get; set; }
        public string Status { get; set; } = default!;
        public DateTime SubmittedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
