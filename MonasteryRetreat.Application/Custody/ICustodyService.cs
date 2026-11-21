namespace MonasteryRetreat.Application.Custody;

public interface ICustodyService
{
    Task<CustodyOverview> GetOverviewAsync(
        CancellationToken cancellationToken = default);

    Task<CustodyReturnedPage> GetReturnedHistoryAsync(
        int page, int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        long stayId,
        string description,
        int quantity,
        string? storageReference,
        string? notes,
        CancellationToken cancellationToken = default);

    Task ReturnAsync(
        long itemId,
        CancellationToken cancellationToken = default);
}
