using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.Modules;
using DarlingBotNet.Services.Sys;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static DarlingBotNet.DataBase.Guilds;
using static DarlingBotNet.DataBase.Warns;

namespace DarlingBotNet.Services
{
    public class OtherSettings
    {
        public static string WelcomeText =
        "⚡️ Бот по стандарту использует префикс: **h.**\n" +
        "h.m - список модулей\n" +
        "h.c [module] - список команд модуля\n" +
        "h.i [command] - информация о команде\n" +
        "🔨 нашли баг? Пишите - **{0}bug [описание бага]**\n" +
        "👑 Инструкция бота - https://docs.darlingbot.ru/ \n\n" +
        "🎁Добавить бота на сервер - [КЛИК](https://discord.com/oauth2/authorize?client_id=663381953181122570&scope=bot&permissions=8)\n\n" +
        "Все обновления бота будут выходить тут - [КЛИК](https://docs.darlingbot.ru/obnovleniya)";


        private readonly DiscordSocketClient _discord;
        //public static bool loading = false;
        //private static IServiceProvider service = null;

        public OtherSettings(DiscordSocketClient discord)
        {
            _discord = discord;

        } // Подключение компонентов

        public static async Task<string> CheckRoleValid(SocketGuildUser User, ulong RoleId, bool AddOrRemove)
        {
            var emb = new EmbedBuilder();
            var DiscordRole = User.Guild.GetRole(RoleId);
            if (DiscordRole != null)
            {
                var bot = User.Guild.CurrentUser;
                var roleRoleCheck = bot.Roles.FirstOrDefault(x => x.Position > DiscordRole.Position);
                if (roleRoleCheck != null)
                {
                    if (bot.GuildPermissions.ManageRoles || bot.GuildPermissions.Administrator)
                    {
                        if (AddOrRemove)
                            await User.RemoveRoleAsync(DiscordRole);
                        else
                            await User.AddRoleAsync(DiscordRole);
                        return null;
                    }
                    else return "Бот не имеет прав выдавать роли!";
                }
                else return $"Роль {DiscordRole.Mention} выше роли бота, из за чего он не может ее выдать!";
            }
            else return "Роль не найдена на сервере!";
        }

        public static async Task<Guilds> CreateMuteRole(SocketGuild Context)
        {
            using (var DBcontext = new DBcontext())
            {
                if(Context.CurrentUser.GuildPermissions.ManageRoles && Context.CurrentUser.GuildPermissions.ManageChannels)
                {
                    var Guild = ScanningDataBase._cache.GetOrCreateGuldsCache(Context.Id);
                    if (Context.GetRole(Guild.chatmuterole) == null)
                    {
                        var MCC = await Context.CreateRoleAsync("ChatMute", new GuildPermissions(mentionEveryone: false), Discord.Color.Red, false, false);

                        foreach (var TC in Context.CategoryChannels)
                            await TC.AddPermissionOverwriteAsync(MCC, new OverwritePermissions(sendMessages: PermValue.Deny));

                        foreach (var TC in Context.TextChannels.Where(x => x.Category != null))
                            await TC.SyncPermissionsAsync();

                        foreach (var TC in Context.TextChannels.Where(x => x.Category == null))
                            await TC.AddPermissionOverwriteAsync(MCC, new OverwritePermissions(sendMessages: PermValue.Deny));

                        Guild.chatmuterole = MCC.Id;
                    }

                    if (Context.GetRole(Guild.voicemuterole) == null)
                    {
                        var MCV = await Context.CreateRoleAsync("VoiceMute", new GuildPermissions(mentionEveryone: false), Discord.Color.Red, false, false);

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
                return null;
            }
        }

        public static async Task LVL(SocketUserMessage message) // ВЫДАЧА ОПЫТА И УРОВНЯ
        {
            using (var DBcontext = new DBcontext())
            {
                var user = message.Author as SocketGuildUser;
                var usr = ScanningDataBase._cache.GetOrCreateUserCache(user.Id, user.Guild.Id);
                if ((ulong)Math.Sqrt((usr.XP + 10) / 80) > usr.Level)
                {
                    var roles = DBcontext.LVLROLES.AsQueryable().Where(x => x.GuildId == user.Guild.Id).AsEnumerable().Where(x => x.CountLvl >= usr.Level && x.CountLvl <= (usr.Level + 1)).OrderBy(x => x.CountLvl).ToList();
                    if (roles.Count() != 0)
                    {
                        var afterrole = roles.LastOrDefault();
                        if (afterrole != null)
                        {
                            var beforerole = roles.FirstOrDefault();
                            if (beforerole != null && afterrole.RoleId != beforerole.RoleId)
                            {
                                await CheckRoleValid(user, beforerole.RoleId, true);
                            }

                            var aftrole = user.Guild.GetRole(afterrole.RoleId);


                            if (!user.Roles.Contains(aftrole))
                                await CheckRoleValid(user, aftrole.Id, true);

                        }
                    }

                    ulong amt = 500 + ((500 / 35) * (usr.Level + 1));
                    usr.ZeroCoin += amt;
                    await message.Channel.SendMessageAsync("", false, new EmbedBuilder().WithColor(255, 0, 94).WithDescription($"{user.Mention} LEVEL UP")
                                                                                        .AddField("LEVEL", $"{usr.Level + 1}", true)
                                                                                        .AddField("XP", $"{usr.XP + 10}", true)
                                                                                        .AddField("ZeroCoins", $"+{amt}")
                                                                                        .Build());
                }
                else
                {
                    var roles = DBcontext.LVLROLES.AsQueryable().Where(x => x.GuildId == user.Guild.Id).AsEnumerable().LastOrDefault(x => x.CountLvl <= usr.Level);
                    if (roles != null)
                    {
                        if (user.Roles.FirstOrDefault(x => x.Id == roles.RoleId) == null)
                            await CheckRoleValid(user, roles.RoleId, false);

                    }
                }
                usr.XP += 10;
                DBcontext.Users.Update(usr);
                await DBcontext.SaveChangesAsync();
            }
        } // Получение опыта


        public static async Task<EmbedBuilder> GetError(string error, DiscordSocketClient _discord, string prefix, CommandInfo command = null)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Ошибка!");
            if (error != "Unknown command." &&
                error != "User not found." &&
                error != "Channel not found." &&
                error != "Role not found." &&
                error != "This command may only be invoked in an NSFW channel." &&
                error != "User requires guild permission MuteMembers." &&
                error != "User requires guild permission KickMembers." &&
                error != "User requires guild permission BanMembers." &&
                error != "User requires guild permission Administrator." &&
                error != "User requires guild permission ManageChannels." &&
                error != "User requires guild permission ManageRoles." &&
                error != "User requires guild permission ManageGuild." &&
                error != "Value is not a Role." &&
                error != "Command can only be run by the owner of the bot." &&
                error != "Команда отключена создателем сервера." &&
                error != "Вы не являетесь создателем сервера чтобы использовать эту команду" &&
                error != "Value is not a ReportType." &&
                error != "Value is not a ViolationSystem.")
            {
                string text = null;
                foreach (var Parameter in command.Parameters)
                {
                    if (Parameter.IsOptional)
                        text += $"[{Parameter}/null]";
                    else
                        text += $"[{Parameter}] ";

                }
                emb.WithDescription($"Описание: {command.Summary}\n\nПример: {prefix}{command.Name} {text}");
            }
            switch (error)
            {
                case "The input text has too many parameters.":
                    emb.Author.Name += "Текст имеет много параметров";
                    break;
                case "The input text has too few parameters.":
                    emb.Author.Name += "Текст имеет мало параметров";
                    break;
                case "User not found.":
                    emb.WithDescription("Введенный пользователь не найден");
                    break;
                case "Channel not found.":
                    emb.WithDescription("Введенный канал не найден");
                    break;
                case "Role not found.":
                    emb.WithDescription("Введенная роль не найдена.");
                    emb.WithFooter("Введите роль в таком формате @everyone");
                    break;
                case "Unknown command.":
                    emb.WithDescription("Команда которую вы ввели не найдена");
                    emb.WithFooter("Возможно вы просто ее неправильно написали.");
                    break;
                case "Failed to parse Int.":
                case "Failed to parse Int16.":
                case "Failed to parse Int32.":
                case "Failed to parse Int64.":
                    emb.Author.Name += "Параметр должен иметь цифру";
                    emb.WithFooter("Возможно вы ввели букву, или большое значение?");
                    break;
                case "Failed to parse UInt.":
                case "Failed to parse UInt16.":
                case "Failed to parse UInt32.":
                case "Failed to parse UInt64.":
                    emb.Author.Name += "Параметр должен иметь цифру";
                    emb.WithFooter("Возможно вы ввели букву, или число меньше 0?");
                    break;
                case "Failed to parse long.":
                case "Failed to parse long16.":
                case "Failed to parse long32.":
                case "Failed to parse long64.":
                    emb.Author.Name += "Параметр должен иметь цифру";
                    emb.WithFooter("Возможно вы ввели букву, или большое значение?");
                    break;
                case "Failed to parse ulong.":
                case "Failed to parse ulong16.":
                case "Failed to parse ulong32.":
                case "Failed to parse ulong64.":
                    emb.Author.Name += "Параметр должен иметь цифру";
                    emb.WithFooter("Возможно вы ввели букву, или число меньше 0?");
                    break;
                case "Failed to parse Boolean.":
                    emb.Author.Name += "Параметр должен быть или **true**(да), или **false**(нет)";
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


                case "Bot requires guild permission MuteMembers.":
                    emb.WithDescription("Бот не обладает правами мутить пользователя.");
                    break;
                case "Bot requires guild permission KickMembers.":
                    emb.WithDescription("Бот не обладает правами кикать пользователя.");
                    break;
                case "Bot requires guild permission BanMembers.":
                    emb.WithDescription("Бот не обладает правами банить пользователя.");
                    break;
                case "Bot requires guild permission Administrator.":
                    emb.WithDescription("Бот не обладает правами администратора.");
                    break;
                case "Bot requires guild permission ManageChannels.":
                    emb.WithDescription("Бот не обладает правами управлять каналами.");
                    break;
                case "Bot requires guild permission ManageRoles.":
                    emb.WithDescription("Бот не обладает правами управлять ролями.");
                    break;
                case "Bot requires guild permission ManageGuild.":
                    emb.WithDescription("Бот не обладает правами управлять сервером.");
                    break;
                case "Bot requires guild permission SendMessage.":
                    emb.WithDescription("Бот не обладает правами отправлять сообщения.");
                    break;


                case "User requires channel permission ManageChannels.":
                    emb.WithDescription("Вы не обладаете правами управлять этим каналом.");
                    break;
                case "User requires channel permission ManageMessages.":
                    emb.WithDescription("Вы не обладаете правами управлять сообщениями этого канала.");
                    break;
                case "Bot requires channel permission ManageMessages.":
                    emb.WithDescription("Бот не обладает правами управлять сообщениями этого канала.");
                    break;



                case "Value is not a Role.":
                    emb.WithDescription("Введенный параметр не является роль.");
                    emb.WithFooter("Введите роль в таком формате @everyone");
                    break;
                case "Command can only be run by the owner of the bot.":
                    emb.WithDescription("Команда предназначена только для создателя бота.");
                    emb.WithFooter($"Создатель бота {_discord.GetUser(BotSettings.hikaruid)}");
                    break;
                case "Value is not a ReportType.":
                    emb.WithDescription($"Значение должно быть из этого списка:\n-{ReportType.ban}\n-{ReportType.kick}\n-{ReportType.mute}\n-{ReportType.tban} - Бан на время\n-{ReportType.tmute} - Мут на время");
                    break;
                case "Value is not a ViolationSystem.":
                    emb.WithDescription($"Значение должно быть из этого списка:\n-{ViolationSystem.WarnSystem}\n-{ViolationSystem.ReportSystem}\n-{ViolationSystem.off}");
                    break;
                case "Value is not a Fishka.":
                    emb.WithDescription($"Значение первого параметра должно быть из этого списка:\n-{User.Fishka.allblack}\n-{User.Fishka.allred}\n-{User.Fishka.allzero}\n-{User.Fishka.black}\n-{User.Fishka.red}\n-{User.Fishka.zero}");
                    break;
                default:
                    emb.WithDescription(error);
                    if (error.ToCharArray().Where(x => x >= 1072 && x <= 1103) == null)
                    {
                        emb.WithFooter("Команда уже отправлена на перевод! Спасибо за ваше любопытство.");
                        await (_discord.GetChannel(BotSettings.SystemMessage) as ITextChannel).SendMessageAsync($"{_discord.GetUser(BotSettings.hikaruid).Mention}", false, emb.Build());
                    }
                    break;
            }



            return emb;
        } // ПЕРЕВОД ОШИБОК


        private static List<SocketUserMessage> MessageList = new List<SocketUserMessage>();

        public static async Task<bool> ChatSystem(SocketCommandContext msg, Channels chnl, string Prefix)
        {
            using (var DBcontext = new DBcontext())
            {
                if (chnl.Spaming)
                {
                    MessageList.Add(msg.Message);
                    MessageList.RemoveAll(x => (DateTime.Now - x.CreatedAt).TotalSeconds >= 5);

                    var mes = MessageList.Where(x => x.Author == msg.User && (x.Author as SocketGuildUser).Guild == msg.Guild);
                    if (mes.Count() > 3)
                    {
                        int CountSumMessage = 0;
                        foreach (var Messes in mes)
                        {
                            if (new SpamChecking().CalculateFuzzyEqualValue(msg.Message.Content, Messes.Content) == 1)
                                CountSumMessage++;

                            else if (msg.Message.Content.Contains(Messes.Content) || Messes.Content.Contains(msg.Message.Content))
                                CountSumMessage++;
                        }
                        if (CountSumMessage > 3)
                        {
                            var CountTempsThisUser = DBcontext.TempUser.AsQueryable().Where(x => x.GuildId == msg.Guild.Id && x.UserId == msg.User.Id).AsEnumerable().Count(x => (x.ToTime - DateTime.Now).TotalSeconds > 30);
                            if (CountTempsThisUser == 0)
                            {
                                var role = await CreateMuteRole(msg.Guild);
                                var vmute = await CheckRoleValid(msg.User as SocketGuildUser, role.voicemuterole, false);
                                var cmute = await CheckRoleValid(msg.User as SocketGuildUser, role.chatmuterole, false);
                                if (vmute != null && cmute != null)
                                {
                                    var TempUser = DBcontext.TempUser.Add(new TempUser() { GuildId = msg.Guild.Id, UserId = msg.User.Id, ToTime = DateTime.Now.AddMinutes(5), Reason = ReportType.tmute }).Entity;
                                    await DBcontext.SaveChangesAsync();
                                    await TaskTimer.StartTempMute(TempUser);
                                }
                            }
                            var messa = await msg.Message.Channel.GetMessagesAsync(CountSumMessage).FlattenAsync();
                            var result = messa.Where(x => x.Author.Id == msg.Message.Author.Id);
                            await (msg.Channel as SocketTextChannel).DeleteMessagesAsync(result);
                            return true;
                        }
                    }
                } // Проверка на спам
                if (chnl.DelCaps && msg.Message.Content.Count(c => char.IsUpper(c)) >= (msg.Message.Content.Length * 0.5))
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
                    var z = Regex.Matches(msg.Message.Content, @"(?:https?:\/\/)?(?:\w+\.)?discord(?:(?:app)?\.com\/invite|\.gg)\/(?<code>[a-z0-9-]+)"); // (https?:\/\/)?(www\.)?(discord\.(gg|io|me|li|com)|discord(app)?\.com\/invite)\/(?<Code>\w+)
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
                if (chnl.DelUrl)
                {
                    string message = msg.Message.Content;

                    if (new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase).Matches(message).Count > 0)
                    {
                        bool success = true;
                        foreach (var word in chnl.csUrlWhiteListList)
                        {
                            
                            if (message.Contains(word))
                                success = false;
                        }
                        
                        if (success)
                        {
                            bool DeleteURL = false;
                            if (chnl.DelUrlImage)
                            {
                                if ((!message.StartsWith("https://tenor.com") || !message.StartsWith("http://tenor.com/")) &&
                                    !message.StartsWith("https://images-ext-1.discordapp.net/") && !message.StartsWith("https://cdn.discordapp.com/") &&
                                    (!message.StartsWith("https://media.discordapp.net") || !message.StartsWith("http://media.discordapp.net")) )
                                {
                                    if (!message.EndsWith(".png") && !message.EndsWith(".gif") && !message.EndsWith(".jpg") && !message.EndsWith(".jpeg"))
                                    {
                                        if (!message.Contains(".png") && !message.Contains(".gif") && !message.Contains(".jpg") && !message.Contains(".jpeg"))
                                        {
                                            DeleteURL = true;
                                        }
                                    }
                                }

                            }


                            if (DeleteURL)
                            {
                                await msg.Message.DeleteAsync();
                                return true;
                            }
                        }

                    }
                } // Отправка ссылки
                return false;
            }

        } // Проверка сообщений
    }
}
