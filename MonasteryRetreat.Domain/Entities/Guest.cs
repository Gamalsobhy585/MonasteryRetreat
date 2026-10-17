using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MonasteryRetreat.Domain.Entities;

public class Guest
{
    public long Id { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public string? NationalId { get; set; }

    public long? GovernorateId { get; set; }
    public Governorate? Governorate { get; set; }

    public string? GuestChurch { get; set; } = string.Empty;

    public string? ConfessionFatherName { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Reservation> Reservations { get; set; }
        = new List<Reservation>();

    public ICollection<Stay> Stays { get; set; }
        = new List<Stay>();
}

