using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Domain.Entities
{
    public class Agent
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string LicenseNumber { get; set; } = default!;
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = default!;
    }
}
