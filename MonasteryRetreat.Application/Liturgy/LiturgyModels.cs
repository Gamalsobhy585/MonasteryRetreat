namespace MonasteryRetreat.Application.Liturgy;

public sealed record ChurchOption(long Id, string NameAr);

public sealed class LiturgyGuestRow
{
    public long StayId { get; init; }
    public string GuestName { get; init; } = string.Empty;
    public long? SelectedChurchId { get; set; }
    public string? SelectedRole { get; set; }
}

public sealed record LiturgyChurchAssignment(string GuestName, string RoleName);

public sealed class LiturgyChurchCard
{
    public long Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public List<LiturgyChurchAssignment> Assignments { get; init; } = new();
    public int GuestCount => Assignments.Count;
}

public sealed class LiturgyOverview
{
    public List<ChurchOption> Churches { get; init; } = new();
    public List<LiturgyGuestRow> Guests { get; init; } = new();
    public List<LiturgyChurchCard> ChurchCards { get; init; } = new();
}

public static class LiturgyRoles
{
    public const string Mardat = "mardat";
    public const string Altar = "altar";
    public const string Congregation = "congregation";

    public static readonly IReadOnlyDictionary<string, string> Names =
        new Dictionary<string, string>
        {
            [Mardat] = "شماس مردات",
            [Altar] = "شماس هيكل",
            [Congregation] = " شعب"
        };
}

public sealed record LiturgyRoleOption(string Value, string NameAr);
