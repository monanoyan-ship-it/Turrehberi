using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    AuthorId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MetaTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogPosts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BlogPosts_Visitors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BlogComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BlogPostId = table.Column<int>(type: "integer", nullable: false),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ParentCommentId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogComments_BlogComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "BlogComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BlogComments_BlogPosts_BlogPostId",
                        column: x => x.BlogPostId,
                        principalTable: "BlogPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogComments_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BlogPosts",
                columns: new[] { "Id", "AuthorId", "CategoryId", "CompanyId", "Content", "CreatedAt", "ImageUrl", "IsActive", "MetaDescription", "MetaTitle", "PublishedAt", "Slug", "StatusId", "Summary", "Tags", "Title", "UpdatedAt", "ViewCount" },
                values: new object[,]
                {
                    { 1, 1, 0, null, "<h2>Kapadokya Balon Turu Rehberi</h2><p>Kapadokya'nin essiz peri bacalari uzerinde balon turu, dunyanin en etkileyici deneyimlerinden biridir. Bu yazida, balon turuna katilmadan once bilmeniz gereken her seyi paylasiyoruz.</p><h3>En Iyi Sezon</h3><p>Nisan-Kasim ayları arasinda hava kosullari balon ucuslari icin en uygun donemdir. Ozellikle Mayis-Haziran ve Eylul-Ekim aylarinda hava en stabil halindedir.</p><h3>Fiyatlar</h3><p>Standart ucuslar 150-250 Euro arasinda degismektedir. Ozel ucuslar ise 300 Euro'dan baslamaktadir.</p><h3>Hazirlik</h3><p>Sabah erken kalkmaniz gerekecektir. Rahat kiyafetler ve spor ayakkabi giyin. Kameranizi unutmayin!</p>", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://picsum.photos/seed/blog-kapadokya/800/400", true, null, null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "kapadokyada-balon-turu-bilmeniz-gereken-her-sey", 1, "Kapadokya balon turuna katilmadan once bilmeniz gereken ipuclari, en iyi sezon ve hazirlik onerileri.", "kapadokya,balon,seyahat ipucu", "Kapadokya'da Balon Turu: Bilmeniz Gereken Her Sey", null, 342 },
                    { 2, 3, 1, 1, "<h2>Ege'nin Gizli Cennetleri</h2><p>Turkiye'nin Ege sahilleri, dunya uzerindeki en guzel kumsallara ve koylara ev sahipligi yapmaktadir. Bu yazida, henuz kesfedilmemis sakli koylari tanitiyoruz.</p><h3>1. Sazak Koyu - Mugla</h3><p>Berrak sulari ve bakir dogasiyla Sazak Koyu, huzur arayanlar icin ideal bir destinasyondur.</p><h3>2. Kabak Koyu - Fethiye</h3><p>Oludeniz'in guneybatisinda yer alan Kabak Koyu, dogal guzelligi ve sakli plajlariyla unlüdur.</p><h3>3. Hayıtbuku - Mugla</h3><p>Bodrum yakinlarindaki Hayitbuku, sakin atmosferi ve lezzetli deniz urunleriyle bilinir.</p>", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://picsum.photos/seed/blog-ege/800/400", true, null, null, new DateTime(2026, 1, 20, 14, 0, 0, 0, DateTimeKind.Utc), "egenin-sakli-koylari-kesfedilmemis-cennetler", 1, "Ege sahillerinde turistik kalabaliktan uzak, huzurlu ve dogal koylari kesfedin.", "ege,koy,deniz,doga", "Ege'nin Saklı Koyları: Keşfedilmemiş Cennetler", null, 215 },
                    { 3, 1, 2, null, "<h2>2026 Turizm Trendleri</h2><p>Turkiye turizm sektoru hizla gelismektedir. Iste 2026 yilinin one cikan trendleri:</p><h3>Eko-Turizm Yukseliste</h3><p>Surdurulebilir turizm anlayisi giderek yayginlasmaktadir. Dogayla uyumlu konaklama tesisleri ve karbon ayak izini azaltan tur programlari populerlesmektedir.</p><h3>Dijital Deneyimler</h3><p>Sanal gerceklik turlari ve arttirilmis gerceklik uygulamalari turizm sektorunde yeni firsatlar sunmaktadir.</p><h3>Gastronomi Turizmi</h3><p>Yerel lezzetleri kesfetmek amaciyla yapilan seyahatler her gecen gun artmaktadir.</p>", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://picsum.photos/seed/blog-trends/800/400", true, null, null, new DateTime(2026, 2, 1, 9, 0, 0, 0, DateTimeKind.Utc), "2026-yilinda-turkiye-turizm-trendleri", 1, "2026 yilinda Turkiye turizmini sekillendiren trendler ve yenilikler hakkinda detayli analiz.", "turizm,trend,2026", "2026 Yilinda Turkiye Turizm Trendleri", null, 178 },
                    { 4, 4, 4, 2, "<h2>Karadeniz Lezzetleri</h2><p>Karadeniz mutfagi, Turkiye'nin en zengin ve ozgun mutfaklarindan biridir.</p><h3>Muhlama</h3><p>Karadeniz'in en meshur lezzeti muhlama, tereyagi ve peynirle yapilan essiz bir yemektir.</p><h3>Kuymak</h3><p>Misir unu ve peynirle hazirlanan kuymak, ozellikle kis aylarinda tercih edilir.</p><h3>Hamsi</h3><p>Karadeniz denince akla ilk gelen balik olan hamsi, tava, pilav ve hatta tatli olarak bile hazirlanir.</p>", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://picsum.photos/seed/blog-karadeniz/800/400", true, null, null, new DateTime(2026, 2, 5, 11, 0, 0, 0, DateTimeKind.Utc), "karadeniz-yaylalarinda-lezzet-duragi", 1, "Karadeniz mutfagininin en ozel lezzetlerini yaylalarda tatma rehberi.", "karadeniz,yemek,lezzet,yayla", "Karadeniz Yaylalarinda Lezzet Duragi", null, 124 }
                });

            migrationBuilder.InsertData(
                table: "BlogComments",
                columns: new[] { "Id", "BlogPostId", "Content", "CreatedAt", "IsActive", "ParentCommentId", "UpdatedAt", "VisitorId" },
                values: new object[,]
                {
                    { 1, 1, "Cok faydali bir yazi olmus, tesekkurler! Kapadokya'ya gitmeden once mutlaka okunsun.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, null, 8 },
                    { 2, 1, "Gecen yil gittik, gercekten harikaydı. Eylul ayini tavsiye ederim.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, null, 9 },
                    { 3, 2, "Kabak Koyu gercekten muhtesem! Herkesin gitmesi lazim.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, null, 10 },
                    { 4, 3, "Eko-turizm trendi cok guzel, doga korundukca turizm de gelisir.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, null, 11 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogComments_BlogPostId",
                table: "BlogComments",
                column: "BlogPostId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogComments_ParentCommentId",
                table: "BlogComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogComments_VisitorId",
                table: "BlogComments",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_AuthorId",
                table: "BlogPosts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_CompanyId",
                table: "BlogPosts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Slug",
                table: "BlogPosts",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogComments");

            migrationBuilder.DropTable(
                name: "BlogPosts");
        }
    }
}
