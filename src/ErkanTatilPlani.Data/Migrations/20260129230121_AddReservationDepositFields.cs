using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationDepositFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmount",
                table: "Reservations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "Reservations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DepositAmount", "PaidAmount" },
                values: new object[] { 0m, 0m });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DepositAmount", "PaidAmount" },
                values: new object[] { 0m, 0m });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DepositAmount", "PaidAmount" },
                values: new object[] { 0m, 0m });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DepositAmount", "PaidAmount" },
                values: new object[] { 0m, 0m });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DepositAmount", "PaidAmount" },
                values: new object[] { 0m, 0m });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "DepositAmount", "PaidAmount" },
                values: new object[] { 0m, 0m });

            // Mevcut verileri duzelt:
            // 1. Eski PaymentStatus=1 (Paid) olan kayitlari PaymentStatus=2 (FullyPaid) yap
            // 2. Bu kayitlarda PaidAmount = TotalPrice olarak set et
            migrationBuilder.Sql(@"
                UPDATE ""Reservations""
                SET ""PaymentStatus"" = 2, ""PaidAmount"" = ""TotalPrice"", ""DepositAmount"" = ""TotalPrice"" * 0.3
                WHERE ""PaymentStatus"" = 1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepositAmount",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "Reservations");
        }
    }
}
