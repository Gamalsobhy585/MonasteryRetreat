namespace MonasteryRetreat.Application.Program;

public sealed record RetreatProgramRow(
    long Id, string TitleAr, string? DescriptionAr,
    TimeOnly StartTime, TimeOnly? EndTime, int DisplayOrder)
{
    public string TimeText => EndTime.HasValue
        ? $"{StartTime:HH:mm} – {EndTime:HH:mm}"
        : $"{StartTime:HH:mm}";
}

public sealed record SaveRetreatProgramItem(
    long? Id, string TitleAr, string? DescriptionAr,
    TimeOnly StartTime, TimeOnly? EndTime, int DisplayOrder);
