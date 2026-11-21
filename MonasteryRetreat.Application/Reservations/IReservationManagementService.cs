namespace MonasteryRetreat.Application.Reservations;

public interface IReservationManagementService
{
    Task<ReservationListResult> ListAsync(
        ReservationQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>The guest has arrived: starts their stay.</summary>
    Task ConfirmArrivalAsync(
        long reservationId,
        CancellationToken cancellationToken = default);

    /// <summary>Cancels a reservation whose guest has not arrived.</summary>
    Task CancelAsync(
        long reservationId,
        CancellationToken cancellationToken = default);

    /// <summary>The guest has left: ends their stay.</summary>
    Task EndStayAsync(
        long reservationId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        long reservationId,
        DateTime expectedArrival,
        DateTime expectedDeparture,
        string? notes,
        CancellationToken cancellationToken = default);
}
