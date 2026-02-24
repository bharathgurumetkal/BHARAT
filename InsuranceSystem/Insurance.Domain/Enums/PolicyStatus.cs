using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Domain.Enums
{
    public enum PolicyStatus
    {
        Draft,
        AwaitingPayment,
        Active,
        Expired,
        Cancelled
    }
}
