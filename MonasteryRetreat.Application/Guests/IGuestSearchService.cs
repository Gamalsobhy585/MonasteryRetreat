namespace MonasteryRetreat.Application.Guests;

public interface IGuestSearchService
{
    /// <summary>
    /// Finds guests whose name or phone contains <paramref name="term"/>.
    /// Returns an empty list when the term is shorter than 2 characters.
    /// </summary>
    Task<IReadOnlyList<GuestSummary>> SearchAsync(
        string term,
        CancellationToken cancellationToken = default);
}
