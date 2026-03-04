using System;

namespace Insurance.Domain.Entities
{
    public class PolicyProduct
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string PropertyCategory { get; set; } = default!;
        public decimal BaseRatePercentage { get; set; }
        public decimal MaxCoverageAmount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
