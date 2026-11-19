
using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Domain.Entities;

namespace MonasteryRetreat.Infrastructure.Persistence.Seeders;

public static class RoomSeeder
{
    public static async Task SeedAsync(
        MonasteryDbContext context,
        CancellationToken cancellationToken = default)
    {
        var rooms = new[]
        {
            new { Number = "1", Name = "السيدة العذراء" },
            new { Number = "2", Name = "الأنبا أنطونيوس" },
            new { Number = "3", Name = "الأنبا مكاريوس" },
            new { Number = "4", Name = "الأنبا يحنس كاما" },
            new { Number = "5", Name = "الأنبا يحنس القصير" },
            new { Number = "6", Name = "الأنبا بيشوي" },
            new { Number = "7", Name = "الأنبا موسى" }
        };

        foreach (var room in rooms)
        {
            var existingRoom = await context.Rooms
                .FirstOrDefaultAsync(
                    r => r.RoomNumber == room.Number,
                    cancellationToken);

            if (existingRoom == null)
            {
                context.Rooms.Add(new Room
                {
                    RoomNumber = room.Number,
                    NameAr = room.Name,
                    Capacity = 8,
                    IsActive = true
                });
            }
            else
            {
                existingRoom.NameAr = room.Name;
                existingRoom.Capacity = 8;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
