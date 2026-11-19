
using Microsoft.EntityFrameworkCore;

namespace MonasteryRetreat.Infrastructure.Persistence.Seeders;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        MonasteryDbContext context,
        CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        await GovernorateSeeder.SeedAsync(context);

        await RoomSeeder.SeedAsync(
            context,
            cancellationToken);

        await MonasteryChurchSeeder.SeedAsync(
            context,
            cancellationToken);
    }
}
