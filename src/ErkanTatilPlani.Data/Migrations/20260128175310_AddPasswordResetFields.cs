using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Visitors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiry",
                table: "Visitors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordResetToken", "PasswordResetTokenExpiry" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordResetToken", "PasswordResetTokenExpiry" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PasswordResetToken", "PasswordResetTokenExpiry" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "PasswordResetToken", "PasswordResetTokenExpiry" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "PasswordResetToken", "PasswordResetTokenExpiry" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "PasswordResetToken", "PasswordResetTokenExpiry" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "PasswordResetToken", "PasswordResetTokenExpiry" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "PasswordResetToken", "PasswordResetTokenExpiry" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "PasswordResetToken", "PasswordResetTokenExpiry" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "PasswordResetToken", "PasswordResetTokenExpiry" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "PasswordResetToken", "PasswordResetTokenExpiry" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "PasswordResetToken", "PasswordResetTokenExpiry" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiry",
                table: "Visitors");
        }
    }
}
