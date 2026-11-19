using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonasteryRetreat.Domain.Entities;

public class Stay
{
    public long Id { get; set; }

    public long GuestId { get; set; }
    public Guest Guest { get; set; } = null!;

    public long? ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public string NationalIdAtCheckin { get; set; } = string.Empty;

    public DateTime ActualArrivalAt { get; set; } = DateTime.UtcNow;

    public DateOnly ExpectedDepartureDate { get; set; }

    public DateTime? ActualDepartureAt { get; set; }

    public string Status { get; set; } = "active";

  
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RoomAssignment> RoomAssignments { get; set; }
        = new List<RoomAssignment>();

    public ICollection<CustodyItem> CustodyItems { get; set; }
        = new List<CustodyItem>();
}
