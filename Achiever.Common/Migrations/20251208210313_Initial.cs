using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Achiever.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Login = table.Column<string>(type: "TEXT", nullable: true),
                    AvatarPath = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    IsAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    PaidPeriod = table.Column<int>(type: "INTEGER", nullable: false),
                    GoldUser = table.Column<bool>(type: "INTEGER", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TelegramChatId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AchievementItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Settings = table.Column<string>(type: "TEXT", nullable: true),
                    OwnerId = table.Column<int>(type: "INTEGER", nullable: true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AchievementItems_AchievementItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AchievementItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AchievementItems_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    UntilDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BadgeSettings = table.Column<string>(type: "TEXT", nullable: true),
                    UseValuesAfterStartOnly = table.Column<bool>(type: "INTEGER", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: true),
                    OwnerId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Challenges_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AchievementValueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AchievementId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementValueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AchievementValueItems_AchievementItems_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "AchievementItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AchievementValueItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DoubleAchievementValueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AchievementId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Count2 = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoubleAchievementValueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoubleAchievementValueItems_AchievementItems_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "AchievementItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DoubleAchievementValueItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Penalties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AchievementId = table.Column<int>(type: "INTEGER", nullable: true),
                    Days = table.Column<int>(type: "INTEGER", nullable: false),
                    Modifier = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsCumulative = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Penalties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Penalties_AchievementItems_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "AchievementItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChallengeAimItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AchievementId = table.Column<int>(type: "INTEGER", nullable: false),
                    UntilDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Count = table.Column<int>(type: "INTEGER", nullable: true),
                    DaysPeriod = table.Column<int>(type: "INTEGER", nullable: true),
                    MinPerDayCount = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxDaysGap = table.Column<int>(type: "INTEGER", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Settings = table.Column<string>(type: "TEXT", nullable: true),
                    ChallengeId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeAimItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeAimItems_AchievementItems_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "AchievementItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeAimItems_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChallengeRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    ChildId = table.Column<int>(type: "INTEGER", nullable: true),
                    StartBlocking = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeRequirements_Challenges_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Challenges",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChallengeRequirements_Challenges_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Challenges",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserChallengeInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChallengeId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsComplete = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompleteTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChallengeInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserChallengeInfos_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChallengeInfos_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementItems_OwnerId",
                table: "AchievementItems",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementItems_ParentId",
                table: "AchievementItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementValueItems_AchievementId",
                table: "AchievementValueItems",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementValueItems_UserId",
                table: "AchievementValueItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeAimItems_AchievementId",
                table: "ChallengeAimItems",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeAimItems_ChallengeId",
                table: "ChallengeAimItems",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeRequirements_ChildId",
                table: "ChallengeRequirements",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeRequirements_ParentId",
                table: "ChallengeRequirements",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_OwnerId",
                table: "Challenges",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DoubleAchievementValueItems_AchievementId",
                table: "DoubleAchievementValueItems",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_DoubleAchievementValueItems_UserId",
                table: "DoubleAchievementValueItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Penalties_AchievementId",
                table: "Penalties",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChallengeInfos_ChallengeId",
                table: "UserChallengeInfos",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChallengeInfos_UserId",
                table: "UserChallengeInfos",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievementValueItems");

            migrationBuilder.DropTable(
                name: "ChallengeAimItems");

            migrationBuilder.DropTable(
                name: "ChallengeRequirements");

            migrationBuilder.DropTable(
                name: "DoubleAchievementValueItems");

            migrationBuilder.DropTable(
                name: "Penalties");

            migrationBuilder.DropTable(
                name: "UserChallengeInfos");

            migrationBuilder.DropTable(
                name: "AchievementItems");

            migrationBuilder.DropTable(
                name: "Challenges");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
