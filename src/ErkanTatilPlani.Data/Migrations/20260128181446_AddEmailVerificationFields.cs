using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "Visitors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationTokenExpiry",
                table: "Visitors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                table: "Visitors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EmailVerificationToken", "EmailVerificationTokenExpiry", "EmailVerified" },
                values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EmailVerificationToken", "EmailVerificationTokenExpiry", "EmailVerified" },
                values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "EmailVerificationToken", "EmailVerificationTokenExpiry", "EmailVerified" },
                values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "EmailVerificationToken", "EmailVerificationTokenExpiry", "EmailVerified" },
                values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "EmailVerificationToken", "EmailVerificationTokenExpiry", "EmailVerified" },
                values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "EmailVerificationToken", "EmailVerificationTokenExpiry", "EmailVerified" },
                values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "EmailVerificationToken", "EmailVerificationTokenExpiry", "EmailVerified" },
                values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "EmailVerificationToken", "EmailVerificationTokenExpiry", "EmailVerified" },
                values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "EmailVerificationToken", "EmailVerificationTokenExpiry", "EmailVerified" },
                values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "EmailVerificationToken", "EmailVerificationTokenExpiry", "EmailVerified" },
                values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "EmailVerificationToken", "EmailVerificationTokenExpiry", "EmailVerified" },
                values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "EmailVerificationToken", "EmailVerificationTokenExpiry", "EmailVerified" },
                values: new object[] { null, null, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "EmailVerificationTokenExpiry",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                table: "Visitors");
        }
    }
}
