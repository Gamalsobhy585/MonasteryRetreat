using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Application.Reservations;
using MonasteryRetreat.Domain.Entities;
using MonasteryRetreat.Infrastructure.Persistence;

namespace MonasteryRetreat.Infrastructure.Services;

public sealed class ReservationManagementService
    : IReservationManagementService
{
    private const string Pending = "pending";
    private const string Confirmed = "confirmed";
    private const string Completed = "completed";
    private const string Cancelled = "cancelled";
    private const string StayActive = "active";

    private readonly MonasteryDbContext _context;

    public ReservationManagementService(MonasteryDbContext context)
    {
        _context = context;
    }

    private static DateOnly Today =>
        DateOnly.FromDateTime(DateTime.Today);

    // ---------------------------------------------------------------
    // Listing
    // ---------------------------------------------------------------

    // Every reservation falls in exactly one of
    // Present / Upcoming / Past / Cancelled.

    private static Expression<Func<Reservation, bool>> IsPresent(
        DateOnly today) =>
        r => r.Status != Cancelled &&
             ((r.Stay != null && r.Stay.Status == StayActive) ||
              (r.Stay == null &&
               r.ExpectedArrivalDate <= today &&
               r.ExpectedDepartureDate >= today));

    private static Expression<Func<Reservation, bool>> IsUpcoming(
        DateOnly today) =>
        r => r.Status != Cancelled &&
             r.Stay == null &&
             r.ExpectedArrivalDate > today;

    private static Expression<Func<Reservation, bool>> IsPast(
        DateOnly today) =>
        r => r.Status != Cancelled &&
             ((r.Stay != null && r.Stay.Status != StayActive) ||
              (r.Stay == null && r.ExpectedDepartureDate < today));

    public async Task<ReservationListResult> ListAsync(
        ReservationQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var today = Today;

        IQueryable<Reservation> reservations =
            _context.Reservations.AsNoTracking();

        switch (query.Period)
        {
            case ReservationPeriod.Present:
                reservations = reservations.Where(IsPresent(today));
                break;
            case ReservationPeriod.Upcoming:
                reservations = reservations.Where(IsUpcoming(today));
                break;
            case ReservationPeriod.Past:
                reservations = reservations.Where(IsPast(today));
                break;
            case ReservationPeriod.Cancelled:
                reservations = reservations.Where(
                    r => r.Status == Cancelled);
                break;
        }

        if (query.From is DateOnly from)
        {
            reservations = reservations.Where(
                r => r.ExpectedDepartureDate >= from);
        }

        if (query.To is DateOnly to)
        {
            reservations = reservations.Where(
                r => r.ExpectedArrivalDate <= to);
        }

        string search = (query.Search ?? string.Empty).Trim();

        if (search.Length > 0)
        {
            reservations = reservations.Where(
                r => r.Guest.FullName.Contains(search) ||
                     r.Guest.Phone.Contains(search));
        }

        // Past and "all" read newest first; present and upcoming read
        // in the order guests arrive.
        bool newestFirst =
            query.Period == ReservationPeriod.Past ||
            query.Period == ReservationPeriod.Cancelled ||
            query.Period == ReservationPeriod.All;

        var ordered = newestFirst
            ? reservations.OrderByDescending(r => r.ExpectedArrivalDate)
            : reservations.OrderBy(r => r.ExpectedArrivalDate);

        var rows = await ordered
            .ThenBy(r => r.Guest.FullName)
            .Select(r => new
            {
                r.Id,
                r.GuestId,
                r.Guest.FullName,
                r.Guest.Phone,
                r.ExpectedArrivalDate,
                r.ExpectedDepartureDate,
                r.Status,
                StayStatus = r.Stay != null ? r.Stay.Status : null,
                r.Notes
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(r =>
        {
            ReservationState state;

            if (r.Status == Cancelled)
                state = ReservationState.Cancelled;
            else if (r.StayStatus == null)
                state = ReservationState.Pending;
            else if (r.StayStatus == StayActive)
                state = ReservationState.InHouse;
            else
                state = ReservationState.Ended;

            return new ReservationListItem
            {
                ReservationId = r.Id,
                GuestId = r.GuestId,
                GuestName = r.FullName,
                Phone = r.Phone,
                ExpectedArrival = r.ExpectedArrivalDate,
                ExpectedDeparture = r.ExpectedDepartureDate,
                State = state,
                Notes = r.Notes,
                CanConfirm =
                    state == ReservationState.Pending &&
                    r.ExpectedArrivalDate <= today
            };
        }).ToList();

        // The two cards ignore the filters: they always describe today.
        int inHouse = await _context.Stays
            .CountAsync(s => s.Status == StayActive, cancellationToken);

        int pendingToday = await _context.Reservations
            .CountAsync(
                r => r.Status == Pending &&
                     r.Stay == null &&
                     r.ExpectedArrivalDate <= today &&
                     r.ExpectedDepartureDate >= today,
                cancellationToken);

        return new ReservationListResult
        {
            Items = items,
            InHouseCount = inHouse,
            PendingTodayCount = pendingToday
        };
    }

    // ---------------------------------------------------------------
    // Actions
    // ---------------------------------------------------------------

    private async Task<Reservation> LoadAsync(
        long reservationId,
        CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Guest)
            .Include(r => r.Stay)
            .FirstOrDefaultAsync(
                r => r.Id == reservationId,
                cancellationToken);

        return reservation ?? throw new ReservationValidationException(
            "الحجز غير موجود أو تم حذفه.");
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Do not leave half-applied changes in the shared context.
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task ConfirmArrivalAsync(
        long reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservation = await LoadAsync(reservationId, cancellationToken);

        if (reservation.Status != Pending || reservation.Stay != null)
        {
            throw new ReservationValidationException(
                "هذا الحجز تم تأكيده أو إلغاؤه من قبل.");
        }

        if (reservation.ExpectedArrivalDate > Today)
        {
            throw new ReservationValidationException(
                "لم يحن موعد وصول هذا الحجز بعد. " +
                "عدّل تاريخ الوصول أولًا إذا وصل الضيف مبكرًا.");
        }

        bool alreadyStaying = await _context.Stays.AnyAsync(
            s => s.GuestId == reservation.GuestId &&
                 s.Status == StayActive,
            cancellationToken);

        if (alreadyStaying)
        {
            throw new ReservationValidationException(
                "هذا الضيف مقيم حاليًا في الدير. " +
                "أنهِ إقامته الحالية أولًا.");
        }

        var now = DateTime.UtcNow;

        reservation.Stay = new Stay
        {
            GuestId = reservation.GuestId,
            NationalIdAtCheckin = reservation.Guest.NationalId ?? string.Empty,
            ActualArrivalAt = now,
            ExpectedDepartureDate = reservation.ExpectedDepartureDate,
            Status = StayActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        reservation.Status = Confirmed;
        reservation.UpdatedAt = now;

        await SaveAsync(cancellationToken);
    }

    public async Task CancelAsync(
        long reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservation = await LoadAsync(reservationId, cancellationToken);

        if (reservation.Status != Pending || reservation.Stay != null)
        {
            throw new ReservationValidationException(
                "لا يمكن إلغاء حجز وصل ضيفه أو أُلغي من قبل. " +
                "استخدم \"إنهاء الإقامة\" للضيف المقيم.");
        }

        reservation.Status = Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;

        await SaveAsync(cancellationToken);
    }

    public async Task EndStayAsync(
        long reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservation = await LoadAsync(reservationId, cancellationToken);

        var stay = reservation.Stay;

        if (stay == null || stay.Status != StayActive)
        {
            throw new ReservationValidationException(
                "هذا الضيف ليس مقيمًا حاليًا.");
        }

        bool custodyOutstanding = await _context.CustodyItems.AnyAsync(
            c => c.StayId == stay.Id && c.ReturnedAt == null,
            cancellationToken);

        if (custodyOutstanding)
        {
            throw new ReservationValidationException(
                "لدى الضيف أمانات لم تُسترد بعد. " +
                "يجب استردادها قبل إنهاء الإقامة.");
        }

        var now = DateTime.UtcNow;

        stay.Status = Completed;
        stay.ActualDepartureAt = now;
        stay.UpdatedAt = now;

        reservation.Status = Completed;
        reservation.UpdatedAt = now;

        // Free the guest's bed.
        var openAssignments = await _context.RoomAssignments
            .Where(a => a.StayId == stay.Id && a.ReleasedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var assignment in openAssignments)
            assignment.ReleasedAt = now;

        _context.CheckoutRecords.Add(new CheckoutRecord
        {
            StayId = stay.Id,
            DepartureType = "normal",
            DepartureAt = now,
            CustodyCleared = true
        });

        await SaveAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        long reservationId,
        DateTime expectedArrival,
        DateTime expectedDeparture,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var reservation = await LoadAsync(reservationId, cancellationToken);

        bool inHouse =
            reservation.Stay != null && reservation.Stay.Status == StayActive;

        if (reservation.Status == Cancelled ||
            (reservation.Stay != null && !inHouse))
        {
            throw new ReservationValidationException(
                "لا يمكن تعديل حجز ملغي أو انتهت إقامته.");
        }

        var arrival = DateOnly.FromDateTime(expectedArrival);
        var departure = DateOnly.FromDateTime(expectedDeparture);

        if (inHouse && arrival != reservation.ExpectedArrivalDate)
        {
            throw new ReservationValidationException(
                "لا يمكن تغيير تاريخ الوصول بعد وصول الضيف.");
        }

        if (departure <= arrival)
        {
            throw new ReservationValidationException(
                "تاريخ المغادرة يجب أن يكون بعد تاريخ الوصول.");
        }

        var conflict = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.GuestId == reservation.GuestId &&
                        r.Id != reservation.Id &&
                        r.Status != Cancelled &&
                        r.ExpectedArrivalDate < departure &&
                        r.ExpectedDepartureDate > arrival)
            .OrderBy(r => r.ExpectedArrivalDate)
            .Select(r => new
            {
                r.ExpectedArrivalDate,
                r.ExpectedDepartureDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (conflict != null)
        {
            throw new ReservationValidationException(
                "لدى هذا الضيف حجز آخر يتداخل مع نفس الفترة " +
                $"(من {conflict.ExpectedArrivalDate:dd/MM/yyyy} " +
                $"إلى {conflict.ExpectedDepartureDate:dd/MM/yyyy}).");
        }

        var now = DateTime.UtcNow;

        reservation.ExpectedArrivalDate = arrival;
        reservation.ExpectedDepartureDate = departure;

        reservation.Notes = string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();

        reservation.UpdatedAt = now;

        if (reservation.Stay != null)
        {
            reservation.Stay.ExpectedDepartureDate = departure;
            reservation.Stay.UpdatedAt = now;
        }

        await SaveAsync(cancellationToken);
    }
}
