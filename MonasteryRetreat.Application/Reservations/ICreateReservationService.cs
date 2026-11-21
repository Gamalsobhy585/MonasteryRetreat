namespace MonasteryRetreat.Application.Reservations;

public interface ICreateReservationService
{
    /// <summary>Creates a new guest and a reservation for them.</summary>
    Task<long> CreateAsync(
        NewReservationInput input,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a reservation for a guest that already exists.</summary>
    Task<long> CreateForExistingGuestAsync(
        ExistingGuestReservationInput input,
        CancellationToken cancellationToken = default);
}
