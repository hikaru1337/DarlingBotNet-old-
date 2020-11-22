using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.Modules;
using DarlingBotNet.Services.Sys;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DarlingBotNet.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        private IMemoryCache _cache;

        public CommandHandler(DiscordSocketClient discord, CommandService commands, IServiceProvider provider, IMemoryCache memoryCache)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _cache = memoryCache;

            _discord.JoinedGuild += JoinedGuild;
            _discord.LeftGuild += LeftGuild;
            _discord.MessageReceived += MessageReceived;
            _discord.MessageDeleted += messsagedelete;
            _discord.MessageUpdated += messageupdate;
            _discord.RoleDeleted += roledeleted;
            _discord.Ready += Ready;
            _discord.UserVoiceStateUpdated += UserVoiceUpdate;
            _discord.UserJoined += UserJoined;
            _discord.UserLeft += UserLeft;
            _discord.UserBanned += UserBan;
            _discord.UserUnbanned += UserUnBan;
            _discord.ChannelCreated += OnChannelCreateAsync;
            _discord.ChannelDestroyed += OnChannelDestroyedAsync;
            _discord.ReactionAdded += ReactAdd;
            _discord.ReactionRemoved += ReactRem;
            _commands.CommandExecuted += CommandErrors;
        } // Подключение компонентов


        private async Task CommandErrors(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            using (var DBcontext = new DBcontext())
            {
                var GuildPrefix = DBcontext.Guilds.FirstOrDefault(x=>x.GuildId == context.Guild.Id).Prefix;
                if (!string.IsNullOrEmpty(result?.ErrorReason))
                {
                    var emb = OtherSettings.GetError(result.ErrorReason, _discord, GuildPrefix, command.IsSpecified ? command.Value : null).Result;
                    await context.Channel.SendMessageAsync("", false, emb.Build());
                }
            }
        }

        private async Task roledeleted(SocketRole role)
        {
            using (var DBcontext = new DBcontext())
            {
                var EmoteClick = DBcontext.EmoteClick.AsQueryable().Where(x => x.GuildId == role.Guild.Id && x.RoleId == role.Id);
                if (EmoteClick != null) DBcontext.EmoteClick.RemoveRange(EmoteClick);

                var LVLROLE = DBcontext.LVLROLES.FirstOrDefault(x => x.GuildId == role.Guild.Id && x.RoleId == role.Id);
                if (LVLROLE != null) DBcontext.LVLROLES.Remove(LVLROLE);

                if (EmoteClick != null || LVLROLE != null)
                    await DBcontext.SaveChangesAsync();

            }
        }

        private async Task OnChannelDestroyedAsync(SocketChannel channel) // Удаление каналов
        {
            var TextChannel = channel as SocketTextChannel;
            if (TextChannel != null)
            {
                using (var DBcontext = new DBcontext())
                {
                    var chnl = DBcontext.Channels.FirstOrDefault(x => x.GuildId == TextChannel.Guild.Id && x.ChannelId == channel.Id);
                    if (chnl != null) DBcontext.Channels.Remove(chnl);

                    var ChannelTask = DBcontext.Tasks.AsQueryable().Where(x => x.GuildId == TextChannel.Guild.Id && x.ChannelId == channel.Id);
                    if (ChannelTask != null) DBcontext.Tasks.RemoveRange(ChannelTask);

                    var emjmess = DBcontext.EmoteClick.AsQueryable().Where(x => x.GuildId == TextChannel.Guild.Id && x.ChannelId == channel.Id);
                    if (emjmess != null) DBcontext.EmoteClick.RemoveRange(emjmess);

                    if (chnl != null || emjmess != null || ChannelTask != null)
                        await DBcontext.SaveChangesAsync();
                }
            }
        }

        public class Checking
        {
            public ulong messid { get; set; }
            public ulong userid { get; set; }
            public ushort clicked { get; set; }
        }

        private async Task ReactAdd(Cacheable<IUserMessage, ulong> mess, ISocketMessageChannel chnl, SocketReaction emj) // Проверка на эмодзи
        {
            if (chnl.GetUserAsync(emj.User.Value.Id).Result.IsBot) 
                return;

            if (emj.Emote.Name == "✅" || emj.Emote.Name == "❎")
            {
                var MarryYes = User.list.FirstOrDefault(x => x.messid == mess.Id && x.userid == emj.UserId);
                if (MarryYes != null)
                {
                    if (emj.Emote.Name == "✅")
                        MarryYes.clicked = 2; // Accept
                    else
                        MarryYes.clicked = 1; // Denied
                }
            } // Marryed
            else if (emj.Emote.Name == "◀️" || emj.Emote.Name == "▶️")
            {
                var SlideClick = Clan.slide.FirstOrDefault(x => x.messid == mess.Id && x.userid == emj.UserId);
                if (SlideClick != null)
                {
                    if (emj.Emote.Name == "◀️")
                        SlideClick.clicked = 2; // Back
                    else
                        SlideClick.clicked = 1; // Forward
                }
            } // Slider
            else if (emj.Emote.Name == "1️⃣" || emj.Emote.Name == "2️⃣" || emj.Emote.Name == "3️⃣" || emj.Emote.Name == "4️⃣" || emj.Emote.Name == "5️⃣" || 
                     emj.Emote.Name == "6️⃣" || emj.Emote.Name == "7️⃣" || emj.Emote.Name == "8️⃣" || emj.Emote.Name == "9️⃣" || emj.Emote.Name == "🔟")
            {
                var SlideClick = User.BuyRole.FirstOrDefault(x => x.MessageId == mess.Id && x.userid == emj.UserId);
                if (SlideClick != null)
                {
                    switch (emj.Emote.Name)
                    {
                        case "1️⃣": SlideClick.SelectItem = 1; break;
                        case "2️⃣": SlideClick.SelectItem = 2; break;
                        case "3️⃣": SlideClick.SelectItem = 3; break;
                        case "4️⃣": SlideClick.SelectItem = 4; break;
                        case "5️⃣": SlideClick.SelectItem = 5; break;
                        case "6️⃣": SlideClick.SelectItem = 6; break;
                        case "7️⃣": SlideClick.SelectItem = 7; break;
                        case "8️⃣": SlideClick.SelectItem = 8; break;
                        case "9️⃣": SlideClick.SelectItem = 9; break;
                        case "🔟": SlideClick.SelectItem = 10; break;
                    }
                }
            }
                
            if (mess.GetOrDownloadAsync().Result != null)
                await GetOrRemoveRole(mess, chnl, emj, false);
        }

        private async Task ReactRem(Cacheable<IUserMessage, ulong> mess, ISocketMessageChannel chnl, SocketReaction emj)
        {
            if (chnl.GetUserAsync(emj.User.Value.Id).Result.IsBot) 
                return;

            if (mess.GetOrDownloadAsync().Result != null)
                await GetOrRemoveRole(mess, chnl, emj, true);
        }

        private async Task GetOrRemoveRole(Cacheable<IUserMessage, ulong> mess, ISocketMessageChannel chnl, SocketReaction emj, bool getOrRemove)
        {
            using (var DBcontext = new DBcontext())
            {
                var mes = DBcontext.EmoteClick.AsQueryable().Where(x => x.GuildId == (chnl as SocketGuildChannel).Guild.Id).AsEnumerable()
                                                            .FirstOrDefault(x => x.MessageId == mess.Id && x.ChannelId == chnl.Id && emj.Emote.Name == Emote.Parse(x.emote).Name);
                if (mes != null)
                {
                    var usr = emj.User.Value as SocketGuildUser;
                    var role = usr.Guild.GetRole(mes.RoleId);
                    if (role != null)
                    {
                        var rolepos = usr.Guild.CurrentUser.Roles.FirstOrDefault(x => x.Position > role.Position);
                        if (rolepos != null)
                        {
                            if(getOrRemove)
                            {
                                if(!mes.get)
                                {
                                    if (usr.Roles.FirstOrDefault(x => x.Id == role.Id) != null)
                                        await OtherSettings.CheckRoleValid(usr, role.Id, true);
                                }
                            }
                            else
                            {
                                if (!mes.get)
                                {
                                    if (usr.Roles.FirstOrDefault(x => x.Id == role.Id) == null)
                                        await OtherSettings.CheckRoleValid(usr, role.Id, false);
                                }
                                else
                                {
                                    if (usr.Roles.FirstOrDefault(x => x.Id == role.Id) != null)
                                        await OtherSettings.CheckRoleValid(usr, role.Id, true);
                                }
                            }
                        }
                    }
                }
            }
        } // Проверка эмодзи в событии ReactRem и ReactAdd

        private async Task OnChannelCreateAsync(SocketChannel chnl) // Создание каналов
        {
            if (chnl as SocketTextChannel != null)
            {
                using (var DBcontext = new DBcontext())
                {
                    var GuildGiveXPnextChannel = DBcontext.Guilds.FirstOrDefault(x => x.GuildId == (chnl as SocketTextChannel).Guild.Id).GiveXPnextChannel;
                    DBcontext.Channels.Add(new Channels() { GuildId = (chnl as SocketTextChannel).Guild.Id, ChannelId = chnl.Id, GiveXP = GuildGiveXPnextChannel, UseCommand = true });
                    await DBcontext.SaveChangesAsync();
                }
            }
        }

        private async Task messageupdate(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channelmes)
        {
            using (var DBcontext = new DBcontext())
            {
                IMessage msg = await before.GetOrDownloadAsync();
                if (msg == null || msg.Author.IsBot || msg.Content.Length > 1023 || after.Content.Length > 1023) return;
                var chnl = channelmes as SocketTextChannel;
                var GuildChannelmesdel = DBcontext.Guilds.FirstOrDefault(x => x.GuildId == chnl.Guild.Id).mesdelchannel;
                var ChannelMessageUpdate = chnl.Guild.GetTextChannel(GuildChannelmesdel);
                if (ChannelMessageUpdate != null)
                {
                    var builder = new EmbedBuilder().WithColor(255, 0, 94).WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                .WithFooter($"id: {msg.Author.Id}", msg.Author.GetAvatarUrl())
                                .WithAuthor(" - изменение сообщения", msg.Author.GetAvatarUrl())
                                .AddField("Прошлое Сообщение", string.IsNullOrWhiteSpace(msg.Content) ? "-" : msg.Content)
                                .AddField("Новое Сообщение", string.IsNullOrWhiteSpace(after.Content) ? "-" : after.Content)
                                .AddField("Отправитель", string.IsNullOrWhiteSpace(after.Content) ? "-" : msg.Author.Mention, true)
                                .AddField("Канал", $"{chnl.Mention}", true);
                    await ChannelMessageUpdate.SendMessageAsync("", false, builder.Build());
                }
            }

        } // Изменение сообщения Logging

        private async Task messsagedelete(Cacheable<IMessage, ulong> cachemess, ISocketMessageChannel channelmes)
        {
            using (var DBcontext = new DBcontext())
            {
                IMessage msg = await cachemess.GetOrDownloadAsync();
                if (msg == null || msg.Author.IsBot || msg.Content.Length > 1023) 
                    return;
                var chnl = channelmes as SocketGuildChannel;
                

                var emjmess = DBcontext.EmoteClick.FirstOrDefault(x => x.GuildId == chnl.Guild.Id && x.ChannelId == channelmes.Id && x.MessageId == msg.Id);
                if (emjmess != null) 
                    DBcontext.EmoteClick.Remove(emjmess);

                var GuildChannelDelMes = DBcontext.Guilds.FirstOrDefault(x => x.GuildId == chnl.Guild.Id).mesdelchannel;
                var MessageDeleteChannel = chnl.Guild.GetTextChannel(GuildChannelDelMes);
                if (MessageDeleteChannel != null)
                {
                    var builder = new EmbedBuilder().WithColor(255, 0, 94) // отсутствие каких либо данных
                                                .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                                .WithFooter($"id: {msg.Author.Id}", msg.Author.GetAvatarUrl())
                                                .WithAuthor(" - удаление сообщения", msg.Author.GetAvatarUrl())
                                                .AddField("Сообщение Удалено", string.IsNullOrWhiteSpace(msg.Content) ? "-" : msg.Content)
                                                .AddField("Отправитель", $"{msg.Author.Mention}", true)
                                                .AddField("Канал", $"{(channelmes as SocketTextChannel).Mention}", true);
                    await MessageDeleteChannel.SendMessageAsync("", false, builder.Build());
                }
            }
        }  // Удаление сообщения Logging

        private async Task UserUnBan(SocketUser user, SocketGuild guild)
        {
            await BanOrUnBan(user, guild, false);
        }

        private async Task UserBan(SocketUser user, SocketGuild guild)
        {
            await BanOrUnBan(user, guild, true);
        }

        private async Task BanOrUnBan(SocketUser user, SocketGuild guild, bool Ban)
        {
            using (var DBcontext = new DBcontext())
            {
                var glds = DBcontext.Guilds.FirstOrDefault(x => x.GuildId == guild.Id);
                var ChannelForMessage = guild.GetChannel(Ban ? glds.banchannel : glds.unbanchannel);
                if (ChannelForMessage != null)
                {
                    var builder = new EmbedBuilder().WithColor(255, 0, 94)
                                                    .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                                    .WithFooter($"id: {user.Id}")
                                                    .WithAuthor(user)
                                                    .AddField($"Пользователь {(Ban ? "забанен" : "разбанен")}", user.Mention);
                    await (ChannelForMessage as ISocketMessageChannel).SendMessageAsync("", false, builder.Build());
                }
            }
        }

        private async Task UserLeft(SocketGuildUser user)
        {
            using (var DBcontext = new DBcontext())
            {
                var glds = DBcontext.Guilds.FirstOrDefault(x => x.GuildId == user.Guild.Id);
                var LeftChannel = user.Guild.GetTextChannel(glds.leftchannel);
                if (LeftChannel != null)
                {
                    var builder = new EmbedBuilder().WithColor(255, 0, 94)
                                                    .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                                    .WithAuthor($"Пользователь вышел: {user.Username}", user.GetAvatarUrl())
                                                    .WithDescription($"Имя: {user.Mention}\n Участников: {user.Guild.MemberCount}\nid: {user.Id}\nАккаунт создан: {user.CreatedAt.ToUniversalTime().ToString("dd.MM.yyyy HH:mm")}");
                    await LeftChannel.SendMessageAsync("", false, builder.Build());
                }
                var LeaveChanne = user.Guild.GetTextChannel(glds.LeaveChannel);
                if (LeaveChanne != null)
                {
                    var mes = MessageBuilder.EmbedUserBuilder(glds.LeaveMessage);
                    mes.Item1.Description = mes.Item1.Description.Replace("%user%", user.Mention);
                    await LeaveChanne.SendMessageAsync(mes.Item2, false, mes.Item1.Build());
                }

                var usr = DBcontext.Users.FirstOrDefault(x => x.UserId == user.Id && x.GuildId == user.Guild.Id);
                if (usr != null)
                {
                    usr.Leaved = true;
                    await DBcontext.SaveChangesAsync();
                }
            }

        }  // Выход пользователя Logging


        private class UserRaid
        {
            public SocketUser User { get; set; }
            public SocketGuild Guild { get; set; }
            public DateTime Data { get; set; }
        }
        private static List<UserRaid> UserRaidList = new List<UserRaid>();


        private async Task UserJoined(SocketGuildUser user)
        {
            using (var DBcontext = new DBcontext())
            {
                var Guild = DBcontext.Guilds.FirstOrDefault(x => x.GuildId == user.Guild.Id);
                await OtherSettings.CheckRoleValid(user, Guild.WelcomeRole, false);
                    

                if (Guild.RaidStop)
                {
                    if (Guild.RaidMuted > DateTime.Now)
                    {
                        await OtherSettings.CheckRoleValid(user, Guild.chatmuterole, false);
                        await OtherSettings.CheckRoleValid(user, Guild.voicemuterole, false);
                    }
                    else
                    {
                        UserRaidList.Add(new UserRaid()
                        {
                            User = user,
                            Guild = user.Guild,
                            Data = DateTime.Now,
                        });

                        UserRaidList.RemoveAll(x => (DateTime.Now - x.Data).TotalSeconds >= Guild.RaidTime);
                        var mes = UserRaidList.Where(x => x.User == user && x.Guild == user.Guild);
                        if (mes.Count() > Guild.RaidUserCount)
                        {
                            Guild.RaidMuted = DateTime.Now.AddSeconds(30);
                            foreach (var userz in mes)
                            {
                                await OtherSettings.CheckRoleValid(user, Guild.chatmuterole, false);
                                await OtherSettings.CheckRoleValid(user, Guild.voicemuterole, false);
                            }
                        }
                    }
                }
                var usr = DBcontext.Users.FirstOrDefault(x => x.UserId == user.Id && x.GuildId == user.Guild.Id);
                if (!user.IsBot)
                {
                    if (usr != null)
                    {
                        usr.Leaved = false;
                        usr.BankTimer = DateTime.Now.AddDays(7);
                        DBcontext.Users.Update(usr);
                        await DBcontext.SaveChangesAsync();
                        var ClanRoleId = DBcontext.Clans.FirstOrDefault(x => x.Id == usr.ClanId);
                        if(ClanRoleId != null)
                            await OtherSettings.CheckRoleValid(user, ClanRoleId.ClanRole, false);


                        var DBroles = DBcontext.LVLROLES.AsQueryable().Where(x => x.GuildId == user.Guild.Id).AsEnumerable().Where(x => x.CountLvl <= usr.Level);
                        if (DBroles != null)
                        {
                            var DBrole = DBroles.OrderBy(x => x.CountLvl).LastOrDefault();
                            if (DBrole != null)
                            {
                                await OtherSettings.CheckRoleValid(user, DBrole.RoleId, false);
                            }
                        }

                    }

                    if (Guild.WelcomeDMmessage != null)
                    {
                        var mes = MessageBuilder.EmbedUserBuilder(Guild.WelcomeDMmessage);
                        mes.Item1.Description = mes.Item1.Description.Replace("%user%", user.Mention);
                        try
                        {
                            await user.SendMessageAsync(mes.Item2, false, mes.Item1.Build());
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                var JoinedChannelDiscord = user.Guild.GetTextChannel(Guild.joinchannel);
                if (JoinedChannelDiscord != null)
                {
                    var builder = new EmbedBuilder().WithColor(255, 0, 94)
                                                    .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                                    .WithAuthor($"Пользователь присоединился: {user.Username}", user.GetAvatarUrl())
                                                    .WithDescription($"Имя: {user.Mention}\n" +
                                                    $"Участников: {user.Guild.MemberCount}\n" +
                                                    $"id: {user.Id}\n" +
                                                    $"Аккаунт создан: {user.CreatedAt.ToUniversalTime().ToString("dd.MM.yyyy HH:mm")}");
                    await JoinedChannelDiscord.SendMessageAsync("", false, builder.Build());
                }
                var WelcomeChannelDiscord = user.Guild.GetTextChannel(Guild.WelcomeChannel);
                if (WelcomeChannelDiscord != null)
                {
                    var mes = MessageBuilder.EmbedUserBuilder(Guild.WelcomeMessage);
                    mes.Item1.Description = mes.Item1.Description.Replace("%user%", user.Mention);
                    await WelcomeChannelDiscord.SendMessageAsync(mes.Item2, false, mes.Item1.Build());
                }
            }
        }  // Вход пользователя Logging

        private async Task UserVoiceUpdate(SocketUser user, SocketVoiceState ot, SocketVoiceState to)
        {
            using (var DBcontext = new DBcontext())
            {
                var usr = user as SocketGuildUser;
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                    .WithFooter(x => x.WithText($"id: {user.Id}")).AddField($"Пользователь", $"{user.Mention}", true);
                var glds = DBcontext.Guilds.FirstOrDefault(x => x.GuildId == usr.Guild.Id);
                SocketTextChannel chnl = usr.Guild.GetTextChannel(glds.voiceUserActions);
                if (ot.VoiceChannel != null)
                {
                    if (glds.PrivateChannelID != 0)
                    {
                        var PrivateChannel = DBcontext.PrivateChannels.Count(x => x.ChannelId == ot.VoiceChannel.Id && x.UserId == user.Id);
                        if (PrivateChannel > 0) // Проверка приваток
                        {
                            await new Privates().PrivateDelete(usr, ot);
                        }
                    }
                    if (to.VoiceChannel == null)
                    {
                        emb.WithAuthor(" - Выход из Голосового чата", user.GetAvatarUrl())
                           .AddField($"Выход из", $"{ot.VoiceChannel.Name}", true)
                           .AddField($"Пользователь", $"{user.Mention}", true);
                        // Выход пользователя из голосового чата
                    }
                    else
                    {
                        if (ot.VoiceChannel != to.VoiceChannel)
                        {
                            emb.WithAuthor(" - Переход в другой Голосовой канал", user.GetAvatarUrl())
                               .AddField($"Переход из", $"{ot.VoiceChannel.Name}", true)
                               .AddField($"Переход в", $"{to.VoiceChannel.Name}", true);
                        }
                        else
                        {
                            emb.AddField($"В канале", $"{ot.VoiceChannel.Name}", true);
                            if (to.IsDeafened)
                                emb.WithAuthor(" - Администратор отключил звук", user.GetAvatarUrl());
                            else if (to.IsMuted)
                                emb.WithAuthor(" - Администратор отключил микрофон", user.GetAvatarUrl());
                            else if (to.IsSelfDeafened)
                                emb.WithAuthor(" - Пользователь отключил звук", user.GetAvatarUrl());
                            else if (to.IsSelfMuted)
                                emb.WithAuthor(" - Пользователь отключил микрофон", user.GetAvatarUrl());
                            else if (to.IsStreaming)
                                emb.WithAuthor(" - Пользователь запустил стрим", user.GetAvatarUrl());
                            else if (ot.IsDeafened)
                                emb.WithAuthor(" - Администратор включил звук", user.GetAvatarUrl());
                            else if (ot.IsMuted)
                                emb.WithAuthor(" - Администратор включил микрофон", user.GetAvatarUrl());
                            else if (ot.IsSelfDeafened)
                                emb.WithAuthor(" - Пользователь включил звук", user.GetAvatarUrl());
                            else if (ot.IsSelfMuted)
                                emb.WithAuthor(" - Пользователь включил микрофон", user.GetAvatarUrl());
                            else if (ot.IsStreaming)
                                emb.WithAuthor(" - Пользователь закончил стрим", user.GetAvatarUrl());
                        }
                        // Переход из одного чата в другой
                    }
                }

                if (to.VoiceChannel != null)
                {
                    if (glds.PrivateChannelID == to.VoiceChannel.Id)
                    {
                        var prv = DBcontext.PrivateChannels.Count(x => x.GuildId == glds.GuildId);
                        var OnlineCount = usr.Guild.Users.Count(x => x.Status == UserStatus.Online || x.Status == UserStatus.DoNotDisturb);
                        var OnlineDel = OnlineCount / 0.6;
                        if (OnlineDel >= prv)
                        {
                            if (!user.IsBot)
                                await new Privates().PrivateCreate(usr, to.VoiceChannel);
                        }// Проверка приваток
                    }

                    if (ot.VoiceChannel == null)
                    {
                        //await VoicePoint(user, to);
                        emb.WithAuthor(" - Вход в голосовой чат", user.GetAvatarUrl())
                           .AddField($"Вход в ", $"{to.VoiceChannel.Name}", true);
                        // Вход пользователя в голосовой чат
                    }
                }
                if (chnl != null)
                    await chnl.SendMessageAsync("", false, emb.Build());
            }
        }

        private async Task VoicePoint(SocketUser user, SocketVoiceState voice)
        {
            using (var DBcontext = new DBcontext())
            {
                var usrs = user as SocketGuildUser;
                //await Task.Run(() =>
                //{
                //    while (true)
                //    {
                //            Thread.Sleep(1500);
                //            Console.WriteLine(TaskTimer.client.GetGuild(usrs.Guild.Id).GetVoiceChannel(voice.VoiceChannel.Id).Users.Count);
                //    }
                //});
                //var usr = DBcontext.Users.FirstOrDefault(x => x.userid == user.Id && x.guildId == usrs.Guild.Id);
                //var dt = DateTime.Now.AddSeconds(30);
                //while (_discord.GetGuild(usrs.Guild.Id).GetVoiceChannel(voice.VoiceChannel.Id).Users.Count > 0)
                //{
                //    await Task.Delay(1000);
                //    Console.WriteLine(_discord.GetGuild(usrs.Guild.Id).VoiceChannels.Count);
                //    if (DateTime.Now >= dt)
                //    {
                //        usr.XP += 80;
                //        DBcontext.Users.Update(usr);
                //        await DBcontext.SaveChangesAsync();
                //        dt = DateTime.Now.AddSeconds(30);
                //    }
                //}
            }
        }

        private async Task Ready()
        {
            ScanningDataBase.GuildCheck(_discord, _provider);
        }

        private class sender
        {
            public ulong UserId { get; set; }
            public ulong GuildId { get; set; }
            public DateTime Time { get; set; }
        }

        private List<sender> UserFreezee = new List<sender>();

        private async Task MessageReceived(SocketMessage message)
        {
            var msg = message as SocketUserMessage;
            
            if (msg == null || msg.Author == null || msg.Author.IsBot || !ScanningDataBase.loading || (message.Author as SocketGuildUser).Guild == null)
                return;
            var Context = new SocketCommandContext(_discord, msg);

            var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
            var Channel = _cache.GetOrCreateChannelCache(Context.Channel.Id, Context.Guild.Id);
            bool ClearCache = false;

            if (msg.MentionedUsers.Count(x => x.IsBot) != 0 && 
                (!message.Content.Contains($"{GuildPrefix}es") || !message.Content.Contains($"{GuildPrefix}embedsay") &&
                 !message.Content.Contains($"{GuildPrefix}userclear") || !message.Content.Contains($"{GuildPrefix}uclr")))
                ClearCache = true;

            if (await OtherSettings.ChatSystem(Context, Channel, GuildPrefix))
                ClearCache = true;

            
            

            if (!ClearCache)
            {
                var RPcommand = _commands.Modules.FirstOrDefault(x => x.Name == "RPgif").Commands;
                int argPos = 0;
                if ((Channel.UseCommand || (Channel.UseRPcommand && RPcommand.FirstOrDefault(x => msg.Content.Contains(x.Name)) != null)) && msg.HasStringPrefix(GuildPrefix, ref argPos))
                {
                    UserFreezee.RemoveAll(x => (DateTime.Now - x.Time).TotalSeconds >= 1.5);
                    if (UserFreezee.FirstOrDefault(x => x.UserId == Context.User.Id && x.GuildId == Context.Guild.Id) != null)
                    {
                        _cache.Removes(Context);
                        return;
                    }
                    UserFreezee.Add(new sender() { UserId = Context.User.Id, GuildId = Context.Guild.Id, Time = DateTime.Now });
                    var result = await _commands.ExecuteAsync(Context, argPos, _provider);
                    if (!result.IsSuccess)
                        ClearCache = true;
                }
                else
                    ClearCache = true;
            }

            if (Channel.GiveXP)
                await OtherSettings.LVL(msg);

            if (ClearCache)
                _cache.Removes(Context);
        }

        private async Task LeftGuild(SocketGuild Guild)
        {
            using (var DBcontext = new DBcontext())
            {
                var glds = DBcontext.Guilds.FirstOrDefault(x => x.GuildId == Guild.Id);
                ScanningDataBase.GuildDelete(glds);
            }
        }

        private async Task JoinedGuild(SocketGuild Guild)
        {
            using (var DBcontext = new DBcontext())
            {
                var GuildPrefix = ScanningDataBase.GuildCreate(Guild).Result.Prefix;
                var emb = new EmbedBuilder().WithColor(255, 0, 94)
                                            .WithAuthor($"Информация о боте {Guild.CurrentUser.Username}🌏", Guild.CurrentUser.GetAvatarUrl())
                                            .WithDescription(string.Format(OtherSettings.WelcomeText, GuildPrefix))
                                            .WithImageUrl(BotSettings.bannerBoturl).Build();
                try
                {
                    await Guild.Owner.SendMessageAsync("", false, emb);
                }
                catch (Exception)
                {
                    await Guild.TextChannels.First().SendMessageAsync("", false, emb);
                }
            }
        }
    }
}
