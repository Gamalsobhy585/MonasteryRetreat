using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonasteryRetreat.Domain.Entities
{
    public class LiturgyAssignment
    {
        public long Id { get; set; }
        public long LiturgyScheduleId { get; set; }
        public LiturgySchedule LiturgySchedule { get; set; } = null!;
        public long StayId { get; set; }
        public Stay Stay { get; set; } = null!;
        public string Role { get; set; } = "congregation";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
