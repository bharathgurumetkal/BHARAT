using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Domain.Entities
{
    public class ClaimDocument
    {
        public Guid Id { get; set; }

        public Guid ClaimId { get; set; }
        public Claim Claim { get; set; } = default!;

        public string FileName { get; set; } = default!;
        public string FilePath { get; set; } = default!;
    }
}
