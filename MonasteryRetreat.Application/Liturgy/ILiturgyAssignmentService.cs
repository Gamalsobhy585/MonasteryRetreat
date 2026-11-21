namespace MonasteryRetreat.Application.Liturgy;

public interface ILiturgyAssignmentService
{
    Task<LiturgyOverview> GetOverviewAsync(
        DateOnly date, CancellationToken cancellationToken = default);

    Task SaveAsync(
        DateOnly date, long stayId, long churchId, string role,
        CancellationToken cancellationToken = default);
}
