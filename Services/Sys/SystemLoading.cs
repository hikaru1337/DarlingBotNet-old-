using DarlingBotNet.DataBase;

using DarlingBotNet.Modules;
using DarlingBotNet.Services.Sys;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DarlingBotNet.Services
{
    public class SystemLoading
    {
        public static string WelcomeText =
        "⚡️ Бот по стандарту использует префикс: **h.**\n" +
        "🔨 нашли баг? Пишите - **{0}bug [описание бага]**\n" +
        "👑 Инструкция бота - https://docs.darlingbot.ru/ \n\n" +
        "🎁Добавить бота на сервер - [КЛИК](https://discord.com/oauth2/authorize?client_id=663381953181122570&scope=bot&permissions=8)\n\n" +
        "Все обновления бота будут выходить тут - [КЛИК](https://docs.darlingbot.ru/obnovleniya)";


        private readonly DiscordSocketClient _discord;
        public static bool loading = false;

        public SystemLoading(DiscordSocketClient discord)
        {
            _discord = discord;

        } // Подключение компонентов


        public static (bool, EmbedBuilder) CheckText(string report)
        {
            bool es = false;
            bool error = false;
            var emb = new EmbedBuilder().WithColor(255, 0, 94);
            try
            {
                if (report.ToLower() == "kick" || report.ToLower() == "ban" || report.ToLower() == "mute")
                    es = true;
                else if (report.ToLower().Substring(0, 4) == "tban" || report.ToLower().Substring(0, 5) == "tmute")
                {
                    int count = 5;
                    if (report.ToLower().Substring(0, 4) == "tban") count = 4;
                        try
                        {
                            if (Convert.ToUInt64(report.ToLower().Substring(count, report.Length - count)) <= 720)
                                es = true;
                            else
                                emb.WithDescription("Время должно быть не больше 720 минут");
                        }
                        catch (Exception)
                        {
                            error = true;
                        }
                }
                else error = true;
            }
            catch (Exception)
            {
                error = true;
            }

            if(error)
                emb.WithDescription("Используйте эти нарушения ban,kick,mute,tmute,tban.")
                    .WithFooter("Инструкция о команде - [инструкция](https://docs.darlingbot.ru/commands/settings-server/system-violation#vystavit-varn-na-servere)");
            
            return (es, emb);
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
                else if(glds.Leaved)
                {
                    glds.Leaved = false;
                    DBcontext.Guilds.Update(glds);
                }
                    

                await DBcontext.SaveChangesAsync();

                await CreateChannelRange(Guild.TextChannels);
                return glds;
            }
        }


        public static async Task CreateChannelRange(IEnumerable<SocketTextChannel> Channels)
        {
            using (var DBcontext = new DBcontext())
            {
                var givexp = DBcontext.Guilds.FirstOrDefault(x => x.guildid == Channels.First().Guild.Id).GiveXPnextChannel;
                var lists = new List<Channels>();
                foreach (var TextChannel in Channels)
                {
                    lists.Add(new Channels() { guildid = TextChannel.Guild.Id, channelid = TextChannel.Id, GiveXP = givexp, UseCommand = true });
                }
                DBcontext.Channels.AddRange(lists);
                await DBcontext.SaveChangesAsync();
            }
        }

        static Stopwatch sw = new Stopwatch();
        public static async Task GuildCheck(DiscordSocketClient _discord,IServiceProvider services)
        {
            service = services;
            sw.Start();
            using (var DBcontext = new DBcontext())
            {
                foreach (var Guild in _discord.Guilds)
                {
                    var glds = DBcontext.Guilds.FirstOrDefault(x => x.guildid == Guild.Id);
                    if (glds == null || glds.Leaved)
                        await GuildCreate(Guild);
                } // Проверка Гильдий которые есть в боте но нету в базе


                foreach (var glds in DBcontext.Guilds)
                {
                    if(_discord.GetGuild(glds.guildid) == null)
                        await GuildDelete(glds);
                } // Проверка гильдий которые удалили бота во время его офлайна
            }
            await ChannelCheck(_discord);
        } // МИГРАЦИЯ, ПРОВЕРКА ГИЛЬДИЙ

        public static async Task GuildDelete(Guilds Guild)
        {
            using (var DBcontext = new DBcontext())
            {
                DBcontext.Channels.RemoveRange(DBcontext.Channels.AsQueryable().Where(x=>x.guildid == Guild.guildid));
                DBcontext.LVLROLES.RemoveRange(DBcontext.LVLROLES.AsQueryable().Where(x => x.guildid == Guild.guildid));
                DBcontext.EmoteClick.RemoveRange(DBcontext.EmoteClick.AsQueryable().Where(x => x.guildid == Guild.guildid));
                DBcontext.PrivateChannels.RemoveRange(DBcontext.PrivateChannels.AsQueryable().Where(x => x.guildid == Guild.guildid));
                DBcontext.Warns.RemoveRange(DBcontext.Warns.AsQueryable().Where(x => x.guildid == Guild.guildid));
                DBcontext.TempUser.RemoveRange(DBcontext.TempUser.AsQueryable().Where(x => x.guildid == Guild.guildid));

                var UsersLeave = DBcontext.Users.AsQueryable().Where(x=>x.guildId == Guild.guildid);
                foreach (var user in UsersLeave) user.Leaved = true;
                DBcontext.Users.UpdateRange(UsersLeave);

                Guild.Leaved = true;
                DBcontext.Guilds.Update(Guild);
                await DBcontext.SaveChangesAsync();
            }
        } // УДАЛЕНИЕ ИНФОРМАЦИИ ГИЛЬДИИ


        public static async Task ChannelCheck(DiscordSocketClient _discord)
        {
            using (var DBcontext = new DBcontext())
            {
                var Guilds = DBcontext.Guilds.AsQueryable().Where(x => !x.Leaved);

                foreach (var Guild in Guilds)
                {
                    var glds = _discord.GetGuild(Guild.guildid); // Выдача гильдии

                    var ChannelsDelete = DBcontext.Channels.AsQueryable().Where(x=> x.guildid == Guild.guildid).AsEnumerable().Where(x=> glds.GetTextChannel(x.channelid) == null); // Удаление недействительных каналов

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

                    var LVLROLE = DBcontext.LVLROLES.AsQueryable().Where(x=> x.guildid == Guild.guildid).AsEnumerable().Where(x => glds.GetRole(x.roleid) == null); // Выдача недействительных Уровневых ролей

                    var chnlcrt = glds.TextChannels.Where(x => DBcontext.Channels.FirstOrDefault(z=> z.channelid == x.Id && z.guildid == x.Guild.Id) == null); // Выдача недействительных каналов

                    if (chnlcrt.Count() > 0) 
                        await CreateChannelRange(chnlcrt); // Создание недействительных каналов

                    var UsersLeave = DBcontext.Users.AsQueryable().Where(x => x.guildId == Guild.guildid);
                    foreach (var user in UsersLeave) user.Leaved = true;

                    DBcontext.Channels.RemoveRange(ChannelsDelete);
                    DBcontext.EmoteClick.RemoveRange(Emoteclickes);
                    DBcontext.LVLROLES.RemoveRange(LVLROLE);
                    DBcontext.Users.UpdateRange(UsersLeave);
                    DBcontext.Guilds.Update(Guild);
                    await DBcontext.SaveChangesAsync();
                    await new Privates().CheckPrivate(glds); // Проверка приваток



                    await CheckTempUser(Guild, glds);
                }
                sw.Stop();
                Console.WriteLine(sw.Elapsed);
                loading = true;
            }
        } // ПРОВЕРКА КАНАЛОВ И РОЛЕЙ

        private static async Task CheckTempUser(Guilds glds, SocketGuild guild)
        {
            using (var DBcontext = new DBcontext())
            {
                var users = DBcontext.TempUser.AsQueryable().Where(x => x.guildid == glds.guildid);
                foreach (var user in users)
                    await UserMuteTime(user, guild);
                
                
            }
        } // Проверка активных нарушений

        private static async Task UserMuteTime(TempUser user, SocketGuild guild)
        {
            using (var DBcontext = new DBcontext())
            {
                if (user.ToTime < DateTime.Now)
                await Task.Delay(user.ToTime.Millisecond);

                var usr = guild.GetUser(user.userId);

                if (user.Reason.Contains("tban"))
                {
                    try
                    {
                        await guild.RemoveBanAsync(usr);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"252 - бан не удалился\n{guild.Name}\n{usr}");
                    }
                }
                    
                else
                {
                    var gld = DBcontext.Guilds.FirstOrDefault(x => x.guildid == guild.Id);
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

        } // Активация нарушений

        public static async Task<Guilds> CreateMuteRole(SocketGuild Context)
        {
            using (var DBcontext = new DBcontext())
            {
                var Guild = DBcontext.Guilds.FirstOrDefault(x => x.guildid == Context.Id);

                if (Context.GetRole(Guild.chatmuterole) == null)
                {
                    var MCC = await Context.CreateRoleAsync("ChatMute", new GuildPermissions(mentionEveryone: false), Color.Red, false, false);

                    foreach (var TC in Context.CategoryChannels)
                        await TC.AddPermissionOverwriteAsync(MCC, new OverwritePermissions(sendMessages: PermValue.Deny));

                    foreach (var TC in Context.TextChannels.Where(x=>x.Category != null))
                        await TC.SyncPermissionsAsync();

                    foreach (var TC in Context.TextChannels.Where(x => x.Category == null))
                        await TC.AddPermissionOverwriteAsync(MCC, new OverwritePermissions(sendMessages: PermValue.Deny));

                    Guild.chatmuterole = MCC.Id;
                }

                if (Context.GetRole(Guild.voicemuterole) == null)
                {
                    var MCV = await Context.CreateRoleAsync("VoiceMute", new GuildPermissions(mentionEveryone: false), Color.Red, false, false);

                    foreach (var TC in Context.CategoryChannels)
                        await TC.AddPermissionOverwriteAsync(MCV, new OverwritePermissions(sendMessages: PermValue.Deny));

                    foreach (var TC in Context.VoiceChannels.Where(x => x.Category != null))
                        await TC.SyncPermissionsAsync();

                    foreach (var VC in Context.VoiceChannels.Where(x => x.Category == null))
                        await VC.AddPermissionOverwriteAsync(MCV, new OverwritePermissions(speak: PermValue.Deny, connect: PermValue.Deny));

                    Guild.voicemuterole = MCV.Id;
                }

                DBcontext.Guilds.Update(Guild);
                await DBcontext.SaveChangesAsync();
                return Guild;
            }
        }

        public static async Task LVL(SocketUserMessage message) // ВЫДАЧА ОПЫТА И УРОВНЯ
        {
            using (var DBcontext = new DBcontext())
            {
                var user = message.Author as SocketGuildUser;
                var usr = GetOrCreateUserCache(user.Id, user.Guild.Id);
                //var usr = DBcontext.Users.FirstOrDefault(x=>x.userid == user.Id && x.guildId == user.Guild.Id);
                if ((ulong)Math.Sqrt((usr.XP + 10) / 80) > usr.Level)
                {
                    var roles = DBcontext.LVLROLES.AsQueryable().Where(x=>x.guildid == user.Guild.Id).ToList().OrderBy(x => x.countlvl);
                    if (roles.Count() != 0)
                    {
                        var afterrole = roles.FirstOrDefault(x => x.countlvl == (usr.Level + 1));
                        if (afterrole != null)
                        {
                            var beforerole = roles.LastOrDefault();

                            if (beforerole != null)
                            {
                                var befrole = user.Guild.GetRole(beforerole.roleid);
                                if (user.Roles.Contains(befrole))
                                    await user.RemoveRoleAsync(befrole);
                            }

                            var aftrole = user.Guild.GetRole(afterrole.roleid);
                            if (!user.Roles.Contains(aftrole))
                                await user.AddRoleAsync(aftrole);
                        }
                    }
                    await message.Channel.SendMessageAsync("", false, new EmbedBuilder().WithColor(255, 0, 94).WithDescription($"{user.Mention} LEVEL UP")
                                                                                        .AddField("LEVEL", $"{usr.Level + 1}", true)
                                                                                        .AddField("XP", $"{usr.XP + 10}", true).Build());
                }
                else
                {
                    var roles = DBcontext.LVLROLES.AsQueryable().Where(x=>x.countlvl == usr.Level && x.guildid == user.Guild.Id).ToList();
                    if (roles.Count() != 0)
                    {
                        var role = user.Guild.GetRole(roles.OrderBy(x => x.countlvl).LastOrDefault().roleid);
                        if (!user.Roles.Contains(role))
                            await user.AddRoleAsync(role);
                    }
                }
                usr.XP += 10;
                DBcontext.Users.Update(usr);
                await DBcontext.SaveChangesAsync();
            }
        } // Получение опыта

        private static IServiceProvider service = null;
        private static IMemoryCache cahce = null;

        public static Guilds GetOrCreateGuldsCache(ulong guildId)
        {
            using (var DBcontext = new DBcontext())
            {
                if (cahce == null)
                    cahce = (IMemoryCache)service.GetService(typeof(IMemoryCache));

                var x = (Guilds)cahce.Get(guildId);
                if (x == null)
                    x = DBcontext.Guilds.FirstOrDefault(x => x.guildid == guildId);

            return x;
            }
        }

        public static Channels GetOrCreateChannelCache(ulong channelId, ulong guildId)
        {
            using (var DBcontext = new DBcontext())
            {
                if (cahce == null)
                    cahce = (IMemoryCache)service.GetService(typeof(IMemoryCache));
                
                var x = (Channels)cahce.Get((channelId, guildId));

                if (x == null)
                    x = DBcontext.Channels.FirstOrDefault(x => x.channelid == channelId && x.guildid == guildId);
                
                return x;
            }
        }

        public static Users GetOrCreateUserCache(ulong userId,ulong guildId)
        {
            using (var DBcontext = new DBcontext())
            {
                if (cahce == null)
                    cahce = (IMemoryCache)service.GetService(typeof(IMemoryCache));
                
                var x = (Users)cahce.Get((userId,guildId));

                if (x == null)
                    x = DBcontext.Users.GetOrCreate(userId,guildId).Result;
                
                return x;
            }
        }

        public static async Task<EmbedBuilder> GetError(string error, DiscordSocketClient _discord)
        {
            var emb = new EmbedBuilder().WithAuthor("Ошибка");
            switch (error)
            {
                case "The input text has too many parameters.":
                    emb.WithDescription("Текст имеет много параметров").WithFooter("Параметры - то что вы пишите после самой команды.");
                    break;
                case "The input text has too few parameters.":
                    emb.WithDescription("Текст имеет мало параметров").WithFooter("Параметры - то что вы пишите после самой команды.");
                    break;
                case "User not found.":
                    emb.WithDescription("Введенный пользователь не найден");
                    break;
                case "Channel not found.":
                    emb.WithDescription("Введенный канал не найден");
                    break;
                case "Role not found.":
                    emb.WithDescription("Введенная роль не найдена.").WithFooter("Введите роль в таком формате @everyone");
                    break;
                case "Unknown command.":
                    emb.WithDescription("Команда которую вы ввели не найдена").WithFooter("Возможно вы просто ее неправильно написали.");
                    break;
                case "Failed to parse Int.":
                case "Failed to parse Int16.":
                case "Failed to parse Int32.":
                case "Failed to parse Int64.":
                    emb.WithDescription("Параметр должен иметь цифру").WithFooter("Возможно вы ввели букву, или большое значение?");
                    break;
                case "Failed to parse UInt.":
                case "Failed to parse UInt16.":
                case "Failed to parse UInt32.":
                case "Failed to parse UInt64.":
                    emb.WithDescription("Параметр должен иметь цифру").WithFooter("Возможно вы ввели букву, или число меньше 0?");
                    break;
                case "Failed to parse long.":
                case "Failed to parse long16.":
                case "Failed to parse long32.":
                case "Failed to parse long64.":
                    emb.WithDescription("Параметр должен иметь цифру").WithFooter("Возможно вы ввели букву, или большое значение?");
                    break;
                case "Failed to parse ulong.":
                case "Failed to parse ulong16.":
                case "Failed to parse ulong32.":
                case "Failed to parse ulong64.":
                    emb.WithDescription("Параметр должен иметь цифру").WithFooter("Возможно вы ввели букву, или число меньше 0?");
                    break;
                case "This command may only be invoked in an NSFW channel.":
                    emb.WithDescription("Данная команда доступна только в NSFW каналах.");
                    break;
                case "User requires guild permission MuteMembers.":
                    emb.WithDescription("Вы не обладаете правами мутить пользователя.");
                    break;
                case "User requires guild permission KickMembers.":
                    emb.WithDescription("Вы не обладаете правами кикать пользователя.");
                    break;
                case "User requires guild permission BanMembers.":
                    emb.WithDescription("Вы не обладаете правами банить пользователя.");
                    break;
                case "User requires guild permission Administrator.":
                    emb.WithDescription("Вы не обладаете правами администратора.");
                    break;
                case "User requires guild permission ManageChannels.":
                    emb.WithDescription("Вы не обладаете правами управлять каналами.");
                    break;
                case "User requires guild permission ManageRoles.":
                    emb.WithDescription("Вы не обладаете правами управлять ролями.");
                    break;
                case "User requires guild permission ManageGuild.":
                    emb.WithDescription("Вы не обладаете правами управлять сервером.");
                    break;
                case "Value is not a Role.":
                    emb.WithDescription("Введенный параметр не является роль.").WithFooter("Введите роль в таком формате @everyone");
                    break;
                case "Command can only be run by the owner of the bot.":
                    emb.WithDescription("Команда предназначена только для создателя бота.").WithFooter($"Создатель бота {_discord.GetUser(BotSettings.hikaruid)}");
                    break;
                case "Команда отключена создателем сервера.":
                case "Вы не являетесь создателем сервера чтобы использовать эту команду":
                    emb.WithDescription(error);
                    break;
                default:
                    emb.WithDescription(error);
                    if (error.ToCharArray().Where(x => x >= 192 && x <= 255) == null)
                    {
                        emb.WithFooter("Команда уже отправлена на перевод! Спасибо за ваше любопытство.");
                        await (_discord.GetChannel(BotSettings.SystemMessage) as ITextChannel).SendMessageAsync($"{_discord.GetUser(BotSettings.hikaruid).Mention}", false, emb.Build());
                    }
                    break;
            }
            return emb;
        } // ПЕРЕВОД ОШИБОК


        private static List<SocketUserMessage> MessageList = new List<SocketUserMessage>();

        public static async Task<bool> ChatSystem(SocketCommandContext msg, Channels chnl,string Prefix)
        {
            using (var DBcontext = new DBcontext())
            {
                if (loading == false) return true;
                if (chnl.Spaming)
                {
                    MessageList.Add(msg.Message);
                    MessageList.RemoveAll(x => (DateTime.Now - x.CreatedAt).TotalSeconds >= 5);

                    var mew = new List<SocketUserMessage>();
                    var mes = MessageList.Where(x => x.Author == msg.User && (x.Author as SocketGuildUser).Guild == (msg.User as SocketGuildUser).Guild);
                    foreach (var Messes in mes)
                    {
                        if (new SpamChecking().CalculateFuzzyEqualValue(msg.Message.Content, Messes.Content) == 1)
                            mew.Add(Messes);
                    }
                    if (mew.Count() > 3)
                    {
                        await (msg.Channel as SocketTextChannel).AddPermissionOverwriteAsync(msg.Message.Author, new OverwritePermissions(sendMessages: PermValue.Deny));
                        var messa = await msg.Message.Channel.GetMessagesAsync(mew.Count()).FlattenAsync();
                        var result = messa.Where(x => x.Author.Id == msg.Message.Author.Id);
                        await (msg.Channel as SocketTextChannel).DeleteMessagesAsync(result);
                        await (msg.Channel as SocketTextChannel).RemovePermissionOverwriteAsync(msg.Message.Author);
                        return true;
                    }
                } // Проверка на спам
                if (chnl.SendUrl)
                {
                    if (chnl.SendUrlImage)
                    {
                        if (new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase).Matches(msg.Message.Content).Count > 0)
                        {
                            if (chnl.csUrlWhiteListList.FirstOrDefault(x => msg.Message.Content.Contains(x)) == null)
                            {
                                await msg.Message.DeleteAsync();
                                return true;
                            }
                        }
                    }
                } // Отправка ссылки
                if (chnl.SendCaps && msg.Message.Content.Count(c => char.IsUpper(c)) == msg.Message.Content.Length)
                {
                        await msg.Message.DeleteAsync();
                        return true;
                } // КАПС СООБЩЕНИЯ
                if (chnl.SendBadWord && chnl.BadWordList != null)
                {
                    int argPos = 0;
                    var x = Regex.Matches(msg.Message.Content, @"\b[\p{L}]+\b").Cast<Match>().Select(match => match.Value.ToLower()).Where(word => chnl.BadWordList.Contains(word)).Any();
                    if (x && !msg.Message.HasStringPrefix(Prefix, ref argPos))
                    {
                        await msg.Message.DeleteAsync();
                        return true;
                    }
                }// ПЛОХИЕ СЛОВА
                if (chnl.InviteMessage)
                {
                    var z = Regex.Matches(msg.Message.Content, @"(https?:\/\/)?(www\.)?(discord\.(gg|io|me|li|com)|discord(app)?\.com\/invite)\/(?<Code>\w+)");
                    if (z.Count > 0)
                    {
                        var x = msg.Guild.GetInvitesAsync().Result.Where(x => msg.Message.Content.Contains(x.Id));
                        if (x.Count() == 0)
                        {
                            await msg.Message.DeleteAsync();
                            return true;
                        }
                    }
                } // ИНВАЙТЫ
                return false;
            }

        } // Проверка сообщений
    }
}
