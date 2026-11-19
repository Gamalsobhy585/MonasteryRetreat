namespace MonasteryRetreat.Application.Reservations;

public interface ICreateReservationService
{
    Task<long> CreateAsync(
        NewReservationInput input,
        CancellationToken cancellationToken = default);
}