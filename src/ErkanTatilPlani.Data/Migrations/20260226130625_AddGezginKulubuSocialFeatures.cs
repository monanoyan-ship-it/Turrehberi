using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGezginKulubuSocialFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Visitors",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FollowerCount",
                table: "Visitors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollowingCount",
                table: "Visitors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsProfilePublic",
                table: "Visitors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TripStoryCount",
                table: "Visitors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TravelerFollows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FollowerId = table.Column<int>(type: "integer", nullable: false),
                    FollowedId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelerFollows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelerFollows_Visitors_FollowedId",
                        column: x => x.FollowedId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TravelerFollows_Visitors_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TripStories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    ReservationId = table.Column<int>(type: "integer", nullable: true),
                    TourId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    TravelDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Destination = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LikeCount = table.Column<int>(type: "integer", nullable: false),
                    CommentCount = table.Column<int>(type: "integer", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripStories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripStories_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TripStories_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TripStories_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TripStoryComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TripStoryId = table.Column<int>(type: "integer", nullable: false),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ParentCommentId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripStoryComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripStoryComments_TripStories_TripStoryId",
                        column: x => x.TripStoryId,
                        principalTable: "TripStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TripStoryComments_TripStoryComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "TripStoryComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripStoryComments_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TripStoryLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TripStoryId = table.Column<int>(type: "integer", nullable: false),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripStoryLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripStoryLikes_TripStories_TripStoryId",
                        column: x => x.TripStoryId,
                        principalTable: "TripStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TripStoryLikes_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TripStoryPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TripStoryId = table.Column<int>(type: "integer", nullable: false),
                    PhotoUrl = table.Column<string>(type: "text", nullable: false),
                    Caption = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripStoryPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripStoryPhotos_TripStories_TripStoryId",
                        column: x => x.TripStoryId,
                        principalTable: "TripStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Bio", "FollowerCount", "FollowingCount", "IsProfilePublic", "TripStoryCount" },
                values: new object[] { null, 0, 0, true, 0 });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Bio", "FollowerCount", "FollowingCount", "IsProfilePublic", "TripStoryCount" },
                values: new object[] { null, 0, 0, true, 0 });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Bio", "FollowerCount", "FollowingCount", "IsProfilePublic", "TripStoryCount" },
                values: new object[] { null, 0, 0, true, 0 });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Bio", "FollowerCount", "FollowingCount", "IsProfilePublic", "TripStoryCount" },
                values: new object[] { null, 0, 0, true, 0 });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Bio", "FollowerCount", "FollowingCount", "IsProfilePublic", "TripStoryCount" },
                values: new object[] { null, 0, 0, true, 0 });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Bio", "FollowerCount", "FollowingCount", "IsProfilePublic", "TripStoryCount" },
                values: new object[] { null, 0, 0, true, 0 });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Bio", "FollowerCount", "FollowingCount", "IsProfilePublic", "TripStoryCount" },
                values: new object[] { null, 0, 0, true, 0 });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Bio", "FollowerCount", "FollowingCount", "IsProfilePublic", "TripStoryCount" },
                values: new object[] { null, 0, 0, true, 0 });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Bio", "FollowerCount", "FollowingCount", "IsProfilePublic", "TripStoryCount" },
                values: new object[] { null, 0, 0, true, 0 });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Bio", "FollowerCount", "FollowingCount", "IsProfilePublic", "TripStoryCount" },
                values: new object[] { null, 0, 0, true, 0 });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Bio", "FollowerCount", "FollowingCount", "IsProfilePublic", "TripStoryCount" },
                values: new object[] { null, 0, 0, true, 0 });

            migrationBuilder.UpdateData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Bio", "FollowerCount", "FollowingCount", "IsProfilePublic", "TripStoryCount" },
                values: new object[] { null, 0, 0, true, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_TravelerFollows_FollowedId",
                table: "TravelerFollows",
                column: "FollowedId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelerFollows_FollowerId_FollowedId",
                table: "TravelerFollows",
                columns: new[] { "FollowerId", "FollowedId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripStories_CreatedAt",
                table: "TripStories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TripStories_ReservationId",
                table: "TripStories",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_TripStories_TourId",
                table: "TripStories",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_TripStories_VisitorId",
                table: "TripStories",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_TripStoryComments_ParentCommentId",
                table: "TripStoryComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_TripStoryComments_TripStoryId",
                table: "TripStoryComments",
                column: "TripStoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TripStoryComments_VisitorId",
                table: "TripStoryComments",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_TripStoryLikes_TripStoryId_VisitorId",
                table: "TripStoryLikes",
                columns: new[] { "TripStoryId", "VisitorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripStoryLikes_VisitorId",
                table: "TripStoryLikes",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_TripStoryPhotos_TripStoryId",
                table: "TripStoryPhotos",
                column: "TripStoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TravelerFollows");

            migrationBuilder.DropTable(
                name: "TripStoryComments");

            migrationBuilder.DropTable(
                name: "TripStoryLikes");

            migrationBuilder.DropTable(
                name: "TripStoryPhotos");

            migrationBuilder.DropTable(
                name: "TripStories");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "FollowerCount",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "FollowingCount",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "IsProfilePublic",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "TripStoryCount",
                table: "Visitors");
        }
    }
}
