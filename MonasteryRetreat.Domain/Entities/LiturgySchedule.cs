using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonasteryRetreat.Domain.Entities
{
    public class LiturgySchedule
    {
        public long Id { get; set; }
        public long MonasteryChurchId { get; set; }
        public MonasteryChurch MonasteryChurch { get; set; } = null!;
        public DateOnly LiturgyDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public string? Notes { get; set; }
    }
}
