using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTourDatesMoveToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_TourDates_TourDateId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_TourGuideAssignments_TourDates_TourDateId",
                table: "TourGuideAssignments");

            migrationBuilder.DropTable(
                name: "TourDates");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "TourDateId",
                table: "TourGuideAssignments",
                newName: "ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_TourGuideAssignments_TourDateId",
                table: "TourGuideAssignments",
                newName: "IX_TourGuideAssignments_ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_TourGuideAssignments_GuideId_TourDateId",
                table: "TourGuideAssignments",
                newName: "IX_TourGuideAssignments_GuideId_ScheduleId");

            migrationBuilder.RenameColumn(
                name: "TourDateId",
                table: "Reservations",
                newName: "ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_TourDateId",
                table: "Reservations",
                newName: "IX_Reservations_ScheduleId");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "Reservations",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "DurationUnitId",
                table: "Reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DurationValue",
                table: "Reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "Reservations",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "Reservations",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Date", "DurationUnitId", "DurationValue", "StartTime", "UnitPrice" },
                values: new object[] { new DateOnly(2026, 2, 15), 2, 1, new TimeSpan(0, 9, 0, 0, 0), 750m });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Date", "DurationUnitId", "DurationValue", "StartTime", "UnitPrice" },
                values: new object[] { new DateOnly(2026, 3, 10), 2, 3, new TimeSpan(0, 8, 0, 0, 0), 1200m });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Date", "DurationUnitId", "DurationValue", "StartTime", "UnitPrice" },
                values: new object[] { new DateOnly(2026, 4, 5), 2, 1, new TimeSpan(0, 6, 0, 0, 0), 3500m });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Date", "DurationUnitId", "DurationValue", "StartTime", "UnitPrice" },
                values: new object[] { new DateOnly(2026, 2, 20), 2, 1, new TimeSpan(0, 10, 0, 0, 0), 450m });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Date", "DurationUnitId", "DurationValue", "StartTime", "UnitPrice" },
                values: new object[] { new DateOnly(2026, 5, 1), 2, 2, new TimeSpan(0, 9, 0, 0, 0), 1100m });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Date", "DurationUnitId", "DurationValue", "StartTime", "UnitPrice" },
                values: new object[] { new DateOnly(2026, 3, 25), 2, 1, new TimeSpan(0, 14, 0, 0, 0), 350m });

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_TourSchedules_ScheduleId",
                table: "Reservations",
                column: "ScheduleId",
                principalTable: "TourSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TourGuideAssignments_TourSchedules_ScheduleId",
                table: "TourGuideAssignments",
                column: "ScheduleId",
                principalTable: "TourSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_TourSchedules_ScheduleId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_TourGuideAssignments_TourSchedules_ScheduleId",
                table: "TourGuideAssignments");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "DurationUnitId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "DurationValue",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                table: "TourGuideAssignments",
                newName: "TourDateId");

            migrationBuilder.RenameIndex(
                name: "IX_TourGuideAssignments_ScheduleId",
                table: "TourGuideAssignments",
                newName: "IX_TourGuideAssignments_TourDateId");

            migrationBuilder.RenameIndex(
                name: "IX_TourGuideAssignments_GuideId_ScheduleId",
                table: "TourGuideAssignments",
                newName: "IX_TourGuideAssignments_GuideId_TourDateId");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                table: "Reservations",
                newName: "TourDateId");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_ScheduleId",
                table: "Reservations",
                newName: "IX_Reservations_TourDateId");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "TourDates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScheduleId = table.Column<int>(type: "integer", nullable: true),
                    TourId = table.Column<int>(type: "integer", nullable: false),
                    BookedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsCancelled = table.Column<bool>(type: "boolean", nullable: false),
                    MaxCapacity = table.Column<int>(type: "integer", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourDates_TourSchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "TourSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TourDates_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 3, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 4, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 5, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 5, 2, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2026, 3, 25, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 25, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_TourDates_ScheduleId",
                table: "TourDates",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TourDates_TourId",
                table: "TourDates",
                column: "TourId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_TourDates_TourDateId",
                table: "Reservations",
                column: "TourDateId",
                principalTable: "TourDates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TourGuideAssignments_TourDates_TourDateId",
                table: "TourGuideAssignments",
                column: "TourDateId",
                principalTable: "TourDates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
