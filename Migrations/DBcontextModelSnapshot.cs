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

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DelCaps")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DelUrl")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DelUrlImage")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("GiveXP")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("InviteMessage")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("SendBadWord")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Spaming")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("UseCommand")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("UseRPcommand")
                        .HasColumnType("INTEGER");

                    b.Property<string>("csUrlWhiteListString")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.Clans", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("ClanMoney")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClanName")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("ClanRole")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("ClanSlots")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateOfCreate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastClanSlotPays")
                        .HasColumnType("TEXT");

                    b.Property<string>("LogoUrl")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("OwnerId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Clans");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.Database.Models.QiwiTransactions", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("discord_id")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("invoice_ammount")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("invoice_date_add")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("QiwiTransaction");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.Database.Models.Tasks", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Message")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Repeat")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Times")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.EmoteClick", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("MessageId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("emote")
                        .HasColumnType("TEXT");

                    b.Property<bool>("get")
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

                    b.Property<bool>("GiveClanRoles")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("GiveXPnextChannel")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("LeaveChannel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LeaveMessage")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Leaved")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("LimitRoleUserClan")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("OwnerId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Prefix")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("PriceBuyRole")
                        .HasColumnType("INTEGER");

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

                    b.Property<int>("VS")
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

                    b.Property<ulong>("CountLvl")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("LVLROLES");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.PrivateChannels", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("PrivateChannels");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.RoleSwaps", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Price")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("RoleSwaps");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.TempUser", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Reason")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ToTime")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("TempUser");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.Users", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Bank")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("BankLastTransit")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("BankTimer")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("ClanId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Daily")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Leaved")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("RealCoin")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Streak")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("XP")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ZeroCoin")
                        .HasColumnType("INTEGER");

                    b.Property<int>("clanInfo")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("countwarns")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("marryedid")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DarlingBotNet.DataBase.Warns", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<byte>("CountWarn")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("MinutesWarn")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ReportTypes")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Warns");
                });

            modelBuilder.Entity("DarlingBotNet.Modules.DarlingBoost", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Ends")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("Streak")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("DarlingBoost");
                });
#pragma warning restore 612, 618
        }
    }
}
