using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Application.Speakers;
using MonasteryRetreat.Domain.Entities;
using MonasteryRetreat.Infrastructure.Persistence;

namespace MonasteryRetreat.Infrastructure.Services;

public sealed class SpeakerAssignmentService : ISpeakerAssignmentService
{
    private readonly MonasteryDbContext _context;

    public SpeakerAssignmentService(MonasteryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SpeakerAssignmentRow>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SpeakerAssignments.AsNoTracking()
            .OrderBy(x => x.SpeakingDate)
            .Select(x => new SpeakerAssignmentRow
            {
                Id = x.Id,
                SpeakingDate = x.SpeakingDate,
                SpeakerName = x.SpeakerName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(
        DateOnly speakingDate,
        string speakerName,
        CancellationToken cancellationToken = default)
    {
        speakerName = (speakerName ?? string.Empty).Trim();
        if (speakerName.Length == 0 || speakerName.Length > 200)
            throw new ArgumentException("يرجى إدخال اسم المتكلم (بحد أقصى ٢٠٠ حرف).");

        // The unique index protects the one-speaker-per-day invariant.
        var assignment = await _context.SpeakerAssignments
            .SingleOrDefaultAsync(x => x.SpeakingDate == speakingDate, cancellationToken);

        var now = DateTime.UtcNow;
        if (assignment is null)
        {
            _context.SpeakerAssignments.Add(new SpeakerAssignment
            {
                SpeakingDate = speakingDate,
                SpeakerName = speakerName,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            assignment.SpeakerName = speakerName;
            assignment.UpdatedAt = now;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            _context.ChangeTracker.Clear();
            throw;
        }
    }
}
