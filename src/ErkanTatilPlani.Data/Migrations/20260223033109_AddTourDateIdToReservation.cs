using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTourDateIdToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TourDateId",
                table: "Reservations",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 1,
                column: "TourDateId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 2,
                column: "TourDateId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 3,
                column: "TourDateId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 4,
                column: "TourDateId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 5,
                column: "TourDateId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 6,
                column: "TourDateId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TourDateId",
                table: "Reservations",
                column: "TourDateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_TourDates_TourDateId",
                table: "Reservations",
                column: "TourDateId",
                principalTable: "TourDates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_TourDates_TourDateId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_TourDateId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TourDateId",
                table: "Reservations");
        }
    }
}
