using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationDate",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ApplicationNotes",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContractFileUrl",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractUploadedAt",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedById",
                table: "Companies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Companies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApplicationDate", "ApplicationNotes", "ContractFileUrl", "ContractUploadedAt", "RejectionReason", "ReviewNotes", "ReviewedAt", "ReviewedById", "StatusId" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", "", null, "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1 });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApplicationDate", "ApplicationNotes", "ContractFileUrl", "ContractUploadedAt", "RejectionReason", "ReviewNotes", "ReviewedAt", "ReviewedById", "StatusId" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", "", null, "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1 });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApplicationDate", "ApplicationNotes", "ContractFileUrl", "ContractUploadedAt", "RejectionReason", "ReviewNotes", "ReviewedAt", "ReviewedById", "StatusId" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", "", null, "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1 });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApplicationDate", "ApplicationNotes", "ContractFileUrl", "ContractUploadedAt", "RejectionReason", "ReviewNotes", "ReviewedAt", "ReviewedById", "StatusId" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", "", null, "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1 });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApplicationDate", "ApplicationNotes", "ContractFileUrl", "ContractUploadedAt", "RejectionReason", "ReviewNotes", "ReviewedAt", "ReviewedById", "StatusId" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", "", null, "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_ReviewedById",
                table: "Companies",
                column: "ReviewedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Visitors_ReviewedById",
                table: "Companies",
                column: "ReviewedById",
                principalTable: "Visitors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Visitors_ReviewedById",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_ReviewedById",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ApplicationDate",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ApplicationNotes",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ContractFileUrl",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ContractUploadedAt",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ReviewedById",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Companies");
        }
    }
}
