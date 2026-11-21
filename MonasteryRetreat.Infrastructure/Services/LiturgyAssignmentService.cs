using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Application.Liturgy;
using MonasteryRetreat.Domain.Entities;
using MonasteryRetreat.Infrastructure.Persistence;

namespace MonasteryRetreat.Infrastructure.Services;

/// <summary>
/// One daily liturgy assignment per active stay. StartTime=00:00 identifies
/// the daily distribution record; it is NOT a claimed liturgy start time.
/// </summary>
public sealed class LiturgyAssignmentService : ILiturgyAssignmentService
{
    private readonly MonasteryDbContext _db;

    public LiturgyAssignmentService(MonasteryDbContext db) => _db = db;

    public async Task<LiturgyOverview> GetOverviewAsync(
        DateOnly date, CancellationToken cancellationToken = default)
    {
        var churches = await _db.MonasteryChurches.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.NameAr)
            .Select(c => new ChurchOption(c.Id, c.NameAr))
            .ToListAsync(cancellationToken);

        // Only checked-in, active stays can serve; pending reservations
        // must not be displayed as confirmed attendees.
        var guests = await _db.Stays.AsNoTracking()
            .Where(s => s.Status == "active" &&
                        s.ActualDepartureAt == null &&
                        s.ExpectedDepartureDate >= date &&
                        s.Reservation != null &&
                        s.Reservation.Status == "confirmed")
            .OrderBy(s => s.Guest.FullName)
            .Select(s => new LiturgyGuestRow
            {
                StayId = s.Id,
                GuestName = s.Guest.FullName
            })
            .ToListAsync(cancellationToken);

        var assignments = await _db.LiturgyAssignments.AsNoTracking()
            .Where(a => a.LiturgySchedule.LiturgyDate == date &&
                        a.LiturgySchedule.StartTime == new TimeOnly(0, 0))
            .Select(a => new
            {
                a.StayId,
                a.LiturgySchedule.MonasteryChurchId,
                a.Role,
                a.Stay.Guest.FullName
            })
            .ToListAsync(cancellationToken);

        var validIds = guests.Select(g => g.StayId).ToHashSet();
        var validChurchIds = churches.Select(c => c.Id).ToHashSet();
        var assigned = assignments
            .Where(a => validIds.Contains(a.StayId) &&
                        validChurchIds.Contains(a.MonasteryChurchId))
            .GroupBy(a => a.StayId)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var guest in guests)
        {
            if (!assigned.TryGetValue(guest.StayId, out var assignment))
                continue;

            guest.SelectedChurchId = assignment.MonasteryChurchId;
            guest.SelectedRole = assignment.Role;
        }

        var cards = churches.Select(church => new LiturgyChurchCard
        {
            Id = church.Id,
            NameAr = church.NameAr,
            Assignments = assigned.Values
                .Where(a => a.MonasteryChurchId == church.Id)
                .OrderBy(a => a.FullName)
                .Select(a => new LiturgyChurchAssignment(
                    a.FullName,
                    LiturgyRoles.Names.GetValueOrDefault(a.Role, a.Role)))
                .ToList()
        }).ToList();

        return new LiturgyOverview
        {
            Churches = churches,
            Guests = guests,
            ChurchCards = cards
        };
    }

    public async Task SaveAsync(
        DateOnly date, long stayId, long churchId, string role,
        CancellationToken cancellationToken = default)
    {
        if (!LiturgyRoles.Names.ContainsKey(role))
            throw new ArgumentException("يرجى اختيار دور الشماس.", nameof(role));

        await using var transaction = await _db.Database
            .BeginTransactionAsync(cancellationToken);

        try
        {
            var validStay = await _db.Stays.AnyAsync(
                s => s.Id == stayId &&
                     s.Status == "active" &&
                     s.ActualDepartureAt == null &&
                     s.ExpectedDepartureDate >= date &&
                     s.Reservation != null &&
                     s.Reservation.Status == "confirmed",
                cancellationToken);

            if (!validStay)
                throw new InvalidOperationException(
                    "الضيف غير موجود ضمن الضيوف المؤكد وصولهم والمقيمين حاليًا.");

            var validChurch = await _db.MonasteryChurches.AnyAsync(
                c => c.Id == churchId && c.IsActive,
                cancellationToken);

            if (!validChurch)
                throw new InvalidOperationException("الكنيسة المختارة غير متاحة.");

            var schedules = await _db.LiturgySchedules
                .Where(s => s.LiturgyDate == date &&
                            s.StartTime == new TimeOnly(0, 0))
                .ToListAsync(cancellationToken);

            var schedule = schedules.FirstOrDefault(
                s => s.MonasteryChurchId == churchId);

            if (schedule == null)
            {
                schedule = new LiturgySchedule
                {
                    MonasteryChurchId = churchId,
                    LiturgyDate = date,
                    StartTime = new TimeOnly(0, 0),
                    Notes = "توزيع خدمة الشمامسة اليومي"
                };
                _db.LiturgySchedules.Add(schedule);
            }

            var scheduleIds = schedules.Select(s => s.Id).ToList();
            var existing = await _db.LiturgyAssignments
                .Where(a => a.StayId == stayId &&
                            scheduleIds.Contains(a.LiturgyScheduleId))
                .OrderBy(a => a.Id)
                .ToListAsync(cancellationToken);

            var assignment = existing.FirstOrDefault();

            if (assignment == null)
            {
                _db.LiturgyAssignments.Add(new LiturgyAssignment
                {
                    StayId = stayId,
                    LiturgySchedule = schedule,
                    Role = role
                });
            }
            else
            {
                assignment.LiturgySchedule = schedule;
                assignment.Role = role;
                // Clean up any duplicate rows left by earlier versions.
                if (existing.Count > 1)
                    _db.LiturgyAssignments.RemoveRange(existing.Skip(1));
            }

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            _db.ChangeTracker.Clear();
            throw;
        }
    }
}
