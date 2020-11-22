using DarlingBotNet.DataBase;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DarlingBotNet.Services.Sys
{
    public class ScanningDataBase
    {
        private readonly DiscordSocketClient _discord;
        public static bool loading = false;
        public static IMemoryCache _cache = null;

        public ScanningDataBase(DiscordSocketClient discord)
        {
            _discord = discord;

        } // Подключение компонентов

        public async static void GuildCheck(DiscordSocketClient _discord, IServiceProvider services)
        {
            _cache = (IMemoryCache)services.GetService(typeof(IMemoryCache));
            var sw = new Stopwatch();
            sw.Start();
            using (var DBcontext = new DBcontext())
            {
                foreach (var glds in DBcontext.Guilds)
                {
                    if (_discord.GetGuild(glds.GuildId) == null)
                        GuildDelete(glds);
                } // Проверка гильдий которые удалили бота во время его офлайна

                foreach (var Guild in _discord.Guilds)
                {
                    var GuildDB = DBcontext.Guilds.FirstOrDefault(x => x.GuildId == Guild.Id);
                    if (GuildDB == null || GuildDB.Leaved)
                        await GuildCreate(Guild, GuildDB);
                    else
                        CheckNextValidGuild(_discord, GuildDB,Guild);

                } // Проверка Гильдий которые есть в боте но нету в базе
                
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            loading = true;
        }

        public static async void CheckNextValidGuild(DiscordSocketClient _discord,Guilds GuildDB,SocketGuild Guild)
        {
            using (var DBcontext = new DBcontext())
            {
                var ChannelsDelete = DBcontext.Channels.AsQueryable().Where(x => x.GuildId == GuildDB.GuildId).AsEnumerable().Where(x => Guild.GetTextChannel(x.ChannelId) == null);
                var TasksDelete = DBcontext.Tasks.AsQueryable().Where(x => x.GuildId == GuildDB.GuildId).AsEnumerable().Where(x => Guild.GetTextChannel(x.ChannelId) == null || x.Times < DateTime.UtcNow);


                var Emoteclickes = DBcontext.EmoteClick.AsQueryable().Where(x => x.GuildId == GuildDB.GuildId).AsEnumerable().Where(x => ChannelsDelete.Where(x => x.ChannelId == x.ChannelId).Count() > 0 || Guild.GetRole(x.RoleId) == null || Guild.Emotes.Where(z => z.Name == x.emote) == null);



                var LVLROLES = DBcontext.LVLROLES.AsQueryable().Where(x => x.GuildId == GuildDB.GuildId); // Удаление недействительных Уровневых ролей
                foreach (var LVLROLE in LVLROLES)
                {
                    if (Guild.GetRole(LVLROLE.RoleId) == null)
                        DBcontext.LVLROLES.Remove(LVLROLE);
                }


                var Roles = DBcontext.RoleSwaps.AsQueryable().Where(x => x.GuildId == GuildDB.GuildId); // Удаление недействительных Ролей на продажу
                foreach (var Role in Roles)
                {
                    if (Guild.GetRole(Role.RoleId) == null)
                        DBcontext.RoleSwaps.Remove(Role);
                }


                var UsersLeave = DBcontext.Users.AsQueryable().Where(x => x.GuildId == GuildDB.GuildId); // Отключение вышедших пользователей
                foreach (var user in UsersLeave)
                {
                    if (Guild.GetUser(user.UserId) != null)
                        user.Leaved = false;
                    else
                        user.Leaved = true;
                }


                var chnlcrt = Guild.TextChannels.Where(x => DBcontext.Channels.FirstOrDefault(z => z.ChannelId == x.Id && z.GuildId == x.Guild.Id) == null);
                if (chnlcrt.Count() > 0)
                    await CreateChannelRange(chnlcrt);

                DBcontext.Channels.RemoveRange(ChannelsDelete);
                DBcontext.EmoteClick.RemoveRange(Emoteclickes);
                DBcontext.Tasks.RemoveRange(TasksDelete);
                DBcontext.Users.UpdateRange(UsersLeave);
                DBcontext.Guilds.Update(GuildDB);
                DBcontext.SaveChanges();
                await new Privates().CheckPrivate(Guild); // Проверка приваток
                CheckTempUser(GuildDB, Guild);
                
            }
        }


        //public static async void ChannelChecks(DiscordSocketClient _discord)
        //{
        //    using (var DBcontext = new DBcontext())
        //    {
        //        var Guilds = DBcontext.Guilds.AsQueryable().Where(x => !x.Leaved);
        //        foreach (var Guild in Guilds)
        //        {
        //            var glds = _discord.GetGuild(Guild.GuildId);
        //            var ChannelsDelete = DBcontext.Channels.AsQueryable().Where(x => x.GuildId == Guild.GuildId).AsEnumerable().Where(x => glds.GetTextChannel(x.ChannelId) == null);
        //            var TasksDelete = DBcontext.Tasks.AsQueryable().Where(x => x.GuildId == Guild.GuildId).AsEnumerable().Where(x => glds.GetTextChannel(x.ChannelId) == null);
        //            foreach (var channel in ChannelsDelete)
        //            {
        //                if (Guild.banchannel == channel.Id) Guild.banchannel = 0;
        //                if (Guild.unbanchannel == channel.Id) Guild.unbanchannel = 0;
        //                if (Guild.WelcomeChannel == channel.Id) Guild.WelcomeChannel = 0;
        //                if (Guild.LeaveChannel == channel.Id) Guild.LeaveChannel = 0;
        //                if (Guild.joinchannel == channel.Id) Guild.joinchannel = 0;
        //                if (Guild.leftchannel == channel.Id) Guild.leftchannel = 0;
        //                if (Guild.mesdelchannel == channel.Id) Guild.mesdelchannel = 0;
        //                if (Guild.meseditchannel == channel.Id) Guild.meseditchannel = 0;
        //                if (Guild.voiceUserActions == channel.Id) Guild.voiceUserActions = 0;
        //            }


        //            var Emoteclickes = DBcontext.EmoteClick.AsQueryable().Where(x => x.GuildId == Guild.GuildId).AsEnumerable().Where(x => ChannelsDelete.Where(x => x.ChannelId == x.ChannelId).Count() > 0 || glds.GetRole(x.RoleId) == null || glds.Emotes.Where(z => z.Name == x.emote) == null);

        //            var chnlcrt = glds.TextChannels.Where(x => DBcontext.Channels.FirstOrDefault(z => z.ChannelId == x.Id && z.GuildId == x.Guild.Id) == null);
        //            if (chnlcrt.Count() > 0)
        //                await CreateChannelRange(chnlcrt);

        //            DBcontext.Channels.RemoveRange(ChannelsDelete);
        //            DBcontext.EmoteClick.RemoveRange(Emoteclickes);
        //            DBcontext.Tasks.RemoveRange(TasksDelete);
        //            DBcontext.Guilds.Update(Guild);
        //            DBcontext.SaveChanges();
        //        }
        //        OtherCheck(_discord, Guilds);
        //    }
        //}

        //public static async void OtherCheck(DiscordSocketClient _discord, IEnumerable<Guilds> Guilds)
        //{
        //    using (var DBcontext = new DBcontext())
        //    {
        //        foreach (var Guild in Guilds)
        //        {
        //            var glds = _discord.GetGuild(Guild.GuildId); // Выдача гильдии
        //            var LVLROLE = DBcontext.LVLROLES.AsQueryable().Where(x => x.GuildId == Guild.GuildId).AsEnumerable().Where(x => glds.GetRole(x.RoleId) == null); // Выдача недействительных Уровневых ролей
        //            var Roles = DBcontext.RoleSwaps.AsQueryable().Where(x => x.GuildId == Guild.GuildId).AsEnumerable().Where(x => glds.GetRole(x.RoleId) == null); // Выдача недействительных Ролей на продажу

        //            if (glds.GetRole(Guild.WelcomeRole) == null) Guild.WelcomeRole = 0;
        //            if (glds.GetRole(Guild.chatmuterole) == null) Guild.chatmuterole = 0;
        //            if (glds.GetRole(Guild.voicemuterole) == null) Guild.voicemuterole = 0;

        //            var UsersLeave = DBcontext.Users.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
        //            foreach (var user in UsersLeave)
        //            {
        //                if (glds.GetUser(user.UserId) != null)
        //                    user.Leaved = false;
        //                else
        //                    user.Leaved = true;
        //            }

        //            DBcontext.RoleSwaps.RemoveRange(Roles);
        //            DBcontext.LVLROLES.RemoveRange(LVLROLE);
        //            DBcontext.Users.UpdateRange(UsersLeave);
        //            DBcontext.Guilds.Update(Guild);
        //            DBcontext.SaveChanges();
        //            await new Privates().CheckPrivate(glds); // Проверка приваток
        //            CheckTempUser(Guild, glds);
        //        }
        //        sw.Stop();
        //        Console.WriteLine(sw.Elapsed);
        //        sw.Reset();
        //        loading = true;
        //    }
        //}

        private static async void CheckTempUser(Guilds glds, SocketGuild guild)
        {
            using (var DBcontext = new DBcontext())
            {
                var TempUsers = DBcontext.TempUser.AsQueryable().Where(x => x.GuildId == glds.GuildId);
                foreach (var TempUser in TempUsers)
                {
                    await UserMuteTime(TempUser, guild, glds);
                }
            }
        } // Проверка активных нарушений

        private static async Task UserMuteTime(TempUser user, SocketGuild guild, Guilds gld)
        {
            using (var DBcontext = new DBcontext())
            {
                if (user.ToTime > DateTime.Now)
                    await Task.Delay((int)(user.ToTime - DateTime.Now).TotalMilliseconds);

                var usr = guild.GetUser(user.UserId);

                if (user.Reason == Warns.ReportType.tban)
                {
                    try
                    {
                        await guild.RemoveBanAsync(usr);
                    }
                    catch (Exception)
                    {
                    }
                    
                }
                else
                {
                    if (usr != null)
                    {
                        await OtherSettings.CheckRoleValid(usr, gld.chatmuterole, true);
                        await OtherSettings.CheckRoleValid(usr, gld.voicemuterole, true);
                    }

                }
                var UserCheckMute = DBcontext.TempUser.FirstOrDefault(x => x.GuildId == user.GuildId && x.UserId == user.UserId && x.ToTime == user.ToTime);
                if (UserCheckMute != null)
                {
                    DBcontext.TempUser.Remove(UserCheckMute);
                    await DBcontext.SaveChangesAsync();
                }
            }

        }

        public static async Task<Guilds> GuildCreate(SocketGuild Guild, Guilds GuildDB = null)
        {
            using (var DBcontext = new DBcontext())
            {
                if (GuildDB == null)
                {
                    GuildDB = new Guilds() { GuildId = Guild.Id, GiveXPnextChannel = true, Prefix = BotSettings.Prefix,OwnerId = Guild.OwnerId };
                    DBcontext.Guilds.Add(GuildDB);
                }
                else if (GuildDB.Leaved)
                {
                    GuildDB.Leaved = false;
                    DBcontext.Guilds.Update(GuildDB);
                }

                await DBcontext.SaveChangesAsync();

                await CreateChannelRange(Guild.TextChannels);
                return GuildDB;
            }
        }

        public static async void GuildDelete(Guilds Guild)
        {
            using (var DBcontext = new DBcontext())
            {
                var UserBoost = DBcontext.DarlingBoost.FirstOrDefault(x => x.UserId == Guild.OwnerId);
                if (UserBoost != null && UserBoost.Ends < DateTime.Now)
                {
                    var DeleteChannels = DBcontext.Channels.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    var DeleteLVLROLES = DBcontext.LVLROLES.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    var DeleteEmoteClick = DBcontext.EmoteClick.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    var DeletePrivateChannels = DBcontext.PrivateChannels.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    var DeleteWarns = DBcontext.Warns.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    var DeleteTempUsers = DBcontext.TempUser.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    var DeleteRoles = DBcontext.RoleSwaps.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    var DeleteTasks = DBcontext.Tasks.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    var DeleteClans = DBcontext.Clans.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    DBcontext.Channels.RemoveRange(DeleteChannels);
                    DBcontext.LVLROLES.RemoveRange(DeleteLVLROLES);
                    DBcontext.EmoteClick.RemoveRange(DeleteEmoteClick);
                    DBcontext.PrivateChannels.RemoveRange(DeletePrivateChannels);
                    DBcontext.Warns.RemoveRange(DeleteWarns);
                    DBcontext.TempUser.RemoveRange(DeleteTempUsers);
                    DBcontext.RoleSwaps.RemoveRange(DeleteRoles);
                    DBcontext.Tasks.RemoveRange(DeleteTasks);
                    DBcontext.Clans.RemoveRange(DeleteClans);
                }
                var UsersLeave = DBcontext.Users.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                if (UsersLeave.Count() > 0)
                {
                    foreach (var user in UsersLeave)
                        user.Leaved = true;

                    DBcontext.Users.UpdateRange(UsersLeave);
                }

                Guild.Leaved = true;
                DBcontext.Guilds.Update(Guild);
                await DBcontext.SaveChangesAsync();
            }
        }

        public static async Task CreateChannelRange(IEnumerable<SocketTextChannel> Channels)
        {
            using (var DBcontext = new DBcontext())
            {
                var GuildGiveXPnextChannel = DBcontext.Guilds.FirstOrDefault(x => x.GuildId == Channels.First().Guild.Id).GiveXPnextChannel;
                var lists = new List<Channels>();
                foreach (var TextChannel in Channels)
                {
                    lists.Add(new Channels() { GuildId = TextChannel.Guild.Id, ChannelId = TextChannel.Id, GiveXP = GuildGiveXPnextChannel, UseCommand = true });
                }
                DBcontext.Channels.AddRange(lists);
                await DBcontext.SaveChangesAsync();
            }
        }
    }
}
