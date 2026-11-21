namespace MonasteryRetreat.Domain.Entities;

/// <summary>One speaker is assigned to each calendar day.</summary>
public sealed class SpeakerAssignment
{
    public long Id { get; set; }
    public DateOnly SpeakingDate { get; set; }
    public string SpeakerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
