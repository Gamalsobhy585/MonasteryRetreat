namespace MonasteryRetreat.Domain.Entities;

/// <summary>Menu descriptions for the three meals on one calendar day; no quantities.</summary>
public sealed class DailyMealMenu
{
    public long Id { get; set; }
    public DateOnly MenuDate { get; set; }
    public string Breakfast { get; set; } = string.Empty;
    public string Lunch { get; set; } = string.Empty;
    public string Dinner { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
