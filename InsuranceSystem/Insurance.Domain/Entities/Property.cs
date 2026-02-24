using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Domain.Entities
{
    public class Property
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }

        public string Category { get; set; } = default!;
        public string SubCategory { get; set; } = default!;
        public string Address { get; set; } = default!;
        public int YearBuilt { get; set; }
        public decimal MarketValue { get; set; }
        public string RiskZone { get; set; } = default!;
        public bool HasSecuritySystem { get; set; }
    }
}
