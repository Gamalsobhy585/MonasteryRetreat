using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MonasteryRetreat.Infrastructure.Persistence;

#nullable disable

namespace MonasteryRetreat.Infrastructure.Persistence.Migrations;

[DbContext(typeof(MonasteryDbContext))]
[Migration("20261121210000_AddDailyMealMenus")]
public sealed class AddDailyMealMenus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "daily_meal_menus",
            columns: table => new
            {
                id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                menu_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                breakfast = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                lunch = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                dinner = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_daily_meal_menus", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_daily_meal_menus_menu_date",
            table: "daily_meal_menus",
            column: "menu_date",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "daily_meal_menus");
    }
}
