
using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Domain.Entities;

namespace MonasteryRetreat.Infrastructure.Persistence.Seeders;

public static class MonasteryChurchSeeder
{
    public static async Task SeedAsync(
        MonasteryDbContext context,
        CancellationToken cancellationToken = default)
    {
        var churches = new[]
        {
            "كنيسة الانبا يحنس",
            "كنيسة الانبا كاراس",
            "كنيسة ابو فام",
            "كنيسة الاربعين",
            "كنيسة البابا كيرلس",
            "كنيسة الثلاثة فتية",
            "كنيسة الانبا بيشوي",
            "كنيسة المغارة",
            "الكنيسة الاثرية",
            "كنيسة ابو مقار",
            "كنيسة الانبا بولا"
        };

        foreach (var churchName in churches)
        {
            var existingChurch = await context.MonasteryChurches
                .FirstOrDefaultAsync(
                    c => c.NameAr == churchName,
                    cancellationToken);

            if (existingChurch == null)
            {
                context.MonasteryChurches.Add(new MonasteryChurch
                {
                    NameAr = churchName,
                    IsActive = true,
                    Notes = null
                });
            }
            else
            {
                existingChurch.IsActive = true;
                existingChurch.Notes = null;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
