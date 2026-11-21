using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Application.Program;
using MonasteryRetreat.Infrastructure.Persistence;
using MonasteryRetreat.Infrastructure.Services;

namespace MonasteryRetreat.Tests;

public sealed class RetreatProgramServiceTests
{
    [Fact]
    public async Task CreatingEditingAndArchivingActivitiesWorks()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MonasteryDbContext>()
            .UseSqlite(connection)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var context = new MonasteryDbContext(options);
        await context.Database.EnsureCreatedAsync();
        var service = new RetreatProgramService(context);

        await service.SaveAsync(new SaveRetreatProgramItem(
            null, "الفطار", "من ٩ إلى ٩:٣٠", new TimeOnly(9, 0),
            new TimeOnly(9, 30), 30));
        var row = Assert.Single(await service.ListAsync());

        await service.SaveAsync(new SaveRetreatProgramItem(
            row.Id, "الإفطار", null, new TimeOnly(9, 0),
            new TimeOnly(9, 30), 20));
        var updated = Assert.Single(await service.ListAsync());
        Assert.Equal(row.Id, updated.Id);
        Assert.Equal("الإفطار", updated.TitleAr);
        Assert.Equal(20, updated.DisplayOrder);
        Assert.Equal(1, await context.RetreatProgramItems.CountAsync());

        await service.DeleteAsync(row.Id);
        Assert.Empty(await service.ListAsync());
        Assert.Equal(1, await context.RetreatProgramItems.CountAsync());
    }

    [Fact]
    public async Task StarterTemplateDoesNotOverwriteExistingProgram()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MonasteryDbContext>()
            .UseSqlite(connection)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var context = new MonasteryDbContext(options);
        await context.Database.EnsureCreatedAsync();
        var service = new RetreatProgramService(context);

        Assert.True(await service.LoadSampleAsync());
        var count = (await service.ListAsync()).Count;
        Assert.True(count > 0);
        Assert.False(await service.LoadSampleAsync());
        Assert.Equal(count, (await service.ListAsync()).Count);
    }

    [Fact]
    public async Task RejectsInvalidTimes()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MonasteryDbContext>()
            .UseSqlite(connection)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var context = new MonasteryDbContext(options);
        var service = new RetreatProgramService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SaveAsync(new SaveRetreatProgramItem(
                null, "غداء", null, new TimeOnly(14, 0),
                new TimeOnly(13, 0), 10)));
    }
}
