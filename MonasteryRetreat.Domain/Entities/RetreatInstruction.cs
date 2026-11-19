using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonasteryRetreat.Domain.Entities
{
    public class RetreatInstruction
    {
        public long Id { get; set; }
        public string InstructionAr { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
