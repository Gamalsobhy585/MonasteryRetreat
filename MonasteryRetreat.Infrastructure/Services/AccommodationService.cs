using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Application.Accommodation;
using MonasteryRetreat.Application.Reservations;
using MonasteryRetreat.Domain.Entities;
using MonasteryRetreat.Infrastructure.Persistence;

namespace MonasteryRetreat.Infrastructure.Services;

public sealed class AccommodationService : IAccommodationService
{
    private const string StayActive = "active";

    private readonly MonasteryDbContext _context;

    public AccommodationService(MonasteryDbContext context)
    {
        _context = context;
    }

    private static string RoomName(string? nameAr, string roomNumber) =>
        string.IsNullOrWhiteSpace(nameAr)
            ? $"غرفة {roomNumber}"
            : nameAr.Trim();

    // ---------------------------------------------------------------
    // Overview
    // ---------------------------------------------------------------

    public async Task<AccommodationOverview> GetOverviewAsync(
        CancellationToken cancellationToken = default)
    {
        var rooms = await _context.Rooms
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Id)
            .Select(r => new
            {
                r.Id,
                r.RoomNumber,
                r.NameAr,
                r.Capacity
            })
            .ToListAsync(cancellationToken);

        // A bed is taken by an assignment that was not released, for a
        // guest who is still staying.
        var assignments = await _context.RoomAssignments
            .AsNoTracking()
            .Where(a => a.ReleasedAt == null &&
                        a.Stay.Status == StayActive)
            .OrderBy(a => a.AssignedAt)
            .Select(a => new
            {
                a.StayId,
                a.RoomId,
                GuestName = a.Stay.Guest.FullName
            })
            .ToListAsync(cancellationToken);

        var stays = await _context.Stays
            .AsNoTracking()
            .Where(s => s.Status == StayActive)
            .OrderBy(s => s.ActualArrivalAt)
            .Select(s => new
            {
                s.Id,
                GuestName = s.Guest.FullName,
                s.Guest.Phone,
                s.ActualArrivalAt,
                s.ExpectedDepartureDate
            })
            .ToListAsync(cancellationToken);

        var occupancies = rooms.Select(r => new RoomOccupancy
        {
            RoomId = r.Id,
            Name = RoomName(r.NameAr, r.RoomNumber),
            RoomLabel = $"غرفة {r.RoomNumber}",
            Capacity = r.Capacity,
            Occupants = assignments
                .Where(a => a.RoomId == r.Id)
                .Select(a => a.GuestName)
                .ToList()
        }).ToList();

        RoomOption ToOption(RoomOccupancy room) => new()
        {
            Id = room.RoomId,
            DisplayName = $"{room.Name}  ({room.CountText})"
        };

        var roomsWithSpace = occupancies
            .Where(r => !r.IsFull)
            .ToList();

        var housedStayIds = assignments
            .Select(a => a.StayId)
            .ToHashSet();

        var unassigned = stays
            .Where(s => !housedStayIds.Contains(s.Id))
            .Select(s => new UnassignedGuestRow
            {
                StayId = s.Id,
                GuestName = s.GuestName,
                Phone = s.Phone,
                ArrivalText = s.ActualArrivalAt
                    .ToLocalTime()
                    .ToString("dd/MM/yyyy"),
                DepartureText = s.ExpectedDepartureDate
                    .ToString("dd/MM/yyyy"),
                AvailableRooms = roomsWithSpace
                    .Select(ToOption)
                    .ToList()
            })
            .ToList();

        var assigned = assignments
            .Select(a =>
            {
                var current = occupancies.First(r => r.RoomId == a.RoomId);

                return new AssignedGuestRow
                {
                    StayId = a.StayId,
                    GuestName = a.GuestName,
                    Phone = stays.First(s => s.Id == a.StayId).Phone,
                    CurrentRoomName = current.Name,
                    AvailableRooms = roomsWithSpace
                        .Where(r => r.RoomId != a.RoomId)
                        .Select(ToOption)
                        .ToList()
                };
            })
            .OrderBy(a => a.CurrentRoomName)
            .ThenBy(a => a.GuestName)
            .ToList();

        return new AccommodationOverview
        {
            Rooms = occupancies,
            Unassigned = unassigned,
            Assigned = assigned
        };
    }

    // ---------------------------------------------------------------
    // Actions
    // ---------------------------------------------------------------

    private async Task EnsureStayActiveAsync(
        long stayId,
        CancellationToken cancellationToken)
    {
        bool active = await _context.Stays.AnyAsync(
            s => s.Id == stayId && s.Status == StayActive,
            cancellationToken);

        if (!active)
        {
            throw new ReservationValidationException(
                "هذا الضيف ليس مقيمًا حاليًا.");
        }
    }

    private async Task EnsureRoomHasSpaceAsync(
        long roomId,
        CancellationToken cancellationToken)
    {
        var room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.Id == roomId && r.IsActive,
                cancellationToken);

        if (room == null)
        {
            throw new ReservationValidationException(
                "الغرفة المختارة غير موجودة.");
        }

        int used = await _context.RoomAssignments.CountAsync(
            a => a.RoomId == roomId &&
                 a.ReleasedAt == null &&
                 a.Stay.Status == StayActive,
            cancellationToken);

        if (used >= room.Capacity)
        {
            throw new ReservationValidationException(
                $"غرفة {RoomName(room.NameAr, room.RoomNumber)} " +
                "ممتلئة. اختر غرفة أخرى.");
        }
    }

    public async Task AssignAsync(
        long stayId,
        long roomId,
        CancellationToken cancellationToken = default)
    {
        await EnsureStayActiveAsync(stayId, cancellationToken);

        bool alreadyHoused = await _context.RoomAssignments.AnyAsync(
            a => a.StayId == stayId && a.ReleasedAt == null,
            cancellationToken);

        if (alreadyHoused)
        {
            throw new ReservationValidationException(
                "هذا الضيف مسكّن بالفعل. استخدم تبويب نقل الضيوف.");
        }

        await EnsureRoomHasSpaceAsync(roomId, cancellationToken);

        _context.RoomAssignments.Add(new RoomAssignment
        {
            StayId = stayId,
            RoomId = roomId,
            AssignedAt = DateTime.UtcNow
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

    public async Task MoveAsync(
        long stayId,
        long toRoomId,
        CancellationToken cancellationToken = default)
    {
        await EnsureStayActiveAsync(stayId, cancellationToken);

        var current = await _context.RoomAssignments
            .FirstOrDefaultAsync(
                a => a.StayId == stayId && a.ReleasedAt == null,
                cancellationToken);

        if (current == null)
        {
            throw new ReservationValidationException(
                "هذا الضيف ليس له غرفة بعد. استخدم تبويب التسكين.");
        }

        if (current.RoomId == toRoomId)
        {
            throw new ReservationValidationException(
                "الضيف موجود بالفعل في هذه الغرفة.");
        }

        await EnsureRoomHasSpaceAsync(toRoomId, cancellationToken);

        // A stay may only have one open assignment, so the old one is
        // closed first; both steps commit together or not at all.
        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var now = DateTime.UtcNow;

            current.ReleasedAt = now;
            await _context.SaveChangesAsync(cancellationToken);

            _context.RoomAssignments.Add(new RoomAssignment
            {
                StayId = stayId,
                RoomId = toRoomId,
                AssignedAt = now
            });
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            _context.ChangeTracker.Clear();
            throw;
        }
    }
}
