using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.Services.Sys;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DarlingBotNet.Services.Sys
{
    public class Loadingdb
    {
        private readonly DiscordSocketClient _discord;
        public static bool loading = false;
        public static IMemoryCache cache = null;

        public Loadingdb(DiscordSocketClient discord)
        {
            _discord = discord;

        } // Подключение компонентов

        static Stopwatch sw = new Stopwatch();


        public static void GuildCheck(DiscordSocketClient _discord, IServiceProvider services)
        {
            cache = (IMemoryCache)services.GetService(typeof(IMemoryCache));
            sw.Start();
            using (var DBcontext = new DBcontext())
            {
                foreach (var Guild in _discord.Guilds)
                {
                    var glds = DBcontext.Guilds.FirstOrDefault(x => x.guildid == Guild.Id);
                    if (glds == null || glds.Leaved)
                        GuildCreate(Guild);
                } // Проверка Гильдий которые есть в боте но нету в базе


                foreach (var glds in DBcontext.Guilds)
                {
                    if (_discord.GetGuild(glds.guildid) == null)
                        GuildDelete(glds);
                } // Проверка гильдий которые удалили бота во время его офлайна
            }
            ChannelChecks(_discord);
        }

        public static async void ChannelChecks(DiscordSocketClient _discord)
        {
            using (var DBcontext = new DBcontext())
            {
                var Guilds = DBcontext.Guilds.AsQueryable().Where(x => !x.Leaved);
                foreach (var Guild in Guilds)
                {
                    var glds = _discord.GetGuild(Guild.guildid);
                    var ChannelsDelete = DBcontext.Channels.AsQueryable().Where(x => x.guildid == Guild.guildid).AsEnumerable().Where(x => glds.GetTextChannel(x.channelid) == null);
                    foreach (var channel in ChannelsDelete)
                    {
                        if (Guild.banchannel == channel.Id) Guild.banchannel = 0;
                        if (Guild.unbanchannel == channel.Id) Guild.unbanchannel = 0;
                        if (Guild.WelcomeChannel == channel.Id) Guild.WelcomeChannel = 0;
                        if (Guild.LeaveChannel == channel.Id) Guild.LeaveChannel = 0;
                        if (Guild.joinchannel == channel.Id) Guild.joinchannel = 0;
                        if (Guild.leftchannel == channel.Id) Guild.leftchannel = 0;
                        if (Guild.mesdelchannel == channel.Id) Guild.mesdelchannel = 0;
                        if (Guild.meseditchannel == channel.Id) Guild.meseditchannel = 0;
                        if (Guild.voiceUserActions == channel.Id) Guild.voiceUserActions = 0;
                    }

                    if (glds.GetRole(Guild.WelcomeRole) == null) Guild.WelcomeRole = 0;
                    if (glds.GetRole(Guild.chatmuterole) == null) Guild.chatmuterole = 0;
                    if (glds.GetRole(Guild.voicemuterole) == null) Guild.voicemuterole = 0;

                    var Emoteclickes = DBcontext.EmoteClick.AsQueryable().Where(x => x.guildid == Guild.guildid).AsEnumerable().Where(x => ChannelsDelete.Where(x => x.channelid == x.channelid).Count() > 0 || glds.GetRole(x.roleid) == null || glds.Emotes.Where(z => z.Name == x.emote) == null);

                    var chnlcrt = glds.TextChannels.Where(x => DBcontext.Channels.FirstOrDefault(z => z.channelid == x.Id && z.guildid == x.Guild.Id) == null);
                    if (chnlcrt.Count() > 0)
                        await CreateChannelRange(chnlcrt);

                    DBcontext.Channels.RemoveRange(ChannelsDelete);
                    DBcontext.EmoteClick.RemoveRange(Emoteclickes);
                    DBcontext.Guilds.Update(Guild);
                    await DBcontext.SaveChangesAsync();
                }
                OtherCheck(_discord, Guilds);
            }
        }

        public static async void OtherCheck(DiscordSocketClient _discord,IEnumerable<Guilds> Guilds)
        {
            using (var DBcontext = new DBcontext())
            {
                foreach (var Guild in Guilds)
                {
                    var glds = _discord.GetGuild(Guild.guildid); // Выдача гильдии
                    var LVLROLE = DBcontext.LVLROLES.AsQueryable().Where(x => x.guildid == Guild.guildid).AsEnumerable().Where(x => glds.GetRole(x.roleid) == null); // Выдача недействительных Уровневых ролей

                    var UsersLeave = DBcontext.Users.AsQueryable().Where(x => x.guildId == Guild.guildid);
                    foreach (var user in UsersLeave)
                    {
                        if (glds.GetUser(user.userid) != null) user.Leaved = false;
                        else user.Leaved = true;
                    }

                    DBcontext.LVLROLES.RemoveRange(LVLROLE);
                    DBcontext.Users.UpdateRange(UsersLeave);
                    await DBcontext.SaveChangesAsync();
                    await new Privates().CheckPrivate(glds); // Проверка приваток
                    CheckTempUser(Guild, glds);
                }
                sw.Stop();
                Console.WriteLine(sw.Elapsed);
                sw.Reset();
                loading = true;
            }
        }

        private static async void CheckTempUser(Guilds glds, SocketGuild guild)
        {
            using (var DBcontext = new DBcontext())
            {
                var users = DBcontext.TempUser.AsQueryable().Where(x => x.guildid == glds.guildid);
                foreach (var user in users)
                    await UserMuteTime(user, guild, glds);
            }
        } // Проверка активных нарушений

        private static async Task UserMuteTime(TempUser user, SocketGuild guild, Guilds gld)
        {
            using (var DBcontext = new DBcontext())
            {
                if (user.ToTime < DateTime.Now)
                    await Task.Delay(user.ToTime.Millisecond);

                var usr = guild.GetUser(user.userId);

                if (user.Reason.Contains("tban"))
                {
                    if(guild.GetBanAsync(usr) != null)
                        await guild.RemoveBanAsync(usr);
                }
                else
                {
                    var cmute = guild.GetRole(gld.chatmuterole);
                    var vmute = guild.GetRole(gld.voicemuterole);
                    try
                    {

                        if (cmute != null && usr != null && usr.Roles.Contains(cmute))
                            await usr.RemoveRoleAsync(cmute);
                        if (vmute != null && usr != null && usr.Roles.Contains(vmute))
                            await usr.RemoveRoleAsync(vmute);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"257 - роли не удалились\n{guild.Name}\n{usr}");
                    }
                }
                var UserCheckMute = DBcontext.TempUser.FirstOrDefault(x => x.guildid == user.guildid && x.userId == user.userId && x.ToTime == user.ToTime);
                if (UserCheckMute != null)
                {
                    DBcontext.TempUser.Remove(UserCheckMute);
                    await DBcontext.SaveChangesAsync();
                }
            }

        }

        public static async Task<Guilds> GuildCreate(SocketGuild Guild)
        {
            using (var DBcontext = new DBcontext())
            {
                var glds = DBcontext.Guilds.FirstOrDefault(x => x.guildid == Guild.Id);
                if (glds == null)
                {
                    glds = new Guilds() { guildid = Guild.Id, GiveXPnextChannel = true, Prefix = BotSettings.Prefix };
                    DBcontext.Guilds.Add(glds);
                }
                else if (glds.Leaved)
                {
                    glds.Leaved = false;
                    DBcontext.Guilds.Update(glds);
                }

                DBcontext.SaveChanges();

                await CreateChannelRange(Guild.TextChannels);
                return glds;
            }
        }

        public static async void GuildDelete(Guilds Guild)
        {
            using (var DBcontext = new DBcontext())
            {
                DBcontext.Channels.RemoveRange(DBcontext.Channels.AsQueryable().Where(x => x.guildid == Guild.guildid));
                DBcontext.LVLROLES.RemoveRange(DBcontext.LVLROLES.AsQueryable().Where(x => x.guildid == Guild.guildid));
                DBcontext.EmoteClick.RemoveRange(DBcontext.EmoteClick.AsQueryable().Where(x => x.guildid == Guild.guildid));
                DBcontext.PrivateChannels.RemoveRange(DBcontext.PrivateChannels.AsQueryable().Where(x => x.guildid == Guild.guildid));
                DBcontext.Warns.RemoveRange(DBcontext.Warns.AsQueryable().Where(x => x.guildid == Guild.guildid));
                DBcontext.TempUser.RemoveRange(DBcontext.TempUser.AsQueryable().Where(x => x.guildid == Guild.guildid));

                var UsersLeave = DBcontext.Users.AsQueryable().Where(x => x.guildId == Guild.guildid);
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
                var givexp = cache.GetOrCreateGuldsCache(Channels.First().Guild.Id).GiveXPnextChannel;
                var lists = new List<Channels>();
                foreach (var TextChannel in Channels)
                {
                    lists.Add(new Channels() { guildid = TextChannel.Guild.Id, channelid = TextChannel.Id, GiveXP = givexp, UseCommand = true });
                }
                DBcontext.Channels.AddRange(lists);
                await DBcontext.SaveChangesAsync();
            }
        }
    }
}
