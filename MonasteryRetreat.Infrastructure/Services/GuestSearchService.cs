using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Application.Guests;
using MonasteryRetreat.Infrastructure.Persistence;

namespace MonasteryRetreat.Infrastructure.Services;

public sealed class GuestSearchService : IGuestSearchService
{
    private const int MaxResults = 30;

    private readonly MonasteryDbContext _context;

    public GuestSearchService(MonasteryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GuestSummary>> SearchAsync(
        string term,
        CancellationToken cancellationToken = default)
    {
        term = (term ?? string.Empty).Trim();

        if (term.Length < 2)
            return Array.Empty<GuestSummary>();

        var guests = await _context.Guests
            .AsNoTracking()
            .Where(g => g.FullName.Contains(term) ||
                        g.Phone.Contains(term))
            .OrderBy(g => g.FullName)
            .Take(MaxResults)
            .Select(g => new
            {
                g.Id,
                g.FullName,
                g.Phone,
                GovernorateName = g.Governorate != null
                    ? g.Governorate.NameAr
                    : string.Empty,
                g.GuestChurch,
                ReservationCount = g.Reservations.Count,
                LastArrival = g.Reservations
                    .OrderByDescending(r => r.ExpectedArrivalDate)
                    .Select(r => (DateOnly?)r.ExpectedArrivalDate)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return guests
            .Select(g => new GuestSummary
            {
                Id = g.Id,
                FullName = g.FullName,
                Phone = g.Phone,
                GovernorateName = g.GovernorateName,
                GuestChurch = g.GuestChurch ?? string.Empty,
                ReservationCount = g.ReservationCount,
                LastArrival = g.LastArrival
            })
            .ToList();
    }
}
