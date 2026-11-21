namespace MonasteryRetreat.Application.Accommodation;

public interface IAccommodationService
{
    Task<AccommodationOverview> GetOverviewAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Puts a confirmed guest who has no room into a room.</summary>
    Task AssignAsync(
        long stayId,
        long roomId,
        CancellationToken cancellationToken = default);

    /// <summary>Moves a housed guest to another room.</summary>
    Task MoveAsync(
        long stayId,
        long toRoomId,
        CancellationToken cancellationToken = default);
}
