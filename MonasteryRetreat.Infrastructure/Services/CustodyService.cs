using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Application.Custody;
using MonasteryRetreat.Domain.Entities;
using MonasteryRetreat.Infrastructure.Persistence;

namespace MonasteryRetreat.Infrastructure.Services;

public sealed class CustodyService : ICustodyService
{
    private readonly MonasteryDbContext _context;

    public CustodyService(MonasteryDbContext context) => _context = context;

    public async Task<CustodyOverview> GetOverviewAsync(
        CancellationToken cancellationToken = default)
    {
        // A confirmed arrival is represented by an active Stay.
        var guests = await _context.Stays.AsNoTracking()
            .Where(s => s.Status == "active" &&
                        s.ActualDepartureAt == null &&
                        s.Reservation != null &&
                        s.Reservation.Status == "confirmed")
            .OrderBy(s => s.Guest.FullName)
            .Select(s => new CustodyGuestOption(s.Id, s.Guest.FullName))
            .ToListAsync(cancellationToken);

        var activeStayIds = guests.Select(g => g.StayId).ToList();
        var items = await _context.CustodyItems.AsNoTracking()
            .Where(i => activeStayIds.Contains(i.StayId))
            .OrderByDescending(i => i.ReceivedAt)
            .ThenByDescending(i => i.Id)
            .Select(i => new CustodyItemRow(
                i.Id,
                i.StayId,
                i.Stay.Guest.FullName,
                i.ItemDescription,
                i.Quantity,
                i.StorageReference,
                i.Notes,
                i.ReceivedAt,
                i.ReturnedAt))
            .ToListAsync(cancellationToken);

        return new CustodyOverview { Guests = guests, Items = items };
    }

    public async Task AddAsync(
        long stayId,
        string description,
        int quantity,
        string? storageReference,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        description = description?.Trim() ?? string.Empty;
        storageReference = storageReference?.Trim();
        notes = notes?.Trim();

        if (description.Length == 0 || description.Length > 300)
            throw new ArgumentException(
                "يرجى إدخال وصف للمتعلقات بحد أقصى ٣٠٠ حرف.");
        if (quantity < 1 || quantity > 100000)
            throw new ArgumentException("الكمية يجب أن تكون بين ١ و١٠٠٠٠٠.");
        if (storageReference?.Length > 120 || notes?.Length > 1000)
            throw new ArgumentException("رقم الحفظ أو الملاحظات أطول من المسموح.");

        var isConfirmedGuest = await _context.Stays.AnyAsync(
            s => s.Id == stayId &&
                 s.Status == "active" &&
                 s.ActualDepartureAt == null &&
                 s.Reservation != null &&
                 s.Reservation.Status == "confirmed",
            cancellationToken);

        if (!isConfirmedGuest)
            throw new InvalidOperationException(
                "يمكن تسجيل المتعلقات فقط للضيوف المؤكد وصولهم والمقيمين حاليًا.");

        _context.CustodyItems.Add(new CustodyItem
        {
            StayId = stayId,
            ItemDescription = description,
            Quantity = quantity,
            StorageReference = string.IsNullOrEmpty(storageReference) ? null : storageReference,
            Notes = string.IsNullOrEmpty(notes) ? null : notes,
            ReceivedAt = DateTime.UtcNow
        });

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

    public async Task ReturnAsync(
        long itemId,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.CustodyItems
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);

        if (item is null)
            throw new InvalidOperationException("هذا المتعلق غير موجود.");

        if (item.ReturnedAt is not null)
            throw new InvalidOperationException("تم تسليم هذا المتعلق بالفعل.");

        var isActive = await _context.Stays.AnyAsync(
            s => s.Id == item.StayId &&
                 s.Status == "active" &&
                 s.ActualDepartureAt == null,
            cancellationToken);

        if (!isActive)
            throw new InvalidOperationException("إقامة الضيف منتهية.");

        item.ReturnedAt = DateTime.UtcNow;

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
