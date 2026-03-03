using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFaz15Features : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessibilityInfo",
                table: "Tours",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSustainable",
                table: "Tours",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWheelchairAccessible",
                table: "Tours",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MobilityLevel",
                table: "Tours",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SustainabilityInfo",
                table: "Tours",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SustainabilityScore",
                table: "Tours",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TourDigitalContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TourId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ContentTypeId = table.Column<int>(type: "integer", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    RequiresReservation = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourDigitalContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourDigitalContents_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TourPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    FinalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourPackages_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TourPackageItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TourPackageId = table.Column<int>(type: "integer", nullable: false),
                    TourId = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    NumberOfPeople = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourPackageItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourPackageItems_TourPackages_TourPackageId",
                        column: x => x.TourPackageId,
                        principalTable: "TourPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourPackageItems_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "AccessibilityInfo", "IsSustainable", "IsWheelchairAccessible", "MobilityLevel", "SustainabilityInfo", "SustainabilityScore" },
                values: new object[] { null, false, false, 0, null, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_TourDigitalContents_TourId",
                table: "TourDigitalContents",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_TourPackageItems_TourId",
                table: "TourPackageItems",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_TourPackageItems_TourPackageId",
                table: "TourPackageItems",
                column: "TourPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_TourPackages_VisitorId",
                table: "TourPackages",
                column: "VisitorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourDigitalContents");

            migrationBuilder.DropTable(
                name: "TourPackageItems");

            migrationBuilder.DropTable(
                name: "TourPackages");

            migrationBuilder.DropColumn(
                name: "AccessibilityInfo",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "IsSustainable",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "IsWheelchairAccessible",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "MobilityLevel",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "SustainabilityInfo",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "SustainabilityScore",
                table: "Tours");
        }
    }
}
