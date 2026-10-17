using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MonasteryRetreat.Domain.Entities;

public class Reservation
{
    public long Id { get; set; }

    public long GuestId { get; set; }
    public Guest Guest { get; set; } = null!;

    public DateOnly ExpectedArrivalDate { get; set; }
    public DateOnly ExpectedDepartureDate { get; set; }

    public string Status { get; set; } = "pending";
    public string Source { get; set; } = "phone";

    public string? Notes { get; set; }


    public Stay? Stay { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
