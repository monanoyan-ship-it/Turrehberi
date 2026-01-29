using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitorProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Visitors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Visitors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Visitors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Address", "AvatarUrl", "BirthDate" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Address", "AvatarUrl", "BirthDate" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Address", "AvatarUrl", "BirthDate" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Address", "AvatarUrl", "BirthDate" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Address", "AvatarUrl", "BirthDate" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Address", "AvatarUrl", "BirthDate" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Address", "AvatarUrl", "BirthDate" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Address", "AvatarUrl", "BirthDate" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Address", "AvatarUrl", "BirthDate" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Address", "AvatarUrl", "BirthDate" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Address", "AvatarUrl", "BirthDate" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Address", "AvatarUrl", "BirthDate" },
                values: new object[] { null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Visitors");
        }
    }
}
