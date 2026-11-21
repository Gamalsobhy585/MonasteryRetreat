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

    /// <summary>
    /// An optional editable example based on the photographed monastery schedule.
    /// Never overwrites an existing, even archived, program.
    /// </summary>
    public async Task<bool> LoadSampleAsync(
        CancellationToken cancellationToken = default)
    {
        if (await _db.RetreatProgramItems.AnyAsync(cancellationToken))
            return false;

        var sample = new (string Title, string? Note, int Hour, int Minute, int? EndHour, int? EndMinute)[]
        {
            ("التسبحة والاستيقاظ", "الاستيقاظ الساعة ٦:١٥ صباحًا. راجع مواعيد الأحد مع المسؤول.", 6, 15, null, null),
            ("القداس", "يوميًا بحسب مواعيد القداسات، مع مراعاة برنامج السبت.", 7, 0, null, null),
            ("الفطار", null, 9, 0, 9, 30),
            ("فترة العمل", "تبدأ من الساعة العاشرة صباحًا.", 10, 0, null, null),
            ("الغداء", "الساعة ١:٣٠ ظهرًا، مع اختلاف الأربعاء والجمعة وأيام الصيام.", 13, 30, null, null),
            ("راحة", null, 14, 0, 16, 0),
            ("صلاة الغروب والنوم", null, 16, 0, null, null),
            ("تمشية (خلوة فردية)", null, 16, 30, 18, 0),
            ("صلاة نصف الليل", "السبت له ترتيباته الخاصة.", 18, 15, null, null),
            ("الكلمة الروحية", "أو حسب ميعاد الأب المتكلم.", 19, 0, null, null),
            ("العشاء", "بعد الكلمة الروحية مباشرة.", 19, 30, null, null),
            ("غلق باب الخلوة", "يوميًا الساعة العاشرة مساءً.", 22, 0, null, null)
        };

        var order = 10;
        foreach (var row in sample)
        {
            _db.RetreatProgramItems.Add(new RetreatProgramItem
            {
                TitleAr = row.Title,
                DescriptionAr = row.Note,
                StartTime = new TimeOnly(row.Hour, row.Minute),
                EndTime = row.EndHour.HasValue
                    ? new TimeOnly(row.EndHour.Value, row.EndMinute!.Value)
                    : null,
                DisplayOrder = order,
                IsActive = true
            });
            order += 10;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            _db.ChangeTracker.Clear();
            throw;
        }
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
