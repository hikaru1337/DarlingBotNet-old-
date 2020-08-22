using DarlingBotNet.DataBase;

using DarlingBotNet.Modules;
using DarlingBotNet.Services.Sys;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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


        public static async Task<(bool, EmbedBuilder)> CheckText(string report)
        {
            await Task.Delay(1);
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
                var glds = DBcontext.Guilds.AsNoTracking().FirstOrDefault(x => x.guildid == Guild.Id);
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

        public static async Task<Users> UserCreate(ulong userId, ulong guildId)
        {
            using (var DBcontext = new DBcontext())
            {
                //EnsureCreated(userId, guildId);
                var gg = DBcontext.Users.AsNoTracking().FirstOrDefault(u => u.userid == userId && u.guildId == guildId);
                if (gg == null)
                {
                    gg = new Users() { userid = userId, guildId = guildId, ZeroCoin = 1000,clanInfo = Users.UserClanRole.ready};
                    DBcontext.Add(gg);
                    await DBcontext.SaveChangesAsync();
                }
                await Task.Delay(1);
                return gg;
            }
        }

        public static async Task CreateChannelRange(IEnumerable<SocketTextChannel> Channels)
        {
            using (var DBcontext = new DBcontext())
            {
                var lists = new List<Channels>();
                foreach (var TextChannel in Channels)
                {
                    lists.Add(new Channels() { guildid = TextChannel.Guild.Id, channelid = TextChannel.Id, GiveXP = true, UseCommand = true });
                }
                DBcontext.Channels.AddRange(lists);
                await DBcontext.SaveChangesAsync();
                await Task.Delay(1);
            }
        }

        Stopwatch sw = new Stopwatch();
        public async Task GuildCheck()
        {
            sw.Start();
            using (var DBcontext = new DBcontext())
            {
                foreach (var Guild in _discord.Guilds)
                {
                    if(DBcontext.Guilds.AsNoTracking().FirstOrDefault(x=>x.guildid == Guild.Id) == null)
                        await GuildCreate(Guild);
                } // Проверка Гильдий которые есть в боте но нету в базе


                foreach (var glds in DBcontext.Guilds.AsNoTracking())
                {
                    if(_discord.GetGuild(glds.guildid) == null)
                        await GuildDelete(glds);
                } // Проверка гильдий которые удалили бота во время его офлайна
            }
            await ChannelCheck(_discord);
        } // МИГРАЦИЯ, ПРОВЕРКА ГИЛЬДИЙ

        public async Task GuildDelete(Guilds Guild)
        {
            using (var DBcontext = new DBcontext())
            {
                DBcontext.Channels.RemoveRange(DBcontext.Channels.AsNoTracking().Where(x=>x.guildid == Guild.guildid));
                DBcontext.LVLROLES.RemoveRange(DBcontext.LVLROLES.AsNoTracking().Where(x => x.guildid == Guild.guildid));
                DBcontext.EmoteClick.RemoveRange(DBcontext.EmoteClick.AsNoTracking().Where(x => x.guildid == Guild.guildid));
                DBcontext.PrivateChannels.RemoveRange(DBcontext.PrivateChannels.AsNoTracking().Where(x => x.guildid == Guild.guildid));
                DBcontext.Warns.RemoveRange(DBcontext.Warns.AsNoTracking().Where(x => x.guildid == Guild.guildid));
                DBcontext.TempUser.RemoveRange(DBcontext.TempUser.AsNoTracking().Where(x => x.guildid == Guild.guildid));

                var UsersLeave = DBcontext.Users.AsNoTracking().Where(x=>x.guildId == Guild.guildid);
                foreach (var user in UsersLeave) user.Leaved = true;
                DBcontext.Users.UpdateRange(UsersLeave);

                Guild.Leaved = true;
                DBcontext.Guilds.Update(Guild);
                await DBcontext.SaveChangesAsync();
                await Task.Delay(1);
            }
        } // УДАЛЕНИЕ ИНФОРМАЦИИ ГИЛЬДИИ


        public async Task ChannelCheck(DiscordSocketClient _discord)
        {
            using (var DBcontext = new DBcontext())
            {
                var Guilds = DBcontext.Guilds.AsNoTracking().Where(x => x.Leaved == false);

                foreach (var Guild in Guilds)
                {
                    var glds = _discord.GetGuild(Guild.guildid); // Выдача гильдии

                    var ChannelsDelete = DBcontext.Channels.AsNoTracking().Where(x=> x.guildid == Guild.guildid).AsEnumerable().Where(x=> glds.GetChannel(x.channelid) == null); // Удаление недействительных каналов

                    var Emoteclickes = DBcontext.EmoteClick.AsNoTracking().Where(x => x.guildid == Guild.guildid).AsEnumerable().Where(x=> ChannelsDelete.Where(x=>x.channelid == x.channelid) != null || (glds.GetRole(x.roleid) != null || glds.Emotes.Where(z => z.Name == x.emote) == null)); // выдача недействительных EmoteCLick

                    var LVLROLE = DBcontext.LVLROLES.AsNoTracking().Where(x=> x.guildid == Guild.guildid).AsEnumerable().Where(x => glds.GetRole(x.roleid) == null); // Выдача недействительных Уровневых ролей

                    var chnlcrt = glds.TextChannels.Where(x => DBcontext.Channels.AsNoTracking().FirstOrDefault(z=> z.channelid == x.Id) == null); // Выдача недействительных каналов

                    if (chnlcrt.Count() > 0) await CreateChannelRange(chnlcrt); // Создание недействительных каналов

                    var UsersLeave = DBcontext.Users.AsNoTracking().Where(x => x.guildId == Guild.guildid);
                    foreach (var user in UsersLeave) user.Leaved = true;

                    DBcontext.Channels.RemoveRange(ChannelsDelete);
                    DBcontext.EmoteClick.RemoveRange(Emoteclickes);
                    DBcontext.LVLROLES.RemoveRange(LVLROLE);
                    DBcontext.Users.UpdateRange(UsersLeave);
                    await DBcontext.SaveChangesAsync();
                    await new Privates().CheckPrivate(glds); // Проверка приваток
                    

                    sw.Stop();
                    Console.WriteLine(sw.Elapsed);
                    CheckTempUser(Guild, glds);
                }
               
            }
        } // ПРОВЕРКА КАНАЛОВ И РОЛЕЙ

        private async void CheckTempUser(Guilds glds, SocketGuild guild)
        {
            using (var DBcontext = new DBcontext())
            {
                var users = DBcontext.TempUser.AsNoTracking().Where(x => x.guildid == glds.guildid);
                foreach (var user in users)
                    await UserMuteTime(user, guild);
                
                loading = true;
            }
        } // Проверка активных нарушений

        private async Task UserMuteTime(TempUser user, SocketGuild guild)
        {
            if (user.ToTime < DateTime.Now)
                await Task.Delay(user.ToTime.Millisecond);

                var usr = guild.GetUser(user.userId);

                if (user.Reason.Contains("tban"))
                    await guild.RemoveBanAsync(usr);
                else
                {
                    var gld = await CreateMuteRole(guild);
                    var cmute = guild.GetRole(gld.chatmuterole);
                    var vmute = guild.GetRole(gld.voicemuterole);


                    if (usr.Roles.Contains(cmute))
                        await usr.RemoveRoleAsync(cmute);
                    if (usr.Roles.Contains(vmute))
                        await usr.RemoveRoleAsync(vmute);

                }
            using (var DBcontext = new DBcontext())
            {
                if (DBcontext.TempUser.AsNoTracking().FirstOrDefault(x=>x.userId == user.userId && x.guildid == user.guildid) != null)
                {
                    DBcontext.TempUser.Remove(user);
                    await DBcontext.SaveChangesAsync();
                }
            }

        } // Активация нарушений

        public async Task<Guilds> CreateMuteRole(SocketGuild Context)
        {
            using (var DBcontext = new DBcontext())
            {
                var Guild = DBcontext.Guilds.AsNoTracking().FirstOrDefault(x => x.guildid == Context.Id);

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

        public async Task LVL(SocketUserMessage message) // ВЫДАЧА ОПЫТА И УРОВНЯ
        {
            using (var DBcontext = new DBcontext())
            {
                var user = message.Author as SocketGuildUser;
                var usr = DBcontext.Users.AsNoTracking().FirstOrDefault(x=>x.userid == user.Id && x.guildId == user.Guild.Id);
                if ((ulong)Math.Sqrt((usr.XP + 10) / 80) > usr.Level)
                {
                    var roles = DBcontext.LVLROLES.AsNoTracking().Where(x=>x.guildid == user.Guild.Id).ToList().OrderBy(x => x.countlvl);
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
                    var roles = DBcontext.LVLROLES.AsNoTracking().Where(x=>x.countlvl == usr.Level && x.guildid == user.Guild.Id).ToList();
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

        public async Task<bool> ChatSystem(SocketCommandContext msg)
        {
            using (var DBcontext = new DBcontext())
            {
                var usr = DBcontext.Users.AsNoTracking().FirstOrDefault(x=>x.userid == msg.User.Id && x.guildId == (msg.User as SocketGuildUser).Guild.Id);
                if (usr == null)
                {
                    DBcontext.Users.Add(new Users() { userid = msg.User.Id, guildId = (msg.User as SocketGuildUser).Guild.Id, ZeroCoin = 1000, clanInfo = Users.UserClanRole.ready });
                    await DBcontext.SaveChangesAsync();
                }
                if (loading == false) return true;
                var Guild = DBcontext.Guilds.AsNoTracking().FirstOrDefault(x=>x.guildid == msg.Guild.Id);
                Channels glds = DBcontext.Channels.AsNoTracking().FirstOrDefault(x=>x.guildid == msg.Guild.Id && x.channelid == (msg.Channel as SocketTextChannel).Id);
                if (glds.Spaming)
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
                if (glds.SendUrl)
                {
                    if (glds.SendUrlImage)
                    {
                        if (new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase).Matches(msg.Message.Content).Count > 0)
                        {
                            if (glds.csUrlWhiteListList.FirstOrDefault(x => msg.Message.Content.Contains(x)) == null)
                            {
                                await msg.Message.DeleteAsync();
                                return true;
                            }
                        }
                    }
                } // Отправка ссылки
                if (glds.SendCaps && msg.Message.Content.Count(c => char.IsUpper(c)) == msg.Message.Content.Length)
                {
                        await msg.Message.DeleteAsync();
                        return true;
                } // КАПС СООБЩЕНИЯ
                if (glds.SendBadWord && glds.BadWordList != null)
                {
                    int argPos = 0;
                    var x = Regex.Matches(msg.Message.Content, @"\b[\p{L}]+\b").Cast<Match>().Select(match => match.Value.ToLower()).Where(word => glds.BadWordList.Contains(word)).Any();
                    if (x && !msg.Message.HasStringPrefix(Guild.Prefix, ref argPos))
                    {
                        await msg.Message.DeleteAsync();
                        return true;
                    }
                }// ПЛОХИЕ СЛОВА
                if (glds.InviteMessage)
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
