using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Application.DTOs.Admin
{
    public class AssignCustomerDto
    {
        public Guid CustomerId { get; set; }
        public Guid AgentId { get; set; }
    }
}
