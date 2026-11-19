using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonasteryRetreat.Domain.Entities
{
    public class RetreatProgramItem
    {
        public long Id { get; set; }
        public string TitleAr { get; set; } = string.Empty;
        public string? DescriptionAr { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
