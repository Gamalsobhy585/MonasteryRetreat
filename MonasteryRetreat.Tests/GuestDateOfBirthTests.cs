using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Domain.Entities;
using MonasteryRetreat.Infrastructure.Persistence;

namespace MonasteryRetreat.Tests;

public sealed class GuestDateOfBirthTests
{
    [Fact]
    public async Task BirthDatePersistsAndOlderGuestsCanHaveNull()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<MonasteryDbContext>()
            .UseSqlite(connection)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using (var context = new MonasteryDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
            context.Guests.AddRange(
                new Guest { FullName = "ضيف جديد", Phone = "01012345678", DateOfBirth = new DateOnly(1999, 4, 15) },
                new Guest { FullName = "ضيف سابق", Phone = "01012345679" });
            await context.SaveChangesAsync();
        }

        await using (var context = new MonasteryDbContext(options))
        {
            var guests = await context.Guests.OrderBy(x => x.FullName).ToListAsync();
            Assert.Contains(guests, x => x.DateOfBirth == new DateOnly(1999, 4, 15));
            Assert.Contains(guests, x => x.DateOfBirth == null);
        }
    }
}
