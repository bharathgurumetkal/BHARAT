using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insurance.Domain.Enums;

namespace Insurance.Domain.Entities
{
    public class Policy
    {
        public Guid Id { get; set; }
        public string PolicyNumber { get; set; } = default!;
        public Guid CustomerId { get; set; }
        public Guid PropertyId { get; set; }

        public decimal CoverageAmount { get; set; }
        public decimal Premium { get; set; }

        public PolicyStatus Status { get; set; } = PolicyStatus.Draft;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
