using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DarlingBotNet.DataBase;
using DarlingBotNet.Services.Sys;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DarlingBotNet.Services
{
    public class SystemLoading
    {
        public static string WelcomeText =
            "⚡️ Бот по стандарту использует префикс: **h.**\n" +
            "🔨 нашли баг? Пишите - **{0}.bug [описание бага]**\n" +
            "👑 Инструкция бота - https://docs.darlingbot.ru/ \n\n" +
            "🎁Добавить бота на сервер - [КЛИК](https://discord.com/oauth2/authorize?client_id=663381953181122570&scope=bot&permissions=8)\n";

        public static bool loading;
        private static readonly List<Message> MessageList = new List<Message>();


        private readonly DiscordSocketClient _discord;

        public SystemLoading(DiscordSocketClient discord)
        {
            _discord = discord;
        } // Подключение компонентов


        public static async Task<(bool, EmbedBuilder)> CheckText(string report)
        {
            await Task.Delay(1);
            var es = false;
            var emb = new EmbedBuilder().WithColor(255, 0, 94);
            try
            {
                if (report.ToLower() == "kick")
                {
                    es = true;
                }
                else if (report.ToLower() == "ban")
                {
                    es = true;
                }
                else if (report.ToLower() == "mute")
                {
                    es = true;
                }
                else if (report.ToLower().Substring(0, 4) == "tban" || report.ToLower().Substring(0, 5) == "tmute")
                {
                    if (report.ToLower().Substring(0, 4) == "tban")
                        try
                        {
                            if (Convert.ToUInt64(report.ToLower().Substring(4, report.Length - 4)) <= 720)
                                es = true;
                            else
                                emb.WithDescription("Время должно быть не больше 720 минут");
                        }
                        catch (Exception)
                        {
                            emb.WithDescription(
                                    $"Для указания временного бана используйте {report.ToLower().Substring(0, 4)}50 (50 - минут)")
                                .WithFooter("Инструкция о команде - ");
                        }
                    else
                        try
                        {
                            if (Convert.ToUInt64(report.ToLower().Substring(5, report.Length - 5)) <= 720)
                                es = true;
                            else
                                emb.WithDescription("Время должно быть не больше 720 минут");
                        }
                        catch (Exception)
                        {
                            emb.WithDescription(
                                    $"Для указания временного мута используйте {report.ToLower().Substring(0, 5)}50 (50 - минут)")
                                .WithFooter("Инструкция о команде - ");
                        }
                }
                else
                {
                    emb.WithDescription("Используйте эти нарушения ban,kick,mute,tmute,tban. Инструкция к команде");
                }

                return (es, emb);
            }
            catch (Exception)
            {
                emb.WithDescription("Используйте эти нарушения ban,kick,mute,tmute,tban. Инструкция к команде");
                return (es, emb);
            }
        }

        public static async Task GuildCheck(DiscordSocketClient _discord)
        {
            await using (var context = new DBcontext())
            {
                context.Database.Migrate();
                context.SaveChanges();
                //   context.Dispose();
            }

            var Guilds =
                _discord.Guilds.Where(z => new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == z.Id) == null);
            foreach (var Guild in Guilds) await GuildCreate(Guild);


            var NullGuilds = new EEF<Guilds>(new DBcontext()).Get(x => _discord.GetGuild(x.guildId) == null);
            foreach (var glds in NullGuilds) await GuildDelete(glds);


            await ChannelCheck(_discord);
        } // МИГРАЦИЯ, ПРОВЕРКА ГИЛЬДИЙ

        public static async Task ChannelCheck(DiscordSocketClient _discord)
        {
            var Guilds = new EEF<Guilds>(new DBcontext()).Get(x => x.Leaved == false);
            foreach (var Guild in Guilds)
            {
                var Channels = new EEF<Channels>(new DBcontext()).Get(x =>
                    x.guildid == Guild.guildId && _discord.GetGuild(Guild.guildId).GetTextChannel(x.channelid) == null);
                new EEF<Channels>(new DBcontext()).RemoveRange(Channels);
                foreach (var Channel in Channels)
                {
                    var ChannelsRole = new EEF<EmoteClick>(new DBcontext()).Get(x =>
                        x.guildid == Guild.guildId && x.channelid == Channel.channelid);
                    new EEF<EmoteClick>(new DBcontext()).RemoveRange(ChannelsRole);
                } // Проверка каналов и удаление их при отсутствии

                var LVLROLE = new EEF<LVLROLES>(new DBcontext()).Get(x =>
                    x.guildid == Guild.guildId && _discord.GetGuild(Guild.guildId).GetRole(x.roleid) == null);
                new EEF<LVLROLES>(new DBcontext()).RemoveRange(LVLROLE); // Delete Role

                foreach (var chnl in _discord.GetGuild(Guild.guildId).TextChannels)
                    if (new EEF<Channels>(new DBcontext()).Get(
                        x => x.guildid == Guild.guildId && x.channelid == chnl.Id) == null)
                        await ChannelCreate(chnl);

                var users = new EEF<Users>(new DBcontext()).Get(x =>
                    x.guildId == Guild.guildId && _discord.GetGuild(Guild.guildId).GetUser(x.userid) == null);
                foreach (var user in users)
                {
                    user.Leaved = true;
                    new EEF<Users>(new DBcontext()).Update(user);
                }

                await Privates.CheckPrivate(Guild.guildId, _discord.GetGuild(Guild.guildId));
                CheckTempUser(Guild, _discord.GetGuild(Guild.guildId));
            }
        } // ПРОВЕРКА КАНАЛОВ И РОЛЕЙ

        private static async void CheckTempUser(Guilds glds, SocketGuild guild)
        {
            var users = new EEF<TempUser>(new DBcontext()).Get(x => x.guildId == glds.guildId);
            foreach (var user in users) await UserMuteTime(user, guild);
            loading = true;
        }

        private static async Task UserMuteTime(TempUser user, SocketGuild guild)
        {
            if (user.ToTime < DateTime.Now)
                await Task.Delay(user.ToTime.Millisecond);
            var usr = guild.GetUser(user.userId);

            if (user.Reason.Contains("tban"))
            {
                await guild.RemoveBanAsync(usr);
            }
            else
            {
                var gld = await CreateMuteRole(guild);
                var cmute = guild.GetRole(gld.chatmuterole);
                var vmute = guild.GetRole(gld.voicemuterole);

                if (gld.ViolationSystem == 2)
                {
                    if (usr.Roles.Contains(cmute))
                        await usr.RemoveRoleAsync(cmute);
                    if (usr.Roles.Contains(vmute))
                        await usr.RemoveRoleAsync(vmute);
                }
            }

            new EEF<TempUser>(new DBcontext()).Remove(user);
        }

        public static async Task<Guilds> CreateMuteRole(SocketGuild Context)
        {
            var Guild = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == Context.Id);
            if (Guild.chatmuterole == 0 || Context.GetRole(Guild.chatmuterole) == null)
            {
                var MCC = await Context.CreateRoleAsync("MuteClownChat", new GuildPermissions(mentionEveryone: false),
                    Color.Red, false, false, RequestOptions.Default);

                foreach (var TC in Context.TextChannels)
                    await TC.AddPermissionOverwriteAsync(MCC,
                        new OverwritePermissions(sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny));
                Guild.chatmuterole = MCC.Id;
            }

            if (Guild.voicemuterole == 0 || Context.GetRole(Guild.voicemuterole) == null)
            {
                var MCV = await Context.CreateRoleAsync("MuteClownVoice", new GuildPermissions(mentionEveryone: false),
                    Color.Red, false, false, RequestOptions.Default);

                foreach (var VC in Context.VoiceChannels)
                    await VC.AddPermissionOverwriteAsync(MCV,
                        new OverwritePermissions(speak: PermValue.Deny, connect: PermValue.Deny));
                Guild.voicemuterole = MCV.Id;
            }

            new EEF<Guilds>(new DBcontext()).Update(Guild);
            return Guild;
        }

        public static async Task GuildDelete(Guilds Guild)
        {
            await Task.Delay(1);
            var channels = new EEF<Channels>(new DBcontext()).Get(x => x.guildid == Guild.guildId);
            var lvlrole = new EEF<LVLROLES>(new DBcontext()).Get(x => x.guildid == Guild.guildId);
            var emoteclick = new EEF<EmoteClick>(new DBcontext()).Get(x => x.guildid == Guild.guildId);
            var privatechannels = new EEF<PrivateChannels>(new DBcontext()).Get(x => x.guildid == Guild.guildId);
            var Warns = new EEF<Warns>(new DBcontext()).Get(x => x.guildId == Guild.guildId);
            var TempUser = new EEF<TempUser>(new DBcontext()).Get(x => x.guildId == Guild.guildId);

            new EEF<Channels>(new DBcontext()).RemoveRange(channels);
            new EEF<LVLROLES>(new DBcontext()).RemoveRange(lvlrole);
            new EEF<EmoteClick>(new DBcontext()).RemoveRange(emoteclick);
            new EEF<PrivateChannels>(new DBcontext()).RemoveRange(privatechannels);
            new EEF<Warns>(new DBcontext()).RemoveRange(Warns);
            new EEF<TempUser>(new DBcontext()).RemoveRange(TempUser);
            Guild.Leaved = true;
            new EEF<Guilds>(new DBcontext()).Update(Guild);
        } // УДАЛЕНИЕ ИНФОРМАЦИИ ГИЛЬДИИ

        public static async Task GuildCreate(SocketGuild Guild)
        {
            new EEF<Guilds>(new DBcontext()).Create(new Guilds
                {guildId = Guild.Id, Prefix = BotSettings.Prefix, GiveXPnextChannel = true});
            await ChannelCreateRange(Guild.TextChannels);
        } // СОЗДАНИЕ ГИЛЬДИИ И КАНАЛОВ

        public static async Task ChannelCreateRange(IEnumerable<SocketGuildChannel> Channels)
        {
            await Task.Delay(1);
            var chnls = new List<Channels>();
            foreach (var TextChannel in Channels)
                chnls.Add(new Channels
                    {guildid = TextChannel.Guild.Id, channelid = TextChannel.Id, GiveXP = true, UseCommand = true});
            new EEF<Channels>(new DBcontext()).CreateRange(chnls);
        } // СОЗДАНИЕ RANGE КАНАЛОВ

        public static async Task<Channels>
            ChannelCreate(SocketGuildChannel Channels) // СОЗДАНИЕ КАНАЛА И ЕГО ВОЗВРАЩЕНИЕ
        {
            await Task.Delay(1);
            var Guild = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == Channels.Guild.Id);
            var chnl = new EEF<Channels>(new DBcontext()).GetF(x =>
                x.channelid == Channels.Id && x.guildid == Channels.Guild.Id);
            if (chnl == null)
            {
                new EEF<Channels>(new DBcontext()).Create(new Channels
                {
                    channelid = Channels.Id, guildid = Channels.Guild.Id, GiveXP = Guild.GiveXPnextChannel,
                    UseCommand = true
                });

                return new EEF<Channels>(new DBcontext()).GetF(x =>
                    x.channelid == Channels.Id && x.guildid == Channels.Guild.Id);
            }

            return chnl;
        }

        public static async Task<Users> CreateUser(SocketUser user)
        {
            await Task.Delay(1);
            var usr = new EEF<Users>(new DBcontext()).GetF(x =>
                x.userid == user.Id && x.guildId == (user as SocketGuildUser).Guild.Id);
            if (usr == null)
                return new EEF<Users>(new DBcontext()).Create(new Users
                    {guildId = (user as SocketGuildUser).Guild.Id, userid = user.Id, ZeroCoin = 1000});
            return usr;
        } // СОЗДАНИЕ ПОЛЬЗОВАТЕЛЯ

        public static async Task LVL(SocketUserMessage message) // ВЫДАЧА ОПЫТА И УРОВНЯ
        {
            var user = message.Author as SocketGuildUser;
            var usr = new EEF<Users>(new DBcontext()).GetF(x => x.userid == user.Id && x.guildId == user.Guild.Id);
            if ((ulong) Math.Sqrt((usr.XP + 10) / 80) > usr.Level)
            {
                var beforerole =
                    new EEF<LVLROLES>(new DBcontext()).GetF(x => x.guildid == user.Guild.Id && x.countlvl == usr.Level);
                var aftererole =
                    new EEF<LVLROLES>(new DBcontext()).GetF(x =>
                        x.guildid == user.Guild.Id && x.countlvl == usr.Level + 1);

                if (beforerole != null)
                {
                    var befrole = user.Guild.GetRole(beforerole.roleid);
                    if (befrole != null)
                        if (user.Roles.Contains(befrole))
                            await user.RemoveRoleAsync(befrole);
                }

                if (aftererole != null)
                {
                    var aftrole = user.Guild.GetRole(aftererole.roleid);
                    if (aftrole != null)
                        if (user.Roles.Contains(aftrole))
                            await user.AddRoleAsync(aftrole);
                }


                await message.Channel.SendMessageAsync("", false, new EmbedBuilder().WithColor(255, 0, 94)
                    .WithDescription($"{user.Mention} LEVEL UP")
                    .AddField("LEVEL", $"{usr.Level + 1}", true)
                    .AddField("XP", $"{usr.XP + 10}", true).Build());
            }

            usr.XP += 10;
            new EEF<Users>(new DBcontext()).Update(usr);
        } // Получение опыта

        public static async Task UserJoinCheck(SocketGuildUser user) // ВЫДАЧА ОПЫТА И УРОВНЯ
        {
            var usr = CreateUser(user).Result;
            var glds = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == user.Guild.Id);
            var role = new EEF<LVLROLES>(new DBcontext()).GetF(x =>
                x.guildid == user.Guild.Id && x.countlvl == usr.Level);
            if (role != null)
                if (user.Guild.GetRole(role.roleid) != null)
                    await user.AddRoleAsync(user.Guild.Roles.FirstOrDefault(x => x.Id == role.roleid));
        } // Получение опыта

        public static async Task<EmbedBuilder> GetError(string error, DiscordSocketClient _discord)
        {
            var emb = new EmbedBuilder().WithAuthor("Ошибка");
            switch (error)
            {
                case "The input text has too many parameters.":
                    emb.WithDescription("Текст имеет много параметров")
                        .WithFooter("Параметры - то что вы пишите после самой команды.");
                    break;
                case "The input text has too few parameters.":
                    emb.WithDescription("Текст имеет мало параметров")
                        .WithFooter("Параметры - то что вы пишите после самой команды.");
                    break;
                case "User not found.":
                    emb.WithDescription("Введенный пользователь не найден");
                    break;
                case "Channel not found.":
                    emb.WithDescription("Введенный канал не найден");
                    break;
                case "Role not found.":
                    emb.WithDescription("Введенная роль не найдена.")
                        .WithFooter("Введите роль в таком формате @everyone");
                    break;
                case "Unknown command.":
                    emb.WithDescription("Команда которую вы ввели не найдена")
                        .WithFooter("Возможно вы просто ее неправильно написали.");
                    break;
                case "Failed to parse Int.":
                    emb.WithDescription("Параметр должен иметь цифру")
                        .WithFooter("Цифру от -2 147 483 648 до 2 147 483 647");
                    break;
                case "Failed to parse UInt.":
                    emb.WithDescription("Параметр должен иметь цифру").WithFooter("Цифру от 0 до 4 294 967 295");
                    break;
                case "Failed to parse long.":
                    emb.WithDescription("Параметр должен иметь цифру")
                        .WithFooter("Цифру от -9 223 372 036 854 775 808 до 9 223 372 036 854 775 807");
                    break;
                case "Failed to parse ulong.":
                    emb.WithDescription("Параметр должен иметь цифру")
                        .WithFooter("Цифру от 0 до 18 446 744 073 709 551 615");
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
                    emb.WithDescription("Введенный параметр не является роль.")
                        .WithFooter("Введите роль в таком формате @everyone");
                    break;
                case "Command can only be run by the owner of the bot.":
                    emb.WithDescription("Команда предназначена только для создателя бота.")
                        .WithFooter("Создатель бота @h1kka.#2627");
                    break;
                case "Команда отключена создателем сервера.":
                    emb.WithDescription(error);
                    break;
                case "Вы не являетесь создателем сервера чтобы использовать эту команду":
                    emb.WithDescription(error);
                    break;
                default:
                    emb.WithDescription(error)
                        .WithFooter("Команда уже отправлена на перевод! Спасибо за ваше любопытство.");
                    await (_discord.GetChannel(BotSettings.SystemMessage) as ITextChannel).SendMessageAsync(
                        $"{_discord.GetUser(BotSettings.hikaruid).Mention}", false, emb.Build());
                    break;
            }

            return emb;
        } // ПЕРЕВОД ОШИБОК

        public static async Task<bool> ChatSystem(SocketCommandContext msg)
        {
            if (loading == false) return true;
            await CreateUser(msg.User as SocketGuildUser);
            var glds = new EEF<Channels>(new DBcontext()).GetF(x =>
                x.guildid == (msg.User as SocketGuildUser).Guild.Id && x.channelid == msg.Channel.Id);
            if (glds.Spaming)
            {
                MessageList.Add(new Message
                {
                    Messages = msg.Message.Content,
                    UserId = msg.User.Id,
                    GuildId = (msg.User as SocketGuildUser).Guild.Id,
                    Data = DateTime.Now,
                    MessagesId = msg.Message.Id
                });

                var mew = MessageList.Where(x => (DateTime.Now - x.Data).TotalSeconds >= 5).ToList();
                foreach (var Messagez in mew) MessageList.Remove(Messagez);

                mew = new List<Message>();
                var mes = MessageList.Where(x =>
                    x.UserId == msg.User.Id && x.GuildId == (msg.User as SocketGuildUser).Guild.Id);
                foreach (var Messes in mes)
                    if (new SpamChecking().CalculateFuzzyEqualValue(msg.Message.Content, Messes.Messages) == 1)
                        mew.Add(Messes);
                if (mew.Count() > 3)
                {
                    await (msg.Channel as SocketTextChannel).AddPermissionOverwriteAsync(msg.Message.Author,
                        new OverwritePermissions(sendMessages: PermValue.Deny));
                    var messa = await msg.Message.Channel.GetMessagesAsync(mew.Count()).FlattenAsync();
                    var result = messa.Where(x => x.Author.Id == msg.Message.Author.Id);
                    await (msg.Channel as SocketTextChannel).DeleteMessagesAsync(result);
                    await (msg.Channel as SocketTextChannel).RemovePermissionOverwriteAsync(msg.Message.Author);
                    return true;
                }
            } // Проверка на спам

            if (glds.SendUrl)
                if (new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase)
                    .Matches(msg.Message.Content).Count > 0)
                {
                    await msg.Message.DeleteAsync();
                    return true;
                }

            if (glds.SendCaps && msg.Message.Content.Count(c => char.IsUpper(c)) == msg.Message.Content.Length)
            {
                await msg.Message.DeleteAsync();
                return true;
            } // КАПС СООБЩЕНИЯ

            if (glds.SendBadWord && glds.BadWordList != null)
            {
                var x = Regex.Matches(msg.Message.Content, @"\b[\p{L}]+\b").Select(match => match.Value.ToLower())
                    .Where(word => glds.BadWordList.Contains(word)).Any();
                if (x)
                {
                    await msg.Message.DeleteAsync();
                    return true;
                }
            }

            if (glds.antiMat && msg.Channel.Id == 637305727131844619)
            {
                //if (mat.Content.ToLower().Contains(mt.ToLower())) 
                //{ 
                //    await msg.Channel.SendMessageAsync($"найден Мат {msg.Content}"); await msg.DeleteAsync(); return true; 
                //}
            } // АНТИ МАТ

            if (glds.InviteMessage)
            {
                var z = Regex.Matches(msg.Message.Content,
                    @"(https?:\/\/)?(www\.)?(discord\.(gg|io|me|li|com)|discord(app)?\.com\/invite)\/(?<Code>\w+)");
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
        } // Проверка сообщений

        private class Message
        {
            public ulong MessagesId { get; set; }
            public ulong UserId { get; set; }
            public ulong GuildId { get; set; }
            public DateTime Data { get; set; }
            public string Messages { get; set; }
        }
    }
}