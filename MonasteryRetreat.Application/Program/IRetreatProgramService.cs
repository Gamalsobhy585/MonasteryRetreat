namespace MonasteryRetreat.Application.Program;

public interface IRetreatProgramService
{
    Task<IReadOnlyList<RetreatProgramRow>> ListAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(SaveRetreatProgramItem item, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
