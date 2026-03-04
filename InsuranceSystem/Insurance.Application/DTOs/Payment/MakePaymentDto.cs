using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Application.DTOs.Payment
{
    public class MakePaymentDto
    {
        public Guid PolicyId { get; set; }
        public decimal Amount { get; set; }
    }
}
