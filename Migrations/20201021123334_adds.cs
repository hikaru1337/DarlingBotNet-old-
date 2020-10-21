using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DarlingBotNet.Migrations
{
    public partial class adds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    channelid = table.Column<ulong>(nullable: false),
                    guildid = table.Column<ulong>(nullable: false),
                    UseCommand = table.Column<bool>(nullable: false),
                    UseRPcommand = table.Column<bool>(nullable: false),
                    GiveXP = table.Column<bool>(nullable: false),
                    DelUrl = table.Column<bool>(nullable: false),
                    DelUrlImage = table.Column<bool>(nullable: false),
                    DelCaps = table.Column<bool>(nullable: false),
                    Spaming = table.Column<bool>(nullable: false),
                    SendBadWord = table.Column<bool>(nullable: false),
                    BadWordString = table.Column<string>(nullable: true),
                    csUrlWhiteListString = table.Column<string>(nullable: true),
                    InviteMessage = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clans",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClanName = table.Column<string>(nullable: true),
                    guildId = table.Column<ulong>(nullable: false),
                    OwnerId = table.Column<ulong>(nullable: false),
                    ClanMoney = table.Column<long>(nullable: false),
                    ClanSlots = table.Column<uint>(nullable: false),
                    LogoUrl = table.Column<string>(nullable: true),
                    ClanRole = table.Column<ulong>(nullable: false),
                    DateOfCreate = table.Column<DateTime>(nullable: false),
                    LastClanSlotPays = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmoteClick",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guildid = table.Column<ulong>(nullable: false),
                    emote = table.Column<string>(nullable: true),
                    messageid = table.Column<ulong>(nullable: false),
                    channelid = table.Column<ulong>(nullable: false),
                    roleid = table.Column<ulong>(nullable: false),
                    get = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmoteClick", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guildid = table.Column<ulong>(nullable: false),
                    Leaved = table.Column<bool>(nullable: false),
                    Prefix = table.Column<string>(nullable: true),
                    chatmuterole = table.Column<ulong>(nullable: false),
                    voicemuterole = table.Column<ulong>(nullable: false),
                    PrivateChannelID = table.Column<ulong>(nullable: false),
                    CommandInviseString = table.Column<string>(nullable: true),
                    GiveXPnextChannel = table.Column<bool>(nullable: false),
                    ViolationSystem = table.Column<byte>(nullable: false),
                    banchannel = table.Column<ulong>(nullable: false),
                    unbanchannel = table.Column<ulong>(nullable: false),
                    kickchannel = table.Column<ulong>(nullable: false),
                    leftchannel = table.Column<ulong>(nullable: false),
                    joinchannel = table.Column<ulong>(nullable: false),
                    meseditchannel = table.Column<ulong>(nullable: false),
                    mesdelchannel = table.Column<ulong>(nullable: false),
                    voiceUserActions = table.Column<ulong>(nullable: false),
                    inviteMessages = table.Column<bool>(nullable: false),
                    WelcomeMessage = table.Column<string>(nullable: true),
                    WelcomeDMmessage = table.Column<string>(nullable: true),
                    WelcomeDMuser = table.Column<bool>(nullable: false),
                    WelcomeChannel = table.Column<ulong>(nullable: false),
                    WelcomeRole = table.Column<ulong>(nullable: false),
                    LeaveMessage = table.Column<string>(nullable: true),
                    LeaveChannel = table.Column<ulong>(nullable: false),
                    RaidStop = table.Column<bool>(nullable: false),
                    RaidTime = table.Column<uint>(nullable: false),
                    RaidUserCount = table.Column<uint>(nullable: false),
                    RaidMuted = table.Column<DateTime>(nullable: false),
                    LimitRoleUserClan = table.Column<uint>(nullable: false),
                    PriceBuyRole = table.Column<ulong>(nullable: false),
                    GiveClanRoles = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LVLROLES",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guildid = table.Column<ulong>(nullable: false),
                    roleid = table.Column<ulong>(nullable: false),
                    countlvl = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LVLROLES", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrivateChannels",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guildid = table.Column<ulong>(nullable: false),
                    userid = table.Column<ulong>(nullable: false),
                    channelid = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateChannels", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GuildId = table.Column<ulong>(nullable: false),
                    ChannelId = table.Column<ulong>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    Times = table.Column<DateTime>(nullable: false),
                    Repeat = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TempUser",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guildid = table.Column<ulong>(nullable: false),
                    userId = table.Column<ulong>(nullable: false),
                    ToTime = table.Column<DateTime>(nullable: false),
                    Reason = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    userid = table.Column<ulong>(nullable: false),
                    Leaved = table.Column<bool>(nullable: false),
                    guildId = table.Column<ulong>(nullable: false),
                    XP = table.Column<ulong>(nullable: false),
                    ZeroCoin = table.Column<ulong>(nullable: false),
                    Daily = table.Column<DateTime>(nullable: false),
                    Streak = table.Column<ulong>(nullable: false),
                    countwarns = table.Column<uint>(nullable: false),
                    marryedid = table.Column<ulong>(nullable: false),
                    clanId = table.Column<ulong>(nullable: false),
                    clanInfo = table.Column<int>(nullable: false),
                    Bank = table.Column<ulong>(nullable: false),
                    BankTimer = table.Column<DateTime>(nullable: false),
                    BankLastTransit = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warns",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guildid = table.Column<ulong>(nullable: false),
                    CountWarn = table.Column<byte>(nullable: false),
                    ReportWarn = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warns", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "Clans");

            migrationBuilder.DropTable(
                name: "EmoteClick");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "LVLROLES");

            migrationBuilder.DropTable(
                name: "PrivateChannels");

            migrationBuilder.DropTable(
                name: "RG_Item");

            migrationBuilder.DropTable(
                name: "RG_Profile");

            migrationBuilder.DropTable(
                name: "RG_Study");

            migrationBuilder.DropTable(
                name: "RG_Studys");

            migrationBuilder.DropTable(
                name: "RG_Work");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "TempUser");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Warns");
        }
    }
}
