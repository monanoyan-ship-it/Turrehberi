using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppliedPromotions",
                table: "Reservations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Reservations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Reservations",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PromotionId",
                table: "Reservations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EarlyBirdEnabled",
                table: "Companies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EarlyBirdRules",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GroupDiscountEnabled",
                table: "Companies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GroupDiscountRules",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PromotionTypeId = table.Column<int>(type: "integer", nullable: false),
                    DiscountTypeId = table.Column<int>(type: "integer", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByVisitorId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UsageLimit = table.Column<int>(type: "integer", nullable: true),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    UsageLimitPerUser = table.Column<int>(type: "integer", nullable: true),
                    MinOrderAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxDiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MinDaysAhead = table.Column<int>(type: "integer", nullable: true),
                    MaxHoursBeforeStart = table.Column<int>(type: "integer", nullable: true),
                    MinAvailableCapacityPercent = table.Column<int>(type: "integer", nullable: true),
                    MinGroupSize = table.Column<int>(type: "integer", nullable: true),
                    MaxGroupSize = table.Column<int>(type: "integer", nullable: true),
                    FlashSaleStock = table.Column<int>(type: "integer", nullable: true),
                    FlashSaleSoldCount = table.Column<int>(type: "integer", nullable: false),
                    BundleTourIds = table.Column<string>(type: "text", nullable: true),
                    MinBundleTourCount = table.Column<int>(type: "integer", nullable: true),
                    SeasonRules = table.Column<string>(type: "text", nullable: true),
                    WeekendMultiplier = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    HighDemandMultiplier = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    HighDemandThresholdPercent = table.Column<int>(type: "integer", nullable: true),
                    ApplicableTourIds = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Promotions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Promotions_Visitors_CreatedByVisitorId",
                        column: x => x.CreatedByVisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PromotionUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PromotionId = table.Column<int>(type: "integer", nullable: false),
                    ReservationId = table.Column<int>(type: "integer", nullable: false),
                    VisitorId = table.Column<int>(type: "integer", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AppliedRule = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionUsages_Promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromotionUsages_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromotionUsages_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EarlyBirdEnabled", "EarlyBirdRules", "GroupDiscountEnabled", "GroupDiscountRules" },
                values: new object[] { false, null, false, null });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EarlyBirdEnabled", "EarlyBirdRules", "GroupDiscountEnabled", "GroupDiscountRules" },
                values: new object[] { false, null, false, null });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "EarlyBirdEnabled", "EarlyBirdRules", "GroupDiscountEnabled", "GroupDiscountRules" },
                values: new object[] { false, null, false, null });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "EarlyBirdEnabled", "EarlyBirdRules", "GroupDiscountEnabled", "GroupDiscountRules" },
                values: new object[] { false, null, false, null });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "EarlyBirdEnabled", "EarlyBirdRules", "GroupDiscountEnabled", "GroupDiscountRules" },
                values: new object[] { false, null, false, null });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AppliedPromotions", "CouponCode", "DiscountAmount", "PromotionId" },
                values: new object[] { null, null, 0m, null });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AppliedPromotions", "CouponCode", "DiscountAmount", "PromotionId" },
                values: new object[] { null, null, 0m, null });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AppliedPromotions", "CouponCode", "DiscountAmount", "PromotionId" },
                values: new object[] { null, null, 0m, null });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AppliedPromotions", "CouponCode", "DiscountAmount", "PromotionId" },
                values: new object[] { null, null, 0m, null });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AppliedPromotions", "CouponCode", "DiscountAmount", "PromotionId" },
                values: new object[] { null, null, 0m, null });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AppliedPromotions", "CouponCode", "DiscountAmount", "PromotionId" },
                values: new object[] { null, null, 0m, null });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PromotionId",
                table: "Reservations",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_CompanyId_Code",
                table: "Promotions",
                columns: new[] { "CompanyId", "Code" },
                unique: true,
                filter: "\"Code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_CreatedByVisitorId",
                table: "Promotions",
                column: "CreatedByVisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionUsages_PromotionId",
                table: "PromotionUsages",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionUsages_ReservationId",
                table: "PromotionUsages",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionUsages_VisitorId",
                table: "PromotionUsages",
                column: "VisitorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Promotions_PromotionId",
                table: "Reservations",
                column: "PromotionId",
                principalTable: "Promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Promotions_PromotionId",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "PromotionUsages");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_PromotionId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "AppliedPromotions",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PromotionId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "EarlyBirdEnabled",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "EarlyBirdRules",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "GroupDiscountEnabled",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "GroupDiscountRules",
                table: "Companies");
        }
    }
}
