using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MonasteryRetreat.Infrastructure.Persistence;

#nullable disable

namespace MonasteryRetreat.Infrastructure.Persistence.Migrations;

[DbContext(typeof(MonasteryDbContext))]
[Migration("20261121190000_AddSpeakerAssignments")]
public sealed class AddSpeakerAssignments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "speaker_assignments",
            columns: table => new
            {
                id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                speaking_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                speaker_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_speaker_assignments", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_speaker_assignments_speaking_date",
            table: "speaker_assignments",
            column: "speaking_date",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "speaker_assignments");
    }
}
