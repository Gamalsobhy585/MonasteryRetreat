namespace MonasteryRetreat.Application.Custody;

public sealed record CustodyGuestOption(long StayId, string GuestName)
{
    public string DisplayName => $"{GuestName} — #{StayId}";
}

public sealed record CustodyItemRow(
    long Id,
    long StayId,
    string GuestName,
    string ItemDescription,
    int Quantity,
    string? StorageReference,
    string? Notes,
    DateTime ReceivedAt,
    DateTime? ReturnedAt)
{
    public bool IsOutstanding => ReturnedAt is null;
    public string StatusText => IsOutstanding ? "في الأمانات" : "تم التسليم";
    public string ReceivedText => ReceivedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    public string ReturnedText => ReturnedAt?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "—";
}

public sealed class CustodyOverview
{
    public List<CustodyGuestOption> Guests { get; init; } = new();
    public List<CustodyItemRow> Items { get; init; } = new();
    public int OutstandingCount => Items.Count(item => item.IsOutstanding);
}

public sealed class CustodyReturnedPage
{
    public IReadOnlyList<CustodyItemRow> Items { get; init; } = Array.Empty<CustodyItemRow>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));
}
