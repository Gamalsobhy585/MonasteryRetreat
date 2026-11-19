using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonasteryRetreat.Domain.Entities
{
    public class CheckoutRecord
    {
        public long Id { get; set; }
        public long StayId { get; set; }
        public Stay Stay { get; set; } = null!;
        public string DepartureType { get; set; } = "normal";
        public DateTime DepartureAt { get; set; }
        public bool CustodyCleared { get; set; }
        public string? CustodyOverrideReason { get; set; }
        public string? Notes { get; set; }

    }

}
