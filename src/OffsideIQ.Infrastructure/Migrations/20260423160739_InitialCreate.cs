using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OffsideIQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "User"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShortCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    Stadium = table.Column<string>(type: "text", nullable: true),
                    League = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwayTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: false),
                    AwayScore = table.Column<int>(type: "integer", nullable: false),
                    MatchDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Competition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Venue = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Teams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Teams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Position = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    JerseyNumber = table.Column<int>(type: "integer", nullable: true),
                    Nationality = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchNotes_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchNotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MatchStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomePossession = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AwayPossession = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    HomeShotsTotal = table.Column<int>(type: "integer", nullable: false),
                    HomeShotsOnTarget = table.Column<int>(type: "integer", nullable: false),
                    AwayShotsTotal = table.Column<int>(type: "integer", nullable: false),
                    AwayShotsOnTarget = table.Column<int>(type: "integer", nullable: false),
                    HomePasses = table.Column<int>(type: "integer", nullable: false),
                    HomePassAccuracy = table.Column<int>(type: "integer", nullable: false),
                    AwayPasses = table.Column<int>(type: "integer", nullable: false),
                    AwayPassAccuracy = table.Column<int>(type: "integer", nullable: false),
                    HomeYellowCards = table.Column<int>(type: "integer", nullable: false),
                    HomeRedCards = table.Column<int>(type: "integer", nullable: false),
                    AwayYellowCards = table.Column<int>(type: "integer", nullable: false),
                    AwayRedCards = table.Column<int>(type: "integer", nullable: false),
                    HomeCorners = table.Column<int>(type: "integer", nullable: false),
                    AwayCorners = table.Column<int>(type: "integer", nullable: false),
                    HomeFouls = table.Column<int>(type: "integer", nullable: false),
                    AwayFouls = table.Column<int>(type: "integer", nullable: false),
                    HomeXg = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    AwayXg = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchStats_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerRatings_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerRatings_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_AwayTeamId",
                table: "Matches",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_CreatedByUserId",
                table: "Matches",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_HomeTeamId",
                table: "Matches",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchNotes_MatchId",
                table: "MatchNotes",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchNotes_UserId",
                table: "MatchNotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchStats_MatchId",
                table: "MatchStats",
                column: "MatchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRatings_MatchId_PlayerId",
                table: "PlayerRatings",
                columns: new[] { "MatchId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRatings_PlayerId",
                table: "PlayerRatings",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_TeamId",
                table: "Players",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_CreatedByUserId",
                table: "Teams",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_ShortCode",
                table: "Teams",
                column: "ShortCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchNotes");

            migrationBuilder.DropTable(
                name: "MatchStats");

            migrationBuilder.DropTable(
                name: "PlayerRatings");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
