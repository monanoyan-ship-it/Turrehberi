using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVolumeRulesToPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VolumeRules",
                table: "Promotions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VolumeRules",
                table: "Promotions");
        }
    }
}
