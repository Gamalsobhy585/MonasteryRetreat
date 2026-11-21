using System.Globalization;

namespace MonasteryRetreat.Application.Speakers;

public sealed class SpeakerAssignmentRow
{
    public long Id { get; init; }
    public DateOnly SpeakingDate { get; init; }
    public string SpeakerName { get; init; } = string.Empty;

    public string DayName => SpeakingDate
        .ToDateTime(TimeOnly.MinValue)
        .ToString("dddd", new CultureInfo("ar-EG"));

    public string DateText => SpeakingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
}
