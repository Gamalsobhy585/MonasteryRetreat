using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Application.Program;
using MonasteryRetreat.Domain.Entities;
using MonasteryRetreat.Infrastructure.Persistence;

namespace MonasteryRetreat.Infrastructure.Services;

public sealed class RetreatProgramService : IRetreatProgramService
{
    private readonly MonasteryDbContext _db;

    public RetreatProgramService(MonasteryDbContext db) => _db = db;

    public async Task<IReadOnlyList<RetreatProgramRow>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.RetreatProgramItems.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.StartTime).ThenBy(x => x.Id)
            .Select(x => new RetreatProgramRow(
                x.Id, x.TitleAr, x.DescriptionAr,
                x.StartTime, x.EndTime, x.DisplayOrder))
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(
        SaveRetreatProgramItem input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var title = input.TitleAr.Trim();
        var description = input.DescriptionAr?.Trim();

        if (title.Length is < 1 or > 150)
            throw new ArgumentException("اسم النشاط مطلوب وبحد أقصى ١٥٠ حرفًا.");
        if (description?.Length > 1000)
            throw new ArgumentException("التفاصيل تتجاوز ١٠٠٠ حرف.");
        if (input.EndTime is { } end && end <= input.StartTime)
            throw new ArgumentException("وقت النهاية يجب أن يكون بعد وقت البداية.");
        if (input.DisplayOrder < 0)
            throw new ArgumentException("ترتيب العرض لا يمكن أن يكون سالبًا.");

        RetreatProgramItem item;
        if (input.Id is long id)
        {
            item = await _db.RetreatProgramItems
                .SingleOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken)
                ?? throw new InvalidOperationException("النشاط غير موجود.");
        }
        else
        {
            item = new RetreatProgramItem();
            _db.RetreatProgramItems.Add(item);
        }

        item.TitleAr = title;
        item.DescriptionAr = string.IsNullOrWhiteSpace(description) ? null : description;
        item.StartTime = input.StartTime;
        item.EndTime = input.EndTime;
        item.DisplayOrder = input.DisplayOrder;
        item.IsActive = true;

        try { await _db.SaveChangesAsync(cancellationToken); }
        catch { _db.ChangeTracker.Clear(); throw; }
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await _db.RetreatProgramItems
            .SingleOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("النشاط غير موجود.");
        item.IsActive = false;
        try { await _db.SaveChangesAsync(cancellationToken); }
        catch { _db.ChangeTracker.Clear(); throw; }
    }
}
