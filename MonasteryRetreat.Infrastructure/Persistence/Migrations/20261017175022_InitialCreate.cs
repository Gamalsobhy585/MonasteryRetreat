using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonasteryRetreat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_settings", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "governorates",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name_ar = table.Column<string>(type: "TEXT", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_governorates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "monastery_churches",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name_ar = table.Column<string>(type: "TEXT", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_monastery_churches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "retreat_instructions",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    instruction_ar = table.Column<string>(type: "TEXT", nullable: false),
                    display_order = table.Column<int>(type: "INTEGER", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_retreat_instructions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "retreat_program_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title_ar = table.Column<string>(type: "TEXT", nullable: false),
                    description_ar = table.Column<string>(type: "TEXT", nullable: true),
                    start_time = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    display_order = table.Column<int>(type: "INTEGER", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_retreat_program_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    room_number = table.Column<string>(type: "TEXT", nullable: false),
                    name_ar = table.Column<string>(type: "TEXT", nullable: true),
                    capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rooms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "guests",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    full_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    national_id = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    governorate_id = table.Column<long>(type: "INTEGER", nullable: true),
                    guest_church = table.Column<string>(type: "TEXT", nullable: true),
                    confession_father_name = table.Column<string>(type: "TEXT", nullable: true),
                    emergency_contact_name = table.Column<string>(type: "TEXT", nullable: true),
                    emergency_contact_phone = table.Column<string>(type: "TEXT", nullable: true),
                    notes = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guests", x => x.id);
                    table.ForeignKey(
                        name: "fk_guests_governorates_governorate_id",
                        column: x => x.governorate_id,
                        principalTable: "governorates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "liturgy_schedules",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    monastery_church_id = table.Column<long>(type: "INTEGER", nullable: false),
                    liturgy_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_liturgy_schedules", x => x.id);
                    table.ForeignKey(
                        name: "fk_liturgy_schedules_monastery_churches_monastery_church_id",
                        column: x => x.monastery_church_id,
                        principalTable: "monastery_churches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guest_id = table.Column<long>(type: "INTEGER", nullable: false),
                    expected_arrival_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    expected_departure_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    status = table.Column<string>(type: "TEXT", nullable: false),
                    source = table.Column<string>(type: "TEXT", nullable: false),
                    notes = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reservations", x => x.id);
                    table.ForeignKey(
                        name: "fk_reservations_guests_guest_id",
                        column: x => x.guest_id,
                        principalTable: "guests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stays",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guest_id = table.Column<long>(type: "INTEGER", nullable: false),
                    reservation_id = table.Column<long>(type: "INTEGER", nullable: true),
                    national_id_at_checkin = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    actual_arrival_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    expected_departure_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    actual_departure_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stays", x => x.id);
                    table.ForeignKey(
                        name: "fk_stays_guests_guest_id",
                        column: x => x.guest_id,
                        principalTable: "guests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stays_reservations_reservation_id",
                        column: x => x.reservation_id,
                        principalTable: "reservations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "checkout_records",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    stay_id = table.Column<long>(type: "INTEGER", nullable: false),
                    departure_type = table.Column<string>(type: "TEXT", nullable: false),
                    departure_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    custody_cleared = table.Column<bool>(type: "INTEGER", nullable: false),
                    custody_override_reason = table.Column<string>(type: "TEXT", nullable: true),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_checkout_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_checkout_records_stays_stay_id",
                        column: x => x.stay_id,
                        principalTable: "stays",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "custody_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    stay_id = table.Column<long>(type: "INTEGER", nullable: false),
                    item_description = table.Column<string>(type: "TEXT", nullable: false),
                    quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    storage_reference = table.Column<string>(type: "TEXT", nullable: true),
                    received_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    returned_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custody_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_custody_items_stays_stay_id",
                        column: x => x.stay_id,
                        principalTable: "stays",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "liturgy_assignments",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    liturgy_schedule_id = table.Column<long>(type: "INTEGER", nullable: false),
                    stay_id = table.Column<long>(type: "INTEGER", nullable: false),
                    role = table.Column<string>(type: "TEXT", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_liturgy_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_liturgy_assignments_liturgy_schedules_liturgy_schedule_id",
                        column: x => x.liturgy_schedule_id,
                        principalTable: "liturgy_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_liturgy_assignments_stays_stay_id",
                        column: x => x.stay_id,
                        principalTable: "stays",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "room_assignments",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    stay_id = table.Column<long>(type: "INTEGER", nullable: false),
                    room_id = table.Column<long>(type: "INTEGER", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    released_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_room_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_room_assignments_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_room_assignments_stays_stay_id",
                        column: x => x.stay_id,
                        principalTable: "stays",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stay_adjustments",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    stay_id = table.Column<long>(type: "INTEGER", nullable: false),
                    old_departure_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    new_departure_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    reason = table.Column<string>(type: "TEXT", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stay_adjustments", x => x.id);
                    table.ForeignKey(
                        name: "fk_stay_adjustments_stays_stay_id",
                        column: x => x.stay_id,
                        principalTable: "stays",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_checkout_records_stay_id",
                table: "checkout_records",
                column: "stay_id");

            migrationBuilder.CreateIndex(
                name: "ix_custody_items_stay_id",
                table: "custody_items",
                column: "stay_id");

            migrationBuilder.CreateIndex(
                name: "ix_guests_governorate_id",
                table: "guests",
                column: "governorate_id");

            migrationBuilder.CreateIndex(
                name: "ix_guests_national_id",
                table: "guests",
                column: "national_id",
                unique: true,
                filter: "national_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_liturgy_assignments_liturgy_schedule_id",
                table: "liturgy_assignments",
                column: "liturgy_schedule_id");

            migrationBuilder.CreateIndex(
                name: "ix_liturgy_assignments_stay_id",
                table: "liturgy_assignments",
                column: "stay_id");

            migrationBuilder.CreateIndex(
                name: "ix_liturgy_schedules_monastery_church_id",
                table: "liturgy_schedules",
                column: "monastery_church_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_guest_id",
                table: "reservations",
                column: "guest_id");

            migrationBuilder.CreateIndex(
                name: "ix_room_assignments_room_id",
                table: "room_assignments",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "ix_room_assignments_stay_id",
                table: "room_assignments",
                column: "stay_id",
                unique: true,
                filter: "released_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_stay_adjustments_stay_id",
                table: "stay_adjustments",
                column: "stay_id");

            migrationBuilder.CreateIndex(
                name: "ix_stays_guest_id",
                table: "stays",
                column: "guest_id",
                unique: true,
                filter: "status = 'active'");

            migrationBuilder.CreateIndex(
                name: "ix_stays_reservation_id",
                table: "stays",
                column: "reservation_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "checkout_records");

            migrationBuilder.DropTable(
                name: "custody_items");

            migrationBuilder.DropTable(
                name: "liturgy_assignments");

            migrationBuilder.DropTable(
                name: "retreat_instructions");

            migrationBuilder.DropTable(
                name: "retreat_program_items");

            migrationBuilder.DropTable(
                name: "room_assignments");

            migrationBuilder.DropTable(
                name: "stay_adjustments");

            migrationBuilder.DropTable(
                name: "liturgy_schedules");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "stays");

            migrationBuilder.DropTable(
                name: "monastery_churches");

            migrationBuilder.DropTable(
                name: "reservations");

            migrationBuilder.DropTable(
                name: "guests");

            migrationBuilder.DropTable(
                name: "governorates");
        }
    }
}
