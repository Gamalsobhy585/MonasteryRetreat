using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MonasteryRetreat.Infrastructure.Persistence;

#nullable disable

namespace MonasteryRetreat.Infrastructure.Persistence.Migrations;

[DbContext(typeof(MonasteryDbContext))]
[Migration("20261121220000_AddGuestDateOfBirth")]
public sealed class AddGuestDateOfBirth : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Existing guests keep NULL until their birth dates become known.
        migrationBuilder.AddColumn<DateOnly>(
            name: "date_of_birth",
            table: "guests",
            type: "TEXT",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "date_of_birth",
            table: "guests");
    }
}
