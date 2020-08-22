using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DarlingBotNet.Migrations
{
    public partial class upall2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "channeltype",
            //    table: "Channels"
            //    );

            //migrationBuilder.DropColumn(
            //    name: "antimat",
            //    table: "Channels"
            //    );

            migrationBuilder.AddColumn<int>(
                name: "RaidStop",
                table: "Guilds",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RaidTime",
                table: "Guilds",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<int>(
                name: "RaidUserCount",
                table: "Guilds",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<int>(
                name: "RaidMuted",
                table: "Guilds",
                nullable: false,
                defaultValue: DateTime.MinValue);

            migrationBuilder.AddColumn<int>(
                name: "clanInfo",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Clans",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClanName = table.Column<string>(nullable: true),
                    guildId = table.Column<ulong>(nullable: false),
                    OwnerId = table.Column<ulong>(nullable: false),
                    ClanMoney = table.Column<ulong>(nullable: false),
                    ClanSlots = table.Column<ulong>(nullable: false),
                    LogoUrl = table.Column<string>(nullable: true),
                    ClanRole = table.Column<ulong>(nullable: false),
                    DateOfCreate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RG_Item",
                columns: table => new
                {
                    itemid = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemName = table.Column<string>(nullable: true),
                    startprice = table.Column<ulong>(nullable: false),
                    startprestije = table.Column<ulong>(nullable: false),
                    countTrade = table.Column<ulong>(nullable: false),
                    userid = table.Column<ulong>(nullable: false),
                    guildid = table.Column<ulong>(nullable: false),
                    traded = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RG_Item", x => x.itemid);
                });

            migrationBuilder.CreateTable(
                name: "RG_Profile",
                columns: table => new
                {
                    id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    userid = table.Column<ulong>(nullable: false),
                    guildid = table.Column<ulong>(nullable: false),
                    money = table.Column<long>(nullable: false),
                    StudyNowid = table.Column<ulong>(nullable: false),
                    DaysStudy = table.Column<ushort>(nullable: false),
                    LastStudy = table.Column<DateTime>(nullable: false),
                    workid = table.Column<ulong>(nullable: false),
                    workStreak = table.Column<ulong>(nullable: false),
                    LastWork = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RG_Profile", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "RG_Study",
                columns: table => new
                {
                    studyid = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    studyName = table.Column<string>(nullable: true),
                    StudyMoney = table.Column<ulong>(nullable: false),
                    DayStudying = table.Column<ushort>(nullable: false),
                    Invise = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RG_Study", x => x.studyid);
                });

            migrationBuilder.CreateTable(
                name: "RG_Studys",
                columns: table => new
                {
                    id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    studyid = table.Column<ulong>(nullable: false),
                    userid = table.Column<ulong>(nullable: false),
                    guildid = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RG_Studys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "RG_Work",
                columns: table => new
                {
                    workid = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    workName = table.Column<string>(nullable: true),
                    studyid = table.Column<ulong>(nullable: false),
                    money = table.Column<ulong>(nullable: false),
                    Invise = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RG_Work", x => x.workid);
                });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {


        }
    }
}
