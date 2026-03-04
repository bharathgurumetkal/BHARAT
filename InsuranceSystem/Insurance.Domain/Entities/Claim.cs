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
        public Policy Policy { get; set; } = default!;

        public decimal ClaimAmount { get; set; }

        public string Reason { get; set; } = default!;

        public ClaimStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // AI Analysis Fields
        public int? AiRiskScore { get; set; }
        public string? AiRiskLevel { get; set; }
        public double? AiFraudProbability { get; set; }
        public string? AiExplanation { get; set; }
        public string? AiRecommendation { get; set; }
        public string? AiSource { get; set; }

        public ICollection<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();
    }
}
