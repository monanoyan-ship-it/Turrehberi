using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanySeoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FoundedYear",
                table: "Companies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "Companies",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "Companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Companies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SocialLinks",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tagline",
                table: "Companies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Address", "City", "CoverImageUrl", "Description", "FoundedYear", "MetaDescription", "MetaTitle", "Slug", "SocialLinks", "Tagline" },
                values: new object[] { "Izmir, Alsancak Mah. Kordon Cad. No:123", "Izmir", "https://picsum.photos/seed/egetur-cover/1200/400", "Ege bolgesi turlari konusunda uzman seyahat acentasi. 15 yillik deneyimimizle Efes, Pamukkale, Cesme ve daha fazlasini kesfetmenizi sagliyoruz.", 2010, "Efes, Pamukkale, Cesme ve tum Ege turlarinda uzman acenta. Gunubirlik ve konaklamali turlar.", "Ege Tur - Izmir Cikisli Ege Turlari", "ege-tur", "", "Ege'nin Guzelliklerini Kesfedin" });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Address", "City", "CoverImageUrl", "Description", "FoundedYear", "MetaDescription", "MetaTitle", "Slug", "SocialLinks", "Tagline" },
                values: new object[] { "Trabzon, Meydan Mah. Ataturk Cad. No:45", "Trabzon", "https://picsum.photos/seed/karadeniz-cover/1200/400", "Karadeniz'in essiz dogasini ve yaylalarini profesyonel rehberler esliginde kesfetmenizi saglayan tur firmasi. Uzungol, Ayder ve daha fazlasi.", 2015, "Uzungol, Ayder, Sumela ve tum Karadeniz yaylalarini kesfedin. Dogayla ic ice tatil firsatlari.", "Karadeniz Gezileri - Yayla ve Doga Turlari", "karadeniz-gezileri", "", "Yesil Cennetin Kapisi" });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Address", "City", "CoverImageUrl", "Description", "FoundedYear", "MetaDescription", "MetaTitle", "Slug", "SocialLinks", "Tagline" },
                values: new object[] { "Antalya, Konyaalti Mah. Liman Cad. No:78", "Antalya", "https://picsum.photos/seed/akdeniz-cover/1200/400", "Turkiye'nin en guzel sahillerinde unutulmaz deneyimler. Tekne turlari, dalıs aktiviteleri ve kultur gezileri duzenliyoruz.", 2008, "Kemer, Kas, Kekova tekne turlari. Akdeniz'in en guzel koylarini ve antik kentleri kesfedin.", "Akdeniz Turizm - Antalya Tekne ve Kultur Turlari", "akdeniz-turizm", "", "Akdeniz'in Mavisiyle Tanisin" });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Address", "City", "CoverImageUrl", "Description", "FoundedYear", "MetaDescription", "MetaTitle", "Slug", "SocialLinks", "Tagline" },
                values: new object[] { "Nevsehir, Goreme Kasabasi Merkez No:12", "Nevsehir", "https://picsum.photos/seed/kapadokya-cover/1200/400", "Kapadokya'nin buleyuci peri bacalari uzerinde balon deneyimi ve kultur turlari. Yeralti sehirleri, vadiler ve daha fazlasi.", 2012, "Kapadokya balon turu, yeralti sehirleri, ATV safari. Peri bacalari uzerinde unutulmaz deneyim.", "Kapadokya Balonlari - Sicak Hava Balonu ve Tur", "kapadokya-balonlari", "", "Gokyuzunden Kapadokya" });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Address", "City", "CoverImageUrl", "Description", "FoundedYear", "MetaDescription", "MetaTitle", "Slug", "SocialLinks", "Tagline" },
                values: new object[] { "Istanbul, Sultanahmet Mah. Divanyolu Cad. No:56", "Istanbul", "https://picsum.photos/seed/istanbul-cover/1200/400", "Dunyanin en etkileyici sehrinde tarihi ve kulturel turlarin adresi. Sultanahmet, Bogaz turlari ve ozel organizasyonlar.", 2005, "Sultanahmet, Ayasofya, Topkapi Sarayi, Bogaz turu. Istanbul'un tum guzelliklerini kesfedin.", "Istanbul Turlari - Tarihi Yarimada ve Bogaz Turu", "istanbul-turlari", "", "Iki Kitanin Kesisim Noktasi" });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Slug",
                table: "Companies",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_Slug",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "FoundedYear",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SocialLinks",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Tagline",
                table: "Companies");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Address", "Description" },
                values: new object[] { "Izmir, Alsancak", "Ege bolgesi turlari" });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Address", "Description" },
                values: new object[] { "Trabzon, Meydan", "Karadeniz yayla turlari" });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Address", "Description" },
                values: new object[] { "Antalya, Konyaalti", "Akdeniz sahil turlari" });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Address", "Description" },
                values: new object[] { "Nevsehir, Goreme", "Kapadokya balon ve kultur turlari" });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Address", "Description" },
                values: new object[] { "Istanbul, Sultanahmet", "Istanbul sehir ve bogazici turlari" });
        }
    }
}
