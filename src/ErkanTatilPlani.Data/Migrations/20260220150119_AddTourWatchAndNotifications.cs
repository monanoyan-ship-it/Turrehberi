using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTourWatchAndNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MeetingPointAddress",
                table: "Tours",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MeetingPointLat",
                table: "Tours",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MeetingPointLng",
                table: "Tours",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParticipantInfo",
                table: "Reservations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoLink",
                table: "Reservations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCode",
                table: "Reservations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrToken",
                table: "Reservations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    TitleKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MessageKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MessageParams = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NotificationTypeId = table.Column<int>(type: "integer", nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "integer", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TourWatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    TourId = table.Column<int>(type: "integer", nullable: false),
                    WatchDays = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NotifyScarcity = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyPriceChange = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyNewDate = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourWatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourWatches_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourWatches_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ParticipantInfo", "PhotoLink", "QrCode", "QrToken" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ParticipantInfo", "PhotoLink", "QrCode", "QrToken" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ParticipantInfo", "PhotoLink", "QrCode", "QrToken" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ParticipantInfo", "PhotoLink", "QrCode", "QrToken" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ParticipantInfo", "PhotoLink", "QrCode", "QrToken" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ParticipantInfo", "PhotoLink", "QrCode", "QrToken" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "MeetingPointAddress", "MeetingPointLat", "MeetingPointLng" },
                values: new object[] { null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_VisitorId_IsRead",
                table: "Notifications",
                columns: new[] { "VisitorId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_TourWatches_TourId",
                table: "TourWatches",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_TourWatches_VisitorId_TourId",
                table: "TourWatches",
                columns: new[] { "VisitorId", "TourId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "TourWatches");

            migrationBuilder.DropColumn(
                name: "MeetingPointAddress",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "MeetingPointLat",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "MeetingPointLng",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "ParticipantInfo",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PhotoLink",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "QrCode",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "QrToken",
                table: "Reservations");
        }
    }
}
