
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
            Phone = input.Phone.Trim(),
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
}
