namespace MonasteryRetreat.Application.Reservations;

public sealed class ExistingGuestReservationInput
{
    public long GuestId { get; set; }

    public DateTime ExpectedArrival { get; set; }
    public DateTime ExpectedDeparture { get; set; }

    public string Notes { get; set; } = string.Empty;
}
