using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTourAdvancedFieldsAndTourDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Tours",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DifficultyId",
                table: "Tours",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Exclusions",
                table: "Tours",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuideLanguages",
                table: "Tours",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Inclusions",
                table: "Tours",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TourDates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TourId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxCapacity = table.Column<int>(type: "integer", nullable: true),
                    BookedCount = table.Column<int>(type: "integer", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourDates_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CategoryId", "DifficultyId", "Exclusions", "GuideLanguages", "Inclusions" },
                values: new object[] { 0, 1, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_TourDates_TourId",
                table: "TourDates",
                column: "TourId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourDates");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "DifficultyId",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "Exclusions",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "GuideLanguages",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "Inclusions",
                table: "Tours");
        }
    }
}
