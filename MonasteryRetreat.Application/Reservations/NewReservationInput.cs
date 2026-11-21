namespace MonasteryRetreat.Application.Reservations;

public sealed class NewReservationInput
{
    public DateTime ExpectedArrival { get; set; }
    public DateTime ExpectedDeparture { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public long GovernorateId { get; set; }
    public string GuestChurch { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }
    public string Notes { get; set; } = string.Empty;
}