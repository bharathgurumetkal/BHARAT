using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Application.DTOs.Claim
{
    public class SubmitClaimDto
    {
        public Guid PolicyId { get; set; }
        public decimal ClaimAmount { get; set; }
        public string Reason { get; set; } = default!;

        // New incident detail fields
        public DateTime? IncidentDate { get; set; }
        public string? IncidentLocation { get; set; }
        public string? DamageType { get; set; }

        public List<string> Documents { get; set; } = new();
    }
}
