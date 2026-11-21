namespace MonasteryRetreat.Application.Speakers;

public interface ISpeakerAssignmentService
{
    Task<IReadOnlyList<SpeakerAssignmentRow>> ListAsync(
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        DateOnly speakingDate,
        string speakerName,
        CancellationToken cancellationToken = default);
}
