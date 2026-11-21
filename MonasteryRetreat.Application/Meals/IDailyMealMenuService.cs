namespace MonasteryRetreat.Application.Meals;

public interface IDailyMealMenuService
{
    Task<IReadOnlyList<DailyMealMenuRow>> ListAsync(
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        DateOnly date, string? breakfast, string? lunch, string? dinner,
        CancellationToken cancellationToken = default);
}
