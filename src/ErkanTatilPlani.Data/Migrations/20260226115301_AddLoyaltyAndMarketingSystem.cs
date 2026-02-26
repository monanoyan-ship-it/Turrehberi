using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLoyaltyAndMarketingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CreditBalance",
                table: "Visitors",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LoyaltyTierId",
                table: "Visitors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReferralCode",
                table: "Visitors",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditUsed",
                table: "Reservations",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AbandonedCarts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisitorId = table.Column<int>(type: "integer", nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TourId = table.Column<int>(type: "integer", nullable: false),
                    ScheduleId = table.Column<int>(type: "integer", nullable: true),
                    NumberOfPeople = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DateToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EmailSent = table.Column<bool>(type: "boolean", nullable: false),
                    EmailSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Recovered = table.Column<bool>(type: "boolean", nullable: false),
                    ReservationId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbandonedCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbandonedCarts_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AbandonedCarts_TourSchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "TourSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AbandonedCarts_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbandonedCarts_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyTierHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    PreviousTierId = table.Column<int>(type: "integer", nullable: false),
                    NewTierId = table.Column<int>(type: "integer", nullable: false),
                    CompletedTourCount = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyTierHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyTierHistories_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Referrals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReferrerVisitorId = table.Column<int>(type: "integer", nullable: false),
                    ReferredVisitorId = table.Column<int>(type: "integer", nullable: false),
                    ReferralCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    BonusAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReservationId = table.Column<int>(type: "integer", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RewardedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Referrals_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Referrals_Visitors_ReferredVisitorId",
                        column: x => x.ReferredVisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referrals_Visitors_ReferrerVisitorId",
                        column: x => x.ReferrerVisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledEmails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    ReservationId = table.Column<int>(type: "integer", nullable: true),
                    EmailTypeId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    EmailTo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TemplateData = table.Column<string>(type: "text", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledEmails_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ScheduledEmails_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TourCredits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    TransactionTypeId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReservationId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourCredits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourCredits_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TourCredits_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreditUsed",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreditUsed",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreditUsed",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreditUsed",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreditUsed",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreditUsed",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreditBalance", "LoyaltyTierId", "ReferralCode" },
                values: new object[] { 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreditBalance", "LoyaltyTierId", "ReferralCode" },
                values: new object[] { 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreditBalance", "LoyaltyTierId", "ReferralCode" },
                values: new object[] { 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreditBalance", "LoyaltyTierId", "ReferralCode" },
                values: new object[] { 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreditBalance", "LoyaltyTierId", "ReferralCode" },
                values: new object[] { 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreditBalance", "LoyaltyTierId", "ReferralCode" },
                values: new object[] { 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreditBalance", "LoyaltyTierId", "ReferralCode" },
                values: new object[] { 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreditBalance", "LoyaltyTierId", "ReferralCode" },
                values: new object[] { 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreditBalance", "LoyaltyTierId", "ReferralCode" },
                values: new object[] { 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreditBalance", "LoyaltyTierId", "ReferralCode" },
                values: new object[] { 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreditBalance", "LoyaltyTierId", "ReferralCode" },
                values: new object[] { 0m, 0, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreditBalance", "LoyaltyTierId", "ReferralCode" },
                values: new object[] { 0m, 0, null });

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_ReferralCode",
                table: "Visitors",
                column: "ReferralCode",
                unique: true,
                filter: "\"ReferralCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AbandonedCarts_Email_TourId",
                table: "AbandonedCarts",
                columns: new[] { "Email", "TourId" });

            migrationBuilder.CreateIndex(
                name: "IX_AbandonedCarts_ReservationId",
                table: "AbandonedCarts",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_AbandonedCarts_ScheduleId",
                table: "AbandonedCarts",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_AbandonedCarts_TourId",
                table: "AbandonedCarts",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_AbandonedCarts_VisitorId",
                table: "AbandonedCarts",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTierHistories_VisitorId",
                table: "LoyaltyTierHistories",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReferredVisitorId",
                table: "Referrals",
                column: "ReferredVisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReferrerVisitorId_ReferredVisitorId",
                table: "Referrals",
                columns: new[] { "ReferrerVisitorId", "ReferredVisitorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReservationId",
                table: "Referrals",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEmails_ReservationId",
                table: "ScheduledEmails",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEmails_StatusId_ScheduledAt",
                table: "ScheduledEmails",
                columns: new[] { "StatusId", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEmails_VisitorId",
                table: "ScheduledEmails",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_TourCredits_ReservationId",
                table: "TourCredits",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_TourCredits_VisitorId",
                table: "TourCredits",
                column: "VisitorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbandonedCarts");

            migrationBuilder.DropTable(
                name: "LoyaltyTierHistories");

            migrationBuilder.DropTable(
                name: "Referrals");

            migrationBuilder.DropTable(
                name: "ScheduledEmails");

            migrationBuilder.DropTable(
                name: "TourCredits");

            migrationBuilder.DropIndex(
                name: "IX_Visitors_ReferralCode",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "CreditBalance",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "LoyaltyTierId",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "ReferralCode",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "CreditUsed",
                table: "Reservations");
        }
    }
}
