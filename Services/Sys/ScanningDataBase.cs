using DarlingBotNet.DataBase;
using Discord;
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
        public static Stopwatch sw = new Stopwatch();

        public ScanningDataBase(DiscordSocketClient discord)
        {
            _discord = discord;

        } // Подключение компонентов

        public async static void GuildCheck(DiscordSocketClient _discord, IServiceProvider services)
        {
            _cache = (IMemoryCache)services.GetService(typeof(IMemoryCache));
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

                } // Проверка Гильдий которые есть в боте но нету в базе
            }
            //ChannelChecks(_discord);
            Console.WriteLine("Проверка гильдий - " + sw.Elapsed);
        }

        public static async Task<IEnumerable<Guilds>> ChannelChecks(DiscordSocketClient _discord)
        {
            using (var DBcontext = new DBcontext())
            {
                var Guilds = DBcontext.Guilds.AsQueryable().Where(x => !x.Leaved).ToList();
                foreach (var GuildDB in Guilds)
                {
                    var Guild = _discord.GetGuild(GuildDB.GuildId);
                    var Channels = DBcontext.Channels.AsQueryable().Where(x => x.GuildId == GuildDB.GuildId);
                    var ChannelsDelete = Channels.AsEnumerable().Where(x => Guild.GetTextChannel(x.ChannelId) == null);
                    var TasksDelete = DBcontext.Tasks.AsQueryable().Where(x => x.GuildId == GuildDB.GuildId).AsEnumerable().Where(x => (ChannelsDelete.FirstOrDefault(z => z.ChannelId == x.ChannelId) == null || x.Times < DateTime.Now.AddHours(3)) && !x.Repeat );
                    //var Emoteclickes = DBcontext.EmoteClick.AsQueryable().Where(x => x.GuildId == GuildDB.GuildId);

                    //foreach (var emoteclick in Emoteclickes)
                    //{
                    //    bool delete = false;
                    //    if (Guild.GetRole(emoteclick.RoleId) == null)
                    //        delete = true;
                    //    else if (Guild.Emotes.FirstOrDefault(z => z.Name == emoteclick.emote) == null)
                    //        delete = true;
                    //    else if (ChannelsDelete.FirstOrDefault(x => x.ChannelId == emoteclick.ChannelId) != null)
                    //        delete = true;

                    //    if (delete)
                    //        DBcontext.EmoteClick.Remove(emoteclick);
                    //}



                    var chnlcrt = Guild.TextChannels.Where(x => Channels.FirstOrDefault(z => z.ChannelId == x.Id) == null);
                    if (chnlcrt.Count() > 0)
                        await CreateChannelRange(chnlcrt);

                    DBcontext.Channels.RemoveRange(ChannelsDelete);
                    DBcontext.Tasks.RemoveRange(TasksDelete);
                    DBcontext.Guilds.Update(GuildDB);
                }
                DBcontext.SaveChanges();
                //OtherCheck(_discord, Guilds.AsEnumerable());
                Console.WriteLine("Первая проверка Параметров гильдий - " + sw.Elapsed);
                return Guilds;
            }
        }

        public static async void OtherCheck(DiscordSocketClient _discord, IEnumerable<Guilds> Guilds)
        {
            using (var DBcontext = new DBcontext())
            {
                foreach (var GuildDB in Guilds)
                {
                    var Guild = _discord.GetGuild(GuildDB.GuildId);
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

                    DBcontext.Users.UpdateRange(UsersLeave);
                    DBcontext.SaveChanges();
                    ScanningDataBase.CheckTempUser(GuildDB, Guild);
                    await Privates.CheckPrivate(Guild); // Проверка приваток
                }
                Console.WriteLine("Вторая проверка Параметров гильдий - " + sw.Elapsed);
                sw.Stop();
                loading = true;
            }
        }

        public static async void CheckTempUser(Guilds glds, SocketGuild guild)
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
                    var Channels = new Channels {GuildId = Guild.GuildId};

                    DBcontext.Channels.AttachRange(Channels);
                    DBcontext.Channels.RemoveRange(Channels);

                    //var DeleteChannels = DBcontext.Channels.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    //var DeleteLVLROLES = DBcontext.LVLROLES.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    //var DeleteEmoteClick = DBcontext.EmoteClick.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    //var DeletePrivateChannels = DBcontext.PrivateChannels.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    //var DeleteWarns = DBcontext.Warns.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    //var DeleteTempUsers = DBcontext.TempUser.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    //var DeleteRoles = DBcontext.RoleSwaps.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    //var DeleteTasks = DBcontext.Tasks.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    //var DeleteClans = DBcontext.Clans.AsQueryable().Where(x => x.GuildId == Guild.GuildId);
                    //DBcontext.Channels.RemoveRange(DeleteChannels);
                    //DBcontext.LVLROLES.RemoveRange(DeleteLVLROLES);
                    //DBcontext.EmoteClick.RemoveRange(DeleteEmoteClick);
                    //DBcontext.PrivateChannels.RemoveRange(DeletePrivateChannels);
                    //DBcontext.Warns.RemoveRange(DeleteWarns);
                    //DBcontext.TempUser.RemoveRange(DeleteTempUsers);
                    //DBcontext.RoleSwaps.RemoveRange(DeleteRoles);
                    //DBcontext.Tasks.RemoveRange(DeleteTasks);
                    //DBcontext.Clans.RemoveRange(DeleteClans);
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
