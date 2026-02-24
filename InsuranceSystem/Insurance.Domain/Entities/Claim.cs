using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insurance.Domain.Enums;

namespace Insurance.Domain.Entities
{
    public class Claim
    {
        public Guid Id { get; set; }
        public Guid PolicyId { get; set; }

        public string IncidentType { get; set; } = default!;
        public decimal ClaimAmount { get; set; }
        public string Description { get; set; } = default!;

        public int FraudScore { get; set; }
        public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;

        public Guid? ClaimsOfficerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
