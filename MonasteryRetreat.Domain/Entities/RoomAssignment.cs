using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MonasteryRetreat.Domain.Entities;

public class RoomAssignment
{
    public long Id { get; set; }

    public long StayId { get; set; }
    public Stay Stay { get; set; } = null!;

    public long RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReleasedAt { get; set; }

}
