using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
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
        //public static bool loading = false;
        //private static IServiceProvider service = null;

        public SystemLoading(DiscordSocketClient discord)
        {
            _discord = discord;

        } // Подключение компонентов


        public static async Task<Guilds> CreateMuteRole(SocketGuild Context)
        {
            using (var DBcontext = new DBcontext())
            {
                var Guild = Loadingdb._cache.GetOrCreateGuldsCache(Context.Id);
                if (Context.GetRole(Guild.chatmuterole) == null)
                {
                    var MCC = await Context.CreateRoleAsync("ChatMute", new GuildPermissions(mentionEveryone: false), Discord.Color.Red, false, false);

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
        }

        public static async Task LVL(SocketUserMessage message) // ВЫДАЧА ОПЫТА И УРОВНЯ
        {
            using (var DBcontext = new DBcontext())
            {
                var user = message.Author as SocketGuildUser;
                var usr = Loadingdb._cache.GetOrCreateUserCache(user.Id, user.Guild.Id);
                if ((ulong)Math.Sqrt((usr.XP + 10) / 80) > usr.Level)
                {
                    var roles = DBcontext.LVLROLES.AsQueryable().Where(x=>x.guildid == user.Guild.Id).AsEnumerable().Where(x=> x.countlvl >= usr.Level && x.countlvl <= (usr.Level +1)).OrderBy(x => x.countlvl).ToList();
                    if (roles.Count() != 0)
                    {
                        var afterrole = roles.LastOrDefault();
                        if (afterrole != null)
                        {
                            var beforerole = roles.FirstOrDefault();
                            if (beforerole != null && afterrole.roleid != beforerole.roleid)
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

                    ulong amt = 500 + ((500 / 35) * (usr.Level+1));
                    usr.ZeroCoin += amt;
                    await message.Channel.SendMessageAsync("", false, new EmbedBuilder().WithColor(255, 0, 94).WithDescription($"{user.Mention} LEVEL UP")
                                                                                        .AddField("LEVEL", $"{usr.Level + 1}", true)
                                                                                        .AddField("XP", $"{usr.XP + 10}", true)
                                                                                        .AddField("ZeroCoins",$"+{amt}")
                                                                                        .Build());
                }
                else
                {
                    var roles = DBcontext.LVLROLES.AsQueryable().Where(x => x.guildid == user.Guild.Id).AsEnumerable().LastOrDefault(x=>x.countlvl <= usr.Level);
                    if (roles != null)
                    {
                        var role = user.Guild.GetRole(roles.roleid);
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

        public static async Task<bool> ChatSystem(SocketCommandContext msg, Channels chnl,string Prefix)
        {
            using (var DBcontext = new DBcontext())
            {
                if (Loadingdb.loading == false) return true;
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

                            else if(msg.Message.Content.Contains(Messes.Content) || Messes.Content.Contains(msg.Message.Content))
                                CountSumMessage++;
                        }
                        if (CountSumMessage > 3)
                        {
                            var CountTempsThisUser = DBcontext.TempUser.AsQueryable().Where(x => x.guildid == msg.Guild.Id && x.userId == msg.User.Id).AsEnumerable().Count(x=>(x.ToTime - DateTime.Now).TotalSeconds > 30);
                            if (CountTempsThisUser == 0)
                            {
                                var TempUser = DBcontext.TempUser.Add(new TempUser() { guildid = msg.Guild.Id, userId = msg.User.Id, ToTime = DateTime.Now.AddMinutes(5), Reason = "TempMute" }).Entity;
                                await DBcontext.SaveChangesAsync();
                                var role = await CreateMuteRole(msg.Guild);
                                var VoiceMuteRole = msg.Guild.GetRole(role.voicemuterole);
                                var ChatMuteRole = msg.Guild.GetRole(role.chatmuterole);
                                await (msg.User as SocketGuildUser).AddRoleAsync(VoiceMuteRole);
                                await (msg.User as SocketGuildUser).AddRoleAsync(ChatMuteRole);
                                await TaskTimer.StartTempMute(TempUser);
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
                if (chnl.DelUrl)
                {
                    string message = msg.Message.Content;

                    if (new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase).Matches(message).Count > 0)
                    {
                        if (chnl.csUrlWhiteListList.Where(x => x.Contains(message) || message.Contains(x)).Count() == 0)
                        {
                            bool DeleteURL = false;
                            if (chnl.DelUrlImage)
                            {
                                if( !(message.StartsWith("https://tenor.com") || message.StartsWith("http://tenor.com/") ) &&
                                    !(message.StartsWith("https://media.discordapp.net/") || message.StartsWith("http://media.discordapp.net/") ) &&
                                    !message.StartsWith("https://images-ext-1.discordapp.net/") && !message.StartsWith("https://cdn.discordapp.com/"))
                                {
                                    if(!message.EndsWith(".png") && !message.EndsWith(".gif") && !message.EndsWith(".jpg") && !message.EndsWith(".jpeg"))
                                    {
                                        if (!message.Contains("png") && !message.Contains(".gif") && !message.Contains(".jpg") && !message.Contains(".jpeg"))
                                        {
                                            DeleteURL = true;
                                        }
                                    }
                                }

                            }
                            else
                                DeleteURL = true;

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
