using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTourSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "TourDates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ScheduleId",
                table: "TourDates",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TourSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TourId = table.Column<int>(type: "integer", nullable: false),
                    DaysOfWeekJson = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    DurationValue = table.Column<int>(type: "integer", nullable: false),
                    DurationUnitId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxCapacity = table.Column<int>(type: "integer", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourSchedules_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TourDates_ScheduleId",
                table: "TourDates",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TourSchedules_TourId",
                table: "TourSchedules",
                column: "TourId");

            migrationBuilder.AddForeignKey(
                name: "FK_TourDates_TourSchedules_ScheduleId",
                table: "TourDates",
                column: "ScheduleId",
                principalTable: "TourSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourDates_TourSchedules_ScheduleId",
                table: "TourDates");

            migrationBuilder.DropTable(
                name: "TourSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TourDates_ScheduleId",
                table: "TourDates");

            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "TourDates");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "TourDates");
        }
    }
}
