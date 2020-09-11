﻿// <auto-generated />
using System;
using DarlingBotNet.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DarlingBotNet.Migrations
{
    [DbContext(typeof(DBcontext))]
    partial class DBcontextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.6");

            modelBuilder.Entity("DarlingBotNet.DataBase.Channels", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("BadWordString")
                        .HasColumnType("TEXT");

                    b.Property<bool>("GiveXP")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("InviteMessage")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("SendBadWord")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("SendCaps")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("SendUrl")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("SendUrlImage")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Spaming")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("UseCommand")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("UseRPcommand")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("channelid")
                        .HasColumnType("INTEGER");

                    b.Property<string>("csUrlWhiteListString")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("guildid")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.Clans", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ClanMoney")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClanName")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("ClanRole")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ClanSlots")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateOfCreate")
                        .HasColumnType("TEXT");

                    b.Property<string>("LogoUrl")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("OwnerId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("guildId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Clans");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.EmoteClick", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("channelid")
                        .HasColumnType("INTEGER");

                    b.Property<string>("emote")
                        .HasColumnType("TEXT");

                    b.Property<bool>("get")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("guildid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("messageid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("roleid")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("EmoteClick");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.Guilds", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CommandInviseString")
                        .HasColumnType("TEXT");

                    b.Property<bool>("GiveXPnextChannel")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("LeaveChannel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LeaveMessage")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Leaved")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("MafiaWaitChannel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Prefix")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("PrivateChannelID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("RaidMuted")
                        .HasColumnType("TEXT");

                    b.Property<bool>("RaidStop")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("RaidTime")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("RaidUserCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("ViolationSystem")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("WelcomeChannel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WelcomeDMmessage")
                        .HasColumnType("TEXT");

                    b.Property<bool>("WelcomeDMuser")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WelcomeMessage")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("WelcomeRole")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("banchannel")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("chatmuterole")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("guildid")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("inviteMessages")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("joinchannel")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("kickchannel")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("leftchannel")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("mesdelchannel")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("meseditchannel")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("unbanchannel")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("voiceUserActions")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("voicemuterole")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.LVLROLES", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<uint>("countlvl")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("guildid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("roleid")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("LVLROLES");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.PrivateChannels", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("channelid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("guildid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("userid")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("PrivateChannels");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.RussiaGame.RussiaGame_Item", b =>
                {
                    b.Property<ulong>("itemid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ItemName")
                        .HasColumnType("TEXT");

                    b.Property<int>("ItemPropertieses")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ItemTypes")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("countTrade")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("guildid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("startprestije")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("startprice")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("traded")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("userid")
                        .HasColumnType("INTEGER");

                    b.HasKey("itemid");

                    b.ToTable("RG_Item");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.RussiaGame.RussiaGame_Study", b =>
                {
                    b.Property<ulong>("studyid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("DayStudying")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Invise")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("StudyMoney")
                        .HasColumnType("INTEGER");

                    b.Property<string>("studyName")
                        .HasColumnType("TEXT");

                    b.HasKey("studyid");

                    b.ToTable("RG_Study");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.RussiaGame.RussiaGame_Studys", b =>
                {
                    b.Property<ulong>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Proffesion_Study")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("guildid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("studyid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("userid")
                        .HasColumnType("INTEGER");

                    b.HasKey("id");

                    b.ToTable("RG_Studys");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.RussiaGame.RussiaGame_Work", b =>
                {
                    b.Property<ulong>("workid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Invise")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("money")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("studyid")
                        .HasColumnType("INTEGER");

                    b.Property<string>("workName")
                        .HasColumnType("TEXT");

                    b.HasKey("workid");

                    b.ToTable("RG_Work");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.RussiaGame_Profile", b =>
                {
                    b.Property<ulong>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("DaysStudy")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastStudy")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastWork")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("StudyNowid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("guildid")
                        .HasColumnType("INTEGER");

                    b.Property<long>("money")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("userid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("workStreak")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("workid")
                        .HasColumnType("INTEGER");

                    b.HasKey("id");

                    b.ToTable("RG_Profile");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.TempUser", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Reason")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ToTime")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("guildid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("userId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("TempUser");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.Users", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Daily")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Leaved")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Streak")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("XP")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ZeroCoin")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("clanId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("clanInfo")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("countwarns")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("guildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("marryedid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("userid")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.Warns", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("CountWarn")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ReportWarn")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("guildid")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Warns");
                });
#pragma warning restore 612, 618
        }
    }
}
