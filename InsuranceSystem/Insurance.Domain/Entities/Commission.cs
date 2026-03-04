namespace Insurance.Domain.Entities
{
    public class Commission
    {
        public Guid Id { get; set; }

        /// <summary>
        /// The UserId of the assigned agent (extracted from PolicyApplication.AssignedAgentId).
        /// </summary>
        public Guid AgentId { get; set; }

        public Guid PolicyId { get; set; }

        public decimal CommissionRate { get; set; }       // e.g. 0.10 for 10%
        public decimal CommissionAmount { get; set; }     // Premium × CommissionRate

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsPaid { get; set; } = false;

        // Navigation
        public Policy Policy { get; set; } = default!;
    }
}
