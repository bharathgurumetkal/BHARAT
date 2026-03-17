using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? AssignedAgentId { get; set; }
        public string Status { get; set; } = "Unassigned";
        
        // AI Prospecting Cache
        public int? AiRenewalScore { get; set; }
        public string? AiLikelihood { get; set; }
        public double? AiChurnProbability { get; set; }
        public string? AiExplanation { get; set; }
        public string? AiRecommendedAction { get; set; }
        public DateTime? AiLastAnalyzedAt { get; set; }

        public User User { get; set; } = default!;
        public User? AssignedAgent { get; set; }
    }
}
