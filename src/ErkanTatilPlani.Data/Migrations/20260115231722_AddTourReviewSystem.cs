using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTourReviewSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "Tours",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "Tours",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TourReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TourId = table.Column<int>(type: "integer", nullable: false),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    ReservationId = table.Column<int>(type: "integer", nullable: true),
                    OverallRating = table.Column<int>(type: "integer", nullable: false),
                    ServiceRating = table.Column<int>(type: "integer", nullable: true),
                    ValueRating = table.Column<int>(type: "integer", nullable: true),
                    LocationRating = table.Column<int>(type: "integer", nullable: true),
                    OrganizationRating = table.Column<int>(type: "integer", nullable: true),
                    GuideRating = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Pros = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Cons = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Comment = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    VisitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TravelTypeId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    ModeratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModeratedById = table.Column<int>(type: "integer", nullable: true),
                    ModerationNote = table.Column<string>(type: "text", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: false),
                    WouldRecommend = table.Column<bool>(type: "boolean", nullable: false),
                    HelpfulCount = table.Column<int>(type: "integer", nullable: false),
                    NotHelpfulCount = table.Column<int>(type: "integer", nullable: false),
                    ReportCount = table.Column<int>(type: "integer", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourReviews_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TourReviews_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourReviews_Visitors_ModeratedById",
                        column: x => x.ModeratedById,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TourReviews_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewHelpfuls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewId = table.Column<int>(type: "integer", nullable: false),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    IsHelpful = table.Column<bool>(type: "boolean", nullable: false),
                    VotedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewHelpfuls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewHelpfuls_TourReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "TourReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewHelpfuls_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewId = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Caption = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AltText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewImages_TourReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "TourReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewReplies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewId = table.Column<int>(type: "integer", nullable: false),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    ParentReplyId = table.Column<int>(type: "integer", nullable: true),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsFromCompany = table.Column<bool>(type: "boolean", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    ModeratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModeratedById = table.Column<int>(type: "integer", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    LikeCount = table.Column<int>(type: "integer", nullable: false),
                    ReportCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewReplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewReplies_ReviewReplies_ParentReplyId",
                        column: x => x.ParentReplyId,
                        principalTable: "ReviewReplies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewReplies_TourReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "TourReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewReplies_Visitors_ModeratedById",
                        column: x => x.ModeratedById,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReviewReplies_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewId = table.Column<int>(type: "integer", nullable: true),
                    ReplyId = table.Column<int>(type: "integer", nullable: true),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    ReasonId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedById = table.Column<int>(type: "integer", nullable: true),
                    ReviewNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewReports_ReviewReplies_ReplyId",
                        column: x => x.ReplyId,
                        principalTable: "ReviewReplies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewReports_TourReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "TourReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewReports_Visitors_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReviewReports_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Tours",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "AverageRating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewHelpfuls_ReviewId_VisitorId",
                table: "ReviewHelpfuls",
                columns: new[] { "ReviewId", "VisitorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewHelpfuls_VisitorId",
                table: "ReviewHelpfuls",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewImages_ReviewId",
                table: "ReviewImages",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReplies_ModeratedById",
                table: "ReviewReplies",
                column: "ModeratedById");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReplies_ParentReplyId",
                table: "ReviewReplies",
                column: "ParentReplyId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReplies_ReviewId",
                table: "ReviewReplies",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReplies_VisitorId",
                table: "ReviewReplies",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReports_ReplyId",
                table: "ReviewReports",
                column: "ReplyId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReports_ReviewedById",
                table: "ReviewReports",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReports_ReviewId",
                table: "ReviewReports",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReports_VisitorId",
                table: "ReviewReports",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_TourReviews_ModeratedById",
                table: "TourReviews",
                column: "ModeratedById");

            migrationBuilder.CreateIndex(
                name: "IX_TourReviews_ReservationId",
                table: "TourReviews",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_TourReviews_TourId_VisitorId",
                table: "TourReviews",
                columns: new[] { "TourId", "VisitorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TourReviews_VisitorId",
                table: "TourReviews",
                column: "VisitorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewHelpfuls");

            migrationBuilder.DropTable(
                name: "ReviewImages");

            migrationBuilder.DropTable(
                name: "ReviewReports");

            migrationBuilder.DropTable(
                name: "ReviewReplies");

            migrationBuilder.DropTable(
                name: "TourReviews");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "Tours");
        }
    }
}
