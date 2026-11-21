using System.Globalization;

namespace MonasteryRetreat.Application.Meals;

public sealed class DailyMealMenuRow
{
    public long Id { get; init; }
    public DateOnly MenuDate { get; init; }
    public string Breakfast { get; init; } = string.Empty;
    public string Lunch { get; init; } = string.Empty;
    public string Dinner { get; init; } = string.Empty;

    public string DayName => MenuDate.ToDateTime(TimeOnly.MinValue)
        .ToString("dddd", CultureInfo.GetCultureInfo("ar-EG"));

    public string DateText => MenuDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
}
