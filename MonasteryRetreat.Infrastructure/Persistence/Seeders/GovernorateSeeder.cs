
using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Domain.Entities;

namespace MonasteryRetreat.Infrastructure.Persistence.Seeders;

public static class GovernorateSeeder
{
    public static async Task SeedAsync(
        MonasteryDbContext context,
        CancellationToken cancellationToken = default)
    {
        var names = new[]
        {
            "القاهرة",
            "الجيزة",
            "الإسكندرية",
            "الدقهلية",
            "البحر الأحمر",
            "البحيرة",
            "الفيوم",
            "الغربية",
            "الإسماعيلية",
            "المنوفية",
            "المنيا",
            "القليوبية",
            "الوادي الجديد",
            "السويس",
            "أسوان",
            "أسيوط",
            "بني سويف",
            "بورسعيد",
            "دمياط",
            "الشرقية",
            "جنوب سيناء",
            "كفر الشيخ",
            "مطروح",
            "الأقصر",
            "قنا",
            "شمال سيناء",
            "سوهاج"
        };

        var existingIds = await context.Governorates
            .Select(g => g.Id)
            .ToListAsync(cancellationToken);

        var existing = existingIds.ToHashSet();

        var missing = names
            .Select((name, index) => new Governorate
            {
                Id = index + 1,
                NameAr = name,
                IsActive = true
            })
            .Where(g => !existing.Contains(g.Id))
            .ToList();

        if (missing.Count == 0)
            return;

        await context.Governorates.AddRangeAsync(
            missing,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
