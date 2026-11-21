using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Application.Meals;
using MonasteryRetreat.Domain.Entities;
using MonasteryRetreat.Infrastructure.Persistence;

namespace MonasteryRetreat.Infrastructure.Services;

public sealed class DailyMealMenuService : IDailyMealMenuService
{
    private readonly MonasteryDbContext _context;

    public DailyMealMenuService(MonasteryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DailyMealMenuRow>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.DailyMealMenus.AsNoTracking()
            .OrderBy(x => x.MenuDate)
            .Select(x => new DailyMealMenuRow
            {
                Id = x.Id,
                MenuDate = x.MenuDate,
                Breakfast = x.Breakfast,
                Lunch = x.Lunch,
                Dinner = x.Dinner
            })
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(
        DateOnly date, string? breakfast, string? lunch, string? dinner,
        CancellationToken cancellationToken = default)
    {
        breakfast = (breakfast ?? string.Empty).Trim();
        lunch = (lunch ?? string.Empty).Trim();
        dinner = (dinner ?? string.Empty).Trim();

        if (breakfast.Length > 1000 || lunch.Length > 1000 || dinner.Length > 1000)
            throw new ArgumentException("وصف أي وجبة يجب ألا يتجاوز ١٠٠٠ حرف.");

        if (breakfast.Length == 0 && lunch.Length == 0 && dinner.Length == 0)
            throw new ArgumentException("أدخل وصف وجبة واحدة على الأقل قبل الحفظ.");

        var menu = await _context.DailyMealMenus
            .SingleOrDefaultAsync(x => x.MenuDate == date, cancellationToken);

        var now = DateTime.UtcNow;
        if (menu is null)
        {
            _context.DailyMealMenus.Add(new DailyMealMenu
            {
                MenuDate = date,
                Breakfast = breakfast,
                Lunch = lunch,
                Dinner = dinner,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            menu.Breakfast = breakfast;
            menu.Lunch = lunch;
            menu.Dinner = dinner;
            menu.UpdatedAt = now;
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
