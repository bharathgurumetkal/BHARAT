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

        public User User { get; set; } = default!;
        public User? AssignedAgent { get; set; }
    }
}
