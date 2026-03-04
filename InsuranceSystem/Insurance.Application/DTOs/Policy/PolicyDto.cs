using System;

namespace Insurance.Application.DTOs.Policy
{
    public class PolicyDto
    {
        public Guid Id { get; set; }
        public string PolicyNumber { get; set; } = default!;
        public Guid CustomerId { get; set; }
        public Guid PropertyId { get; set; }
        public decimal CoverageAmount { get; set; }
        public decimal Premium { get; set; }
        public string Status { get; set; } = default!;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? ApplicationId { get; set; }
        public string ProductName { get; set; } = default!;
    }
}
