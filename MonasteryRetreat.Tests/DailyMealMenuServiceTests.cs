using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Infrastructure.Persistence;
using MonasteryRetreat.Infrastructure.Services;

namespace MonasteryRetreat.Tests;

public sealed class DailyMealMenuServiceTests
{
    [Fact]
    public async Task SavingSameDateTwiceUpdatesMealsWithoutDuplicatingDate()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MonasteryDbContext>()
            .UseSqlite(connection)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var context = new MonasteryDbContext(options);
        await context.Database.EnsureCreatedAsync();
        var service = new DailyMealMenuService(context);
        var day = new DateOnly(2026, 11, 22);

        await service.SaveAsync(day, "فول، جبنة", "أرز، دجاج", "زبادي");
        await service.SaveAsync(day, "فول، خبز", "أرز، خضار", "");

        var menus = await service.ListAsync();
        Assert.Single(menus);
        Assert.Equal(day, menus[0].MenuDate);
        Assert.Equal("فول، خبز", menus[0].Breakfast);
        Assert.Equal("أرز، خضار", menus[0].Lunch);
        Assert.Equal("", menus[0].Dinner);
        Assert.Equal(1, await context.DailyMealMenus.CountAsync());
    }

    [Fact]
    public async Task RejectsEmptyDayMenu()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MonasteryDbContext>()
            .UseSqlite(connection)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var context = new MonasteryDbContext(options);
        var service = new DailyMealMenuService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SaveAsync(new DateOnly(2026, 11, 22), "", " ", null));
    }
}
