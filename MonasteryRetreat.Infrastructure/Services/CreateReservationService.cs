
using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Application.Reservations;
using MonasteryRetreat.Domain.Entities;
using MonasteryRetreat.Infrastructure.Persistence;

namespace MonasteryRetreat.Infrastructure.Services;

public sealed class CreateReservationService
    : ICreateReservationService
{
    private readonly MonasteryDbContext _context;

    public CreateReservationService(
        MonasteryDbContext context)
    {
        _context = context;
    }

    public async Task<long> CreateAsync(
        NewReservationInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input.FullName) ||
            string.IsNullOrWhiteSpace(input.Phone) ||
            string.IsNullOrWhiteSpace(input.GuestChurch))
        {
            throw new ArgumentException(
                "Guest information is incomplete.");
        }

        if (input.ExpectedDeparture.Date <=
            input.ExpectedArrival.Date)
        {
            throw new ArgumentException(
                "Departure must be after arrival.");
        }

        if (input.Age < 1 || input.Age > 120)
        {
            throw new ArgumentException("Invalid guest age.");
        }

        string phone = EgyptianPhone.Normalize(input.Phone);

        if (!EgyptianPhone.IsValid(phone))
        {
            throw new ReservationValidationException(
                EgyptianPhone.ErrorMessage);
        }

        bool phoneTaken = await _context.Guests
            .AnyAsync(g => g.Phone == phone, cancellationToken);

        if (phoneTaken)
        {
            throw new ReservationValidationException(
                "رقم الهاتف مسجل بالفعل لضيف آخر. " +
                "اختر \"ضيف سابق\" للحجز له.");
        }

        bool governorateExists =
            await _context.Governorates
                .AnyAsync(
                    g => g.Id == input.GovernorateId && g.IsActive,
                    cancellationToken);

        if (!governorateExists)
        {
            throw new ArgumentException(
                "The selected governorate is invalid.");
        }

        var now = DateTime.UtcNow;

        var guest = new Guest
        {
            FullName = input.FullName.Trim(),
            Phone = phone,
            GovernorateId = input.GovernorateId,
            GuestChurch = input.GuestChurch.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        var reservation = new Reservation
        {
            Guest = guest,

            ExpectedArrivalDate =
                DateOnly.FromDateTime(input.ExpectedArrival),

            ExpectedDepartureDate =
                DateOnly.FromDateTime(input.ExpectedDeparture),

            Status = "pending",
            Source = "phone",

            Notes = string.IsNullOrWhiteSpace(input.Notes)
                ? null
                : input.Notes.Trim(),

            CreatedAt = now,
            UpdatedAt = now
        };

        // Both entities are saved in one SaveChanges transaction.
        _context.Reservations.Add(reservation);

        await _context.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }

    public async Task<long> CreateForExistingGuestAsync(
        ExistingGuestReservationInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.ExpectedDeparture.Date <=
            input.ExpectedArrival.Date)
        {
            throw new ArgumentException(
                "Departure must be after arrival.");
        }

        bool guestExists = await _context.Guests
            .AnyAsync(g => g.Id == input.GuestId, cancellationToken);

        if (!guestExists)
        {
            throw new ArgumentException(
                "The selected guest no longer exists.");
        }

        var arrival = DateOnly.FromDateTime(input.ExpectedArrival);
        var departure = DateOnly.FromDateTime(input.ExpectedDeparture);

        // Two stays overlap, even partially, when each one starts before
        // the other ends. Leaving on the day another stay starts is allowed.
        var conflict = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.GuestId == input.GuestId &&
                        r.Status != "cancelled" &&
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

        var reservation = new Reservation
        {
            GuestId = input.GuestId,

            ExpectedArrivalDate =
                DateOnly.FromDateTime(input.ExpectedArrival),

            ExpectedDepartureDate =
                DateOnly.FromDateTime(input.ExpectedDeparture),

            Status = "pending",
            Source = "phone",

            Notes = string.IsNullOrWhiteSpace(input.Notes)
                ? null
                : input.Notes.Trim(),

            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Reservations.Add(reservation);

        await _context.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}
