using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Infrastructure.Persistence;
using MonasteryRetreat.Infrastructure.Services;

namespace MonasteryRetreat.Tests;

public sealed class SpeakerAssignmentServiceTests
{
    [Fact]
    public async Task SavingSameDateTwiceUpdatesExistingRow()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<MonasteryDbContext>()
            .UseSqlite(connection)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var context = new MonasteryDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var service = new SpeakerAssignmentService(context);
        var date = new DateOnly(2026, 11, 22);

        await service.SaveAsync(date, "أبونا يوحنا");
        await service.SaveAsync(date, "  أبونا مرقس  ");

        var items = await service.ListAsync();

        Assert.Single(items);
        Assert.Equal(date, items[0].SpeakingDate);
        Assert.Equal("أبونا مرقس", items[0].SpeakerName);
        Assert.Equal(1, await context.SpeakerAssignments.CountAsync());
    }

    [Fact]
    public async Task EmptySpeakerNameIsRejected()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<MonasteryDbContext>()
            .UseSqlite(connection)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var context = new MonasteryDbContext(options);
        var service = new SpeakerAssignmentService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SaveAsync(new DateOnly(2026, 11, 22), "   "));
    }
}
