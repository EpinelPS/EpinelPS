using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EpinelPS.Migrations
{
    /// <inheritdoc />
    public partial class GameUserExpansion1 : Migration
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
                name: "GachaTutorialPlayCount",
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

            migrationBuilder.AddColumn<bool>(
                name: "IsBanned",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReset",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "LastStoryStageCleared",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWeeklyReset",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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

            migrationBuilder.AddColumn<int>(
                name: "SynchroDeviceLevel",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SynchroDeviceUpgraded",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TitleId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "sickpulls",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "GachaTutorialPlayCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InfraCoreExp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InfraCoreLvl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsBanned",
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
                name: "LastReset",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastStoryStageCleared",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastWeeklyReset",
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
                name: "SynchroDeviceLevel",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SynchroDeviceUpgraded",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TitleId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "sickpulls",
                table: "Users");
        }
    }
}
