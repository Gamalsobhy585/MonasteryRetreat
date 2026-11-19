using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MonasteryRetreat.Domain.Entities;

// StayAdjustment.cs
public class StayAdjustment
{
    public long Id { get; set; }
    public long StayId { get; set; }
    public Stay Stay { get; set; } = null!;
    public DateOnly OldDepartureDate { get; set; }
    public DateOnly NewDepartureDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}