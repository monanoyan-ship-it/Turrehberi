using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodSettingsAndPaymentMethodSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodSystemName",
                table: "Reservations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "iyzico-card");

            migrationBuilder.AddColumn<string>(
                name: "PaymentProviderSystemName",
                table: "Reservations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "iyzico");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodSystemName",
                table: "PaymentTransactions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "iyzico-card");

            migrationBuilder.CreateTable(
                name: "PaymentMethodSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SystemName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ProviderSystemName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsMarketplaceSplit = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IconClass = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApiKey = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    SecretKey = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IsSandbox = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraSettingsJson = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethodSettings", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PaymentMethodSystemName", "PaymentProviderSystemName" },
                values: new object[] { "iyzico-card", "iyzico" });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PaymentMethodSystemName", "PaymentProviderSystemName" },
                values: new object[] { "iyzico-card", "iyzico" });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PaymentMethodSystemName", "PaymentProviderSystemName" },
                values: new object[] { "iyzico-card", "iyzico" });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "PaymentMethodSystemName", "PaymentProviderSystemName" },
                values: new object[] { "iyzico-card", "iyzico" });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "PaymentMethodSystemName", "PaymentProviderSystemName" },
                values: new object[] { "iyzico-card", "iyzico" });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "PaymentMethodSystemName", "PaymentProviderSystemName" },
                values: new object[] { "iyzico-card", "iyzico" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodSettings_IsActive_DisplayOrder",
                table: "PaymentMethodSettings",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodSettings_IsActive_IsEnabled_IsDefault",
                table: "PaymentMethodSettings",
                columns: new[] { "IsActive", "IsEnabled", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodSettings_SystemName",
                table: "PaymentMethodSettings",
                column: "SystemName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentMethodSettings");

            migrationBuilder.DropColumn(
                name: "PaymentMethodSystemName",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaymentProviderSystemName",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaymentMethodSystemName",
                table: "PaymentTransactions");
        }
    }
}
