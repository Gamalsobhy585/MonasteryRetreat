using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonasteryRetreat.Domain.Entities
{
    public class CustodyItem
    {
        public long Id { get; set; }
        public long StayId { get; set; }
        public Stay Stay { get; set; } = null!;
        public string ItemDescription { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public string? StorageReference { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReturnedAt { get; set; }
        public string? Notes { get; set; }
    }
}
