using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EpinelPS.Migrations
{
    /// <inheritdoc />
    public partial class Campaign1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BanEnd",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "BanId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BanStart",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "BattleTime",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ClaimedJukeboxRewardTriggers",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "DispatchClearList",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "DispatchCollectionLv",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DispatchFavoriteLv",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DispatchLv",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DispatchResetCount",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExperiencePoint",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InfraCoreExp",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InfraCoreLvl",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "JukeboxBgm",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "LastClearedDifficulty",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastHardStageCleared",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastNormalStageCleared",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastStoryStageCleared",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Memorial",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "OutpostBattleLevel",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OutpostBattleLevelExp",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProfileCardsData",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "ProfileFrame",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProfileIconId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ProfileIconIsPrism",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RepresentationTeamDataNew",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "TitleId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserLevel",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ViewedScenarios",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.CreateTable(
                name: "CharacterModel",
                columns: table => new
                {
                    Csn = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tid = table.Column<int>(type: "INTEGER", nullable: false),
                    CostumeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    UltimateLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    Skill1Lvl = table.Column<int>(type: "INTEGER", nullable: false),
                    Skill2Lvl = table.Column<int>(type: "INTEGER", nullable: false),
                    Grade = table.Column<int>(type: "INTEGER", nullable: false),
                    IsMainForce = table.Column<bool>(type: "INTEGER", nullable: false),
                    NameCode = table.Column<int>(type: "INTEGER", nullable: false),
                    BondLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    BondLevelExp = table.Column<int>(type: "INTEGER", nullable: false),
                    Favorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    TotalCounseledCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedDialogs = table.Column<string>(type: "TEXT", nullable: false),
                    FlushableWatchedDialogIds = table.Column<string>(type: "TEXT", nullable: false),
                    ObtainedRewardLevels = table.Column<string>(type: "TEXT", nullable: false),
                    RareType = table.Column<int>(type: "INTEGER", nullable: false),
                    RewardStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterModel", x => x.Csn);
                    table.ForeignKey(
                        name: "FK_CharacterModel_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClearedTutorial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameUserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    TutorialId = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClearedTutorial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClearedTutorial_Users_GameUserId",
                        column: x => x.GameUserId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameUserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrencyModel_Users_GameUserId",
                        column: x => x.GameUserId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FieldInfo",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    CompletedStages = table.Column<string>(type: "TEXT", nullable: false),
                    FieldItemTableIdList = table.Column<string>(type: "TEXT", nullable: false),
                    AcquiredPasswordList = table.Column<string>(type: "TEXT", nullable: false),
                    UnlockedDoorList = table.Column<string>(type: "TEXT", nullable: false),
                    BossEntered = table.Column<bool>(type: "INTEGER", nullable: false),
                    PositionJson = table.Column<string>(type: "TEXT", nullable: false),
                    MapName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldInfo_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameUserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    QuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRewardRecieved = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestProgress_Users_GameUserId",
                        column: x => x.GameUserId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    LastContentsTeamNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamType = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    SlotIds = table.Column<string>(type: "TEXT", nullable: false),
                    SlotIdTypes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamModel_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompletedFieldObject",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ActionAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    PositionId = table.Column<string>(type: "TEXT", nullable: false),
                    Json = table.Column<string>(type: "TEXT", nullable: false),
                    FieldInfoId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedFieldObject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletedFieldObject_FieldInfo_FieldInfoId",
                        column: x => x.FieldInfoId,
                        principalTable: "FieldInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompletedFieldObject_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterModel_UserId",
                table: "CharacterModel",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClearedTutorial_GameUserId",
                table: "ClearedTutorial",
                column: "GameUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedFieldObject_FieldInfoId",
                table: "CompletedFieldObject",
                column: "FieldInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedFieldObject_UserId",
                table: "CompletedFieldObject",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyModel_GameUserId",
                table: "CurrencyModel",
                column: "GameUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldInfo_UserId",
                table: "FieldInfo",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestProgress_GameUserId",
                table: "QuestProgress",
                column: "GameUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamModel_UserId",
                table: "TeamModel",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterModel");

            migrationBuilder.DropTable(
                name: "ClearedTutorial");

            migrationBuilder.DropTable(
                name: "CompletedFieldObject");

            migrationBuilder.DropTable(
                name: "CurrencyModel");

            migrationBuilder.DropTable(
                name: "QuestProgress");

            migrationBuilder.DropTable(
                name: "TeamModel");

            migrationBuilder.DropTable(
                name: "FieldInfo");

            migrationBuilder.DropColumn(
                name: "BanEnd",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BanId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BanStart",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BattleTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ClaimedJukeboxRewardTriggers",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DispatchClearList",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DispatchCollectionLv",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DispatchFavoriteLv",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DispatchLv",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DispatchResetCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExperiencePoint",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InfraCoreExp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InfraCoreLvl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JukeboxBgm",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastClearedDifficulty",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastHardStageCleared",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastNormalStageCleared",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastStoryStageCleared",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Memorial",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OutpostBattleLevel",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OutpostBattleLevelExp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileCardsData",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileFrame",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileIconId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileIconIsPrism",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RepresentationTeamDataNew",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TitleId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserLevel",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ViewedScenarios",
                table: "Users");
        }
    }
}
