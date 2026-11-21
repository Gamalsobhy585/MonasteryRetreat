using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonasteryRetreat.Domain.Entities;

namespace MonasteryRetreat.Infrastructure.Persistence.Configurations;

public sealed class DailyMealMenuConfiguration : IEntityTypeConfiguration<DailyMealMenu>
{
    public void Configure(EntityTypeBuilder<DailyMealMenu> builder)
    {
        builder.ToTable("daily_meal_menus");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MenuDate).IsRequired();
        builder.Property(x => x.Breakfast).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Lunch).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Dinner).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.HasIndex(x => x.MenuDate).IsUnique();
    }
}
