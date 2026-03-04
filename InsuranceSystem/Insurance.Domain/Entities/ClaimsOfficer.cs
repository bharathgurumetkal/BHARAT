using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Domain.Entities
{
    public class ClaimsOfficer
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string Department { get; set; } = default!;

        public User User { get; set; } = default!;
    }
}
