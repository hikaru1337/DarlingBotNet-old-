using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.Modules;
using DarlingBotNet.Services.Sys;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private readonly DbService _db;

        public SystemLoading(DiscordSocketClient discord, DbService db)
        {
            _discord = discord;
            _db = db;
        } // Подключение компонентов


        public static async Task<(bool, EmbedBuilder)> CheckText(string report)
        {
            await Task.Delay(1);
            bool es = false;
            var emb = new EmbedBuilder().WithColor(255, 0, 94);
            try
            {
                if (report.ToLower() == "kick")
                    es = true;
                else if (report.ToLower() == "ban")
                    es = true;
                else if (report.ToLower() == "mute")
                    es = true;
                else if (report.ToLower().Substring(0, 4) == "tban" || report.ToLower().Substring(0, 5) == "tmute")
                {
                    if (report.ToLower().Substring(0, 4) == "tban")
                    {
                        try
                        {
                            if (Convert.ToUInt64(report.ToLower().Substring(4,report.Length - 4)) <= 720)
                                es = true;
                            else
                                emb.WithDescription("Время должно быть не больше 720 минут");
                        }
                        catch (Exception)
                        {
                            emb.WithDescription($"Для указания временного бана используйте {report.ToLower().Substring(0, 4)}50 (50 - минут)")
                               .WithFooter("Инструкция о команде - ");
                        }
                    }
                    else
                    {
                        try
                        {
                            if (Convert.ToUInt64(report.ToLower().Substring(5, report.Length - 5)) <= 720)
                                es = true;
                            else
                                emb.WithDescription("Время должно быть не больше 720 минут");
                        }
                        catch (Exception)
                        {
                            emb.WithDescription($"Для указания временного мута используйте {report.ToLower().Substring(0, 5)}50 (50 - минут)")
                               .WithFooter("Инструкция о команде - [инструкция](https://docs.darlingbot.ru/commands/settings-server/system-violation#vystavit-varn-na-servere)");
                        }
                    }

                }
                else emb.WithDescription("Используйте эти нарушения ban,kick,mute,tmute,tban. Инструкция к команде");
                return (es, emb);
            }
            catch (Exception)
            {
                emb.WithDescription("Используйте эти нарушения ban,kick,mute,tmute,tban. Инструкция к команде");
                return (es, emb);
            }
        }


        public async Task GuildCheck()
        {
            using (var DBcontext = _db.GetDbContext())
            {
                foreach (var Guild in _discord.Guilds)
                {
                    if(DBcontext.Guilds.Get(Guild) == null)
                    {
                        var glds = DBcontext.Guilds.GetOrCreate(Guild);
                        DBcontext.Channels.CreateRange(Guild.Channels.Where(x => x as SocketCategoryChannel == null));
                    }
                } // Проверка Гильдий которые есть в боте но нету в базе


                var NullGuilds = DBcontext.Guilds.GetAll().Where(x => _discord.GetGuild(x.guildId) == null);
                foreach (var glds in NullGuilds)
                {
                    await GuildDelete(glds);
                } // Проверка гильдий которые удалили бота во время его офлайна
                await DBcontext.SaveChangesAsync();
            }
            await ChannelCheck(_discord);
        } // МИГРАЦИЯ, ПРОВЕРКА ГИЛЬДИЙ

        public async Task GuildDelete(Guilds Guild)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                DBcontext.Channels.RemoveRange(Guild.guildId);
                DBcontext.LVLROLES.RemoveRange(Guild.guildId);
                DBcontext.EmoteClick.RemoveRange(Guild.guildId);
                DBcontext.PrivateChannels.RemoveRange(Guild.guildId);
                DBcontext.Warns.RemoveRange(Guild.guildId);
                DBcontext.TempUser.RemoveRange(Guild.guildId);

                var UsersLeave = DBcontext.Users.Get(Guild.guildId);
                foreach (var user in UsersLeave) user.Leaved = true;
                DBcontext.Users.UpdateRange(UsersLeave);

                Guild.Leaved = true;
                DBcontext.Guilds.Update(Guild);
                await Task.Delay(1);
            }
        } // УДАЛЕНИЕ ИНФОРМАЦИИ ГИЛЬДИИ


        public async Task ChannelCheck(DiscordSocketClient _discord)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var Guilds = DBcontext.Guilds.Where(x => x.Leaved == false);

                foreach (var Guild in Guilds)
                {
                    var glds = _discord.GetGuild(Guild.guildId);

                    var Channels = DBcontext.Channels.Get(Guild).Where(x=>glds.GetChannel(x.channelid) == null);

                    var Emoteclickes = DBcontext.EmoteClick.GetCS(Guild.guildId,Channels).Where(x => (glds.GetRole(x.roleid) != null || glds.Emotes.Where(z => z.Name == x.emote) == null));

                    var LVLROLE = DBcontext.LVLROLES.Get(Guild).Where(x=> glds.GetRole(x.roleid) == null);

                    var chnlcrt = glds.Channels.Where(x => DBcontext.Channels.Get(x) != null);

                    if (chnlcrt != null) DBcontext.Channels.CreateRange(chnlcrt.Where(x => x as SocketCategoryChannel == null));

                    var UsersLeave = DBcontext.Users.Get(Guild.guildId).Where(x=> glds.GetUser(x.userid) == null);
                    foreach (var user in UsersLeave) user.Leaved = true;

                    DBcontext.Channels.RemoveRange(Channels);
                    DBcontext.EmoteClick.RemoveRange(Emoteclickes);
                    DBcontext.LVLROLES.RemoveRange(LVLROLE);
                    DBcontext.Users.UpdateRange(UsersLeave);
                    await DBcontext.SaveChangesAsync();

                    await new Privates(_db).CheckPrivate(Guild.guildId, glds);
                    CheckTempUser(Guild, glds);
                }
               
            }
        } // ПРОВЕРКА КАНАЛОВ И РОЛЕЙ

        private async void CheckTempUser(Guilds glds, SocketGuild guild)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var users = DBcontext.TempUser.Get(glds.guildId);
                foreach (var user in users)
                {
                    await UserMuteTime(user, guild);
                }
                loading = true;
            }
        }

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
            using (var DBcontext = _db.GetDbContext())
            {
                if (DBcontext.TempUser.Get(user) != null)
                {
                    DBcontext.TempUser.Remove(user);
                    await DBcontext.SaveChangesAsync();
                }
            }

        }

        public async Task<Guilds> CreateMuteRole(SocketGuild Context)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var Guild = DBcontext.Guilds.Get(Context.Id);
                if (Guild.chatmuterole == 0 || Context.GetRole(Guild.chatmuterole) == null)
                {
                    var MCC = await Context.CreateRoleAsync("MuteClownChat", new GuildPermissions(mentionEveryone: false), Color.Red, false, false);

                    foreach (var TC in Context.TextChannels)
                        await TC.AddPermissionOverwriteAsync(MCC, new OverwritePermissions(sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny));
                    Guild.chatmuterole = MCC.Id;
                }

                if (Guild.voicemuterole == 0 || Context.GetRole(Guild.voicemuterole) == null)
                {
                    var MCV = await Context.CreateRoleAsync("MuteClownVoice", new GuildPermissions(mentionEveryone: false), Color.Red, false, false);

                    foreach (var VC in Context.VoiceChannels)
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
            using (var DBcontext = _db.GetDbContext())
            {
                var user = message.Author as SocketGuildUser;
                var usr = DBcontext.Users.Get(user.Id,user.Guild.Id);
                if ((ulong)Math.Sqrt((usr.XP + 10) / 80) > usr.Level)
                {
                    var roles = DBcontext.LVLROLES.Get(user.Guild).OrderBy(x => x.countlvl);
                    if (roles != null)
                    {
                        var afterrole = roles.FirstOrDefault(x => x.countlvl == (usr.Level + 1));
                        if (afterrole != null)
                        {
                            var beforerole = roles.LastOrDefault();

                            if (beforerole != null)
                            {
                                var befrole = user.Guild.GetRole(beforerole.roleid);
                                if (befrole != null && user.Roles.Contains(befrole))
                                    await user.RemoveRoleAsync(befrole);
                            }

                            var aftrole = user.Guild.GetRole(afterrole.roleid);
                            if (aftrole != null && !user.Roles.Contains(aftrole))
                                await user.AddRoleAsync(aftrole);
                        }
                    }
                    await message.Channel.SendMessageAsync("", false, new EmbedBuilder().WithColor(255, 0, 94).WithDescription($"{user.Mention} LEVEL UP")
                                                                                        .AddField("LEVEL", $"{usr.Level + 1}", true)
                                                                                        .AddField("XP", $"{usr.XP + 10}", true).Build());
                }
                else
                {
                    var roles = DBcontext.LVLROLES.Get(usr.Level,user.Guild.Id).ToList().OrderBy(x => x.countlvl).LastOrDefault();
                    if (roles != null)
                    {
                        var role = user.Guild.GetRole(roles.roleid);
                        if (role != null && !user.Roles.Contains(role))
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

        private class Message
        {
            public ulong MessagesId { get; set; }
            public ulong UserId { get; set; }
            public ulong GuildId { get; set; }
            public DateTime Data { get; set; }
            public string Messages { get; set; }
        }
        private static List<Message> MessageList = new List<Message>();

        public async Task<bool> ChatSystem(SocketCommandContext msg)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                DBcontext.Users.GetOrCreate(msg.User.Id,(msg.User as SocketGuildUser).Guild.Id);
                await DBcontext.SaveChangesAsync();
                if (loading == false) return true;
                var Guild = DBcontext.Guilds.Get(msg.Guild.Id);
                Channels glds = DBcontext.Channels.Get(msg.Channel as SocketGuildChannel);
                if (glds.Spaming)
                {
                    MessageList.Add(new Message()
                    {
                        Messages = msg.Message.Content,
                        UserId = msg.User.Id,
                        GuildId = (msg.User as SocketGuildUser).Guild.Id,
                        Data = DateTime.Now,
                        MessagesId = msg.Message.Id
                    });

                    MessageList.RemoveAll(x => (DateTime.Now - x.Data).TotalSeconds >= 5);

                    var mew = new List<Message>();
                    var mes = MessageList.Where(x => x.UserId == msg.User.Id && x.GuildId == (msg.User as SocketGuildUser).Guild.Id);
                    foreach (var Messes in mes)
                    {
                        if (new SpamChecking().CalculateFuzzyEqualValue(msg.Message.Content, Messes.Messages) == 1)
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
                if (glds.antiMat && msg.Channel.Id == 637305727131844619)
                {
                    //if (mat.Content.ToLower().Contains(mt.ToLower())) 
                    //{ 
                    //    await msg.Channel.SendMessageAsync($"найден Мат {msg.Content}"); await msg.DeleteAsync(); return true; 
                    //}

                } // АНТИ МАТ
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
