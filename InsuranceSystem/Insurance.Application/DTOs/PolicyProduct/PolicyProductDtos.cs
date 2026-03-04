namespace Insurance.Application.DTOs.PolicyProduct
{
    public class CreatePolicyProductDto
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string PropertyCategory { get; set; } = default!;
        public decimal BaseRatePercentage { get; set; }
        public decimal MaxCoverageAmount { get; set; }
    }

    public class PolicyProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string PropertyCategory { get; set; } = default!;
        public decimal BaseRatePercentage { get; set; }
        public decimal MaxCoverageAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
