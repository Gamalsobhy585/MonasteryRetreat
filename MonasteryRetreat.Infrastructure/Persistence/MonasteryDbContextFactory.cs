
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MonasteryRetreat.Infrastructure.Persistence;

public class MonasteryDbContextFactory
    : IDesignTimeDbContextFactory<MonasteryDbContext>
{
    public MonasteryDbContext CreateDbContext(string[] args)
    {
        var databasePath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData),
            "MonasteryRetreat",
            "Data",
            "monastery.db"
        );

        Directory.CreateDirectory(
            Path.GetDirectoryName(databasePath)!);

        var options = new DbContextOptionsBuilder<MonasteryDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .UseSnakeCaseNamingConvention()
            .Options;

        return new MonasteryDbContext(options);
    }
}
