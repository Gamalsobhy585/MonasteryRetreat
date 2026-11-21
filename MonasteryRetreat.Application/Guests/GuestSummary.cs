namespace MonasteryRetreat.Application.Guests;

/// <summary>A guest row shown in the "previous guest" search results.</summary>
public sealed class GuestSummary
{
    public long Id { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public string GovernorateName { get; set; } = string.Empty;
    public string GuestChurch { get; set; } = string.Empty;

    public int ReservationCount { get; set; }

    /// <summary>Arrival date of the most recent reservation, if any.</summary>
    public DateOnly? LastArrival { get; set; }

    public string LastVisitText =>
        LastArrival?.ToString("dd/MM/yyyy") ?? "—";
}
