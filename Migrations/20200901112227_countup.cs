using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DarlingBotNet.Migrations
{
    public partial class countup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<int>(
            //    name: "countlvl",
            //    table: "LVLROLE",
            //    type: "uint"
            //    );
            //migrationBuilder.AddColumn<int>(
            //    name: "clanInfo",
            //    table: "Users",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "RaidMuted",
            //    table: "Guilds",
            //    nullable: false,
            //    defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            //migrationBuilder.AddColumn<bool>(
            //    name: "RaidStop",
            //    table: "Guilds",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddColumn<uint>(
            //    name: "RaidTime",
            //    table: "Guilds",
            //    nullable: false,
            //    defaultValue: 0u);

            //migrationBuilder.AddColumn<uint>(
            //    name: "RaidUserCount",
            //    table: "Guilds",
            //    nullable: false,
            //    defaultValue: 0u);

            //migrationBuilder.AddColumn<bool>(
            //    name: "SendUrlImage",
            //    table: "Channels",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddColumn<bool>(
            //    name: "UseRPcommand",
            //    table: "Channels",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddColumn<string>(
            //    name: "csUrlWhiteListString",
            //    table: "Channels",
            //    nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "clanInfo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RaidMuted",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "RaidStop",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "RaidTime",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "RaidUserCount",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "SendUrlImage",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "UseRPcommand",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "csUrlWhiteListString",
                table: "Channels");
        }
    }
}
