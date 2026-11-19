using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonasteryRetreat.Domain.Entities;

public class Room
{
    public long Id { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public string? NameAr { get; set; }

    public int Capacity { get; set; } = 8;

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }

    public ICollection<RoomAssignment> RoomAssignments { get; set; }
        = new List<RoomAssignment>();
}
