using System;

namespace Insurance.Application.DTOs.Policy
{
    public class RenewPolicyResponseDto
    {
        public Guid PolicyId { get; set; }
        public DateTime NewStartDate { get; set; }
        public DateTime NewEndDate { get; set; }
        public string Status { get; set; } = default!;
    }
}
