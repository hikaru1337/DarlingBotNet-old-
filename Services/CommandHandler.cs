using DarlingBotNet.DataBase;
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
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DarlingBotNet.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        private IMemoryCache cache;

        public CommandHandler(DiscordSocketClient discord, CommandService commands, IServiceProvider provider, IMemoryCache memoryCache)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            cache = memoryCache;

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
        } // Подключение компонентов



        private async Task roledeleted(SocketRole role)
        {
            using (var DBcontext = new DBcontext())
            {
                var grte = DBcontext.EmoteClick.AsQueryable().Where(x => x.guildid == role.Guild.Id && x.roleid == role.Id);
                if (grte != null) DBcontext.EmoteClick.RemoveRange(grte);

                var LVLROLE = DBcontext.LVLROLES.FirstOrDefault(x => x.guildid == role.Guild.Id && x.roleid == role.Id);
                if (LVLROLE != null) DBcontext.LVLROLES.Remove(LVLROLE);

                if (grte != null || LVLROLE != null)
                    await DBcontext.SaveChangesAsync();

            }
        }

        private async Task OnChannelDestroyedAsync(SocketChannel channel) // Удаление каналов
        {
            if (channel as SocketTextChannel != null)
            {
                using (var DBcontext = new DBcontext())
                {
                    var chnl = DBcontext.Channels.FirstOrDefault(x => x.guildid == (channel as SocketTextChannel).Guild.Id && x.channelid == (channel as SocketTextChannel).Id);
                    if (chnl != null) DBcontext.Channels.Remove(chnl);

                    var emjmess = DBcontext.EmoteClick.FirstOrDefault(x => x.guildid == (channel as SocketTextChannel).Guild.Id && x.channelid == channel.Id);
                    if (emjmess != null) DBcontext.EmoteClick.RemoveRange(emjmess);

                    if (chnl != null || emjmess != null)
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
            if (emj.User.Value.IsBot) return;

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
              //else if (emj.Emote.Name == "◀️" || emj.Emote.Name == "▶️")
              //{
              //    var SlideClick = RussiaGame.slide.FirstOrDefault(x => x.messid == mess.Id && x.userid == emj.UserId);
              //    if (SlideClick != null)
              //    {
              //        if (emj.Emote.Name == "◀️")
              //            SlideClick.clicked = 2; // Back
              //        else
              //            SlideClick.clicked = 1; // Forward
              //    }
              //} // Slider
            if (mess.GetOrDownloadAsync().Result != null)
                await GetOrRemoveRole(mess, chnl, emj, false);
        }

        private async Task ReactRem(Cacheable<IUserMessage, ulong> mess, ISocketMessageChannel chnl, SocketReaction emj)
        {
            if (mess.GetOrDownloadAsync().Result != null)
                await GetOrRemoveRole(mess, chnl, emj, true);
        }

        private async Task GetOrRemoveRole(Cacheable<IUserMessage, ulong> mess, ISocketMessageChannel chnl, SocketReaction emj, bool getOrRemove)
        {
            using (var DBcontext = new DBcontext())
            {
                var mes = DBcontext.EmoteClick.AsQueryable().Where(x => x.guildid == (chnl as SocketGuildChannel).Guild.Id).AsEnumerable().FirstOrDefault(x => x.messageid == mess.Id && x.channelid == chnl.Id && emj.Emote.Name == Emote.Parse(x.emote).Name);
                if (mes != null)
                {
                    var usr = emj.User.Value as SocketGuildUser;
                    var role = usr.Guild.GetRole(mes.roleid);
                    if (!mes.get)
                    {
                        if (usr.Roles.FirstOrDefault(x => x == role) != null)
                            await usr.RemoveRoleAsync(role);
                        else
                            await usr.AddRoleAsync(role);
                    }
                    else
                    {
                        if (usr.Roles.Contains(role))
                            await usr.RemoveRoleAsync(role);

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
                    var glds = SystemLoading.GetOrCreateGuldsCache((chnl as SocketTextChannel).Guild.Id).GiveXPnextChannel;
                    DBcontext.Channels.Add(new Channels() { guildid = (chnl as SocketTextChannel).Guild.Id, channelid = chnl.Id, GiveXP = glds, UseCommand = true });
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
                var chnl = channelmes as SocketGuildChannel;
                var glds = SystemLoading.GetOrCreateGuldsCache(chnl.Guild.Id);
                if (chnl.Guild.GetTextChannel(glds.mesdelchannel) != null)
                {
                    var builder = new EmbedBuilder().WithColor(255, 0, 94).WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                .WithFooter($"id: {msg.Author.Id}", msg.Author.GetAvatarUrl())
                                .WithAuthor(" - изменение сообщения", msg.Author.GetAvatarUrl())
                                .AddField("Прошлое Сообщение", string.IsNullOrWhiteSpace(msg.Content) ? "-" : msg.Content)
                                .AddField("Новое Сообщение", string.IsNullOrWhiteSpace(after.Content) ? "-" : after.Content)
                                .AddField("Отправитель", string.IsNullOrWhiteSpace(after.Content) ? "-" : msg.Author.Mention, true)
                                .AddField("Канал", $"{(channelmes as SocketTextChannel).Mention}", true);
                    await (chnl.Guild.GetChannel(glds.mesdelchannel) as ISocketMessageChannel).SendMessageAsync("", false, builder.Build());
                }
            }

        } // Изменение сообщения Logging

        private async Task messsagedelete(Cacheable<IMessage, ulong> cachemess, ISocketMessageChannel channelmes)
        {
            using (var DBcontext = new DBcontext())
            {

                IMessage msg = await cachemess.GetOrDownloadAsync();
                if (msg == null || msg.Author.IsBot || msg.Content.Length > 1023) return;
                var chnl = channelmes as SocketGuildChannel;
                var glds = SystemLoading.GetOrCreateGuldsCache(chnl.Guild.Id);

                var emjmess = DBcontext.EmoteClick.FirstOrDefault(x => x.guildid == chnl.Guild.Id && x.channelid == channelmes.Id && x.messageid == msg.Id);
                if (emjmess != null) DBcontext.EmoteClick.Remove(emjmess);

                if (chnl.Guild.GetTextChannel(glds.mesdelchannel) != null)
                {
                    var builder = new EmbedBuilder().WithColor(255, 0, 94) // отсутствие каких либо данных
                                                .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                                .WithFooter($"id: {msg.Author.Id}", msg.Author.GetAvatarUrl())
                                                .WithAuthor(" - удаление сообщения", msg.Author.GetAvatarUrl())
                                                .AddField("Сообщение Удалено", string.IsNullOrWhiteSpace(msg.Content) ? "-" : msg.Content)
                                                .AddField("Отправитель", $"{msg.Author.Mention}", true)
                                                .AddField("Канал", $"{(channelmes as SocketTextChannel).Mention}", true);
                    await (chnl.Guild.GetChannel(glds.mesdelchannel) as ISocketMessageChannel).SendMessageAsync("", false, builder.Build());
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
                var glds = SystemLoading.GetOrCreateGuldsCache(guild.Id);
                if (guild.GetChannel(glds.unbanchannel) != null)
                {
                    var builder = new EmbedBuilder().WithColor(255, 0, 94)
                                                    .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                                    .WithFooter($"id: {user.Id}")
                                                    .WithAuthor(user)
                                                    .AddField($"Пользователь {(Ban ? "забанен" : "разбанен")}", user.Mention);
                    await (guild.GetChannel(glds.unbanchannel) as ISocketMessageChannel).SendMessageAsync("", false, builder.Build());
                }
            }
        }

        private async Task UserLeft(SocketGuildUser user)
        {
            using (var DBcontext = new DBcontext())
            {
                var glds = SystemLoading.GetOrCreateGuldsCache(user.Guild.Id);
                if (user.Guild.GetChannel(glds.leftchannel) != null)
                {
                    var builder = new EmbedBuilder().WithColor(255, 0, 94)
                                                    .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                                    .WithFooter(x => x.WithText($"id: {user.Id}"))
                                                    .WithAuthor($"Пользователь вышел: {user.Username}", user.GetAvatarUrl())
                                                    .WithDescription($"Имя: {user.Mention}\n Участников: {user.Guild.MemberCount}\nid: {user.Id}\nАккаунт создан: {user.CreatedAt.ToUniversalTime().ToString("dd.MM.yyyy HH:mm")}");
                    await (user.Guild.GetChannel(glds.leftchannel) as ISocketMessageChannel).SendMessageAsync("", false, builder.Build());
                }
                if (glds.LeaveChannel != 0)
                {
                    var mes = MessageBuilder.EmbedUserBuilder(glds.LeaveMessage);
                    mes.Item1.Description = mes.Item1.Description.Replace("%user%", user.Mention);
                    await (user.Guild.GetChannel(glds.LeaveChannel) as ISocketMessageChannel).SendMessageAsync(mes.Item2, false, mes.Item1.Build());
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
                var Guild = SystemLoading.GetOrCreateGuldsCache(user.Guild.Id);
                if (Guild.WelcomeRole != 0)
                    await user.AddRoleAsync(user.Guild.GetRole(Guild.WelcomeRole));

                if (Guild.RaidStop)
                {
                    if (Guild.RaidMuted > DateTime.Now)
                    {
                        await user.AddRoleAsync(user.Guild.GetRole(Guild.chatmuterole));
                        await user.AddRoleAsync(user.Guild.GetRole(Guild.voicemuterole));
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
                            Guild = await SystemLoading.CreateMuteRole(user.Guild);
                            Guild.RaidMuted = DateTime.Now.AddSeconds(30);
                            foreach (var userz in mes)
                            {
                                await (userz.User as SocketGuildUser).AddRoleAsync(user.Guild.GetRole(Guild.chatmuterole));
                                await (userz.User as SocketGuildUser).AddRoleAsync(user.Guild.GetRole(Guild.voicemuterole));
                            }
                        }
                    }
                }

                var usr = DBcontext.Users.FirstOrDefault(x => x.userid == user.Id && x.guildId == user.Guild.Id);
                if (usr != null)
                {
                    var role = DBcontext.LVLROLES.AsQueryable().Where(x => x.guildid == user.Guild.Id).AsEnumerable().Where(x => x.countlvl <= usr.Level).OrderBy(x => x.countlvl).LastOrDefault();
                    if (role != null && user.Guild.GetRole(role.roleid) != null)
                        await user.AddRoleAsync(user.Guild.GetRole(role.roleid));
                }

                if (user.Guild.GetChannel(Guild.joinchannel) != null)
                {
                    var builder = new EmbedBuilder().WithColor(255, 0, 94)
                                                    .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                                    .WithFooter(x => x.WithText($"id: {user.Id}"))
                                                    .WithAuthor($"Пользователь присоединился: {user.Username}", user.GetAvatarUrl())
                                                    .WithDescription($"Имя: {user.Mention}\nУчастников: {user.Guild.MemberCount}\nid: {user.Id}\nАккаунт создан: {user.CreatedAt.ToUniversalTime().ToString("dd.MM.yyyy HH:mm")}");
                    await (user.Guild.GetChannel(Guild.joinchannel) as ISocketMessageChannel).SendMessageAsync("", false, builder.Build());
                }
                if (Guild.WelcomeChannel != 0)
                {
                    var mes = MessageBuilder.EmbedUserBuilder(Guild.WelcomeMessage);
                    mes.Item1.Description = mes.Item1.Description.Replace("%user%", user.Mention);
                    await (user.Guild.GetChannel(Guild.WelcomeChannel) as ISocketMessageChannel).SendMessageAsync(mes.Item2, false, mes.Item1.Build());

                }
                if (Guild.WelcomeDMmessage != null && user.GetOrCreateDMChannelAsync().Result != null)
                {
                    var mes = MessageBuilder.EmbedUserBuilder(Guild.WelcomeDMmessage);
                    mes.Item1.Description = mes.Item1.Description.Replace("%user%", user.Mention);
                    await user.SendMessageAsync(mes.Item2, false, mes.Item1.Build());
                }
            }
        }  // Вход пользователя Logging

        private async Task UserVoiceUpdate(SocketUser user, SocketVoiceState ot, SocketVoiceState to)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                    .WithFooter(x => x.WithText($"id: {user.Id}")).AddField($"Пользователь", $"{user.Mention}", true);
                var glds = DBcontext.Guilds.FirstOrDefault(x => x.guildid == (user as SocketGuildUser).Guild.Id);

                if (ot.VoiceChannel != null)
                {
                    if (glds.PrivateChannelID != 0) // Проверка приваток
                    {
                        if (DBcontext.PrivateChannels.FirstOrDefault(x => x.userid == user.Id && x.guildid == ot.VoiceChannel.Guild.Id && x.channelid == ot.VoiceChannel.Id) != null) // Проверка приваток
                        {
                            await new Privates().PrivateDelete(user as SocketGuildUser, ot);
                        }
                    }
                    if (to.VoiceChannel == null)
                    {
                        if ((user as SocketGuildUser).Guild.GetTextChannel(glds.voiceUserActions) != null)
                        {
                            var builder = emb.WithAuthor(" - Выход из Голосового чата", user.GetAvatarUrl())
                                                   .AddField($"Выход из", $"{ot.VoiceChannel.Name}", true)
                                                   .AddField($"Пользователь", $"{user.Mention}", true);
                            await (user as SocketGuildUser).Guild.GetTextChannel(glds.voiceUserActions).SendMessageAsync("", false, builder.Build());
                        } // Выход пользователя из голосового чата
                    }
                    else
                    {
                        if ((user as SocketGuildUser).Guild.GetTextChannel(glds.voiceUserActions) != null)
                        {
                            var builder = emb.WithAuthor(" - Переход в другой Голосовой канал", user.GetAvatarUrl())
                                                            .AddField($"Переход из", $"{ot.VoiceChannel.Name}", true)
                                                            .AddField($"Переход в", $"{to.VoiceChannel.Name}", true);
                            await (user as SocketGuildUser).Guild.GetTextChannel(glds.voiceUserActions).SendMessageAsync("", false, builder.Build());
                        } // Переход из одного чата в другой
                    }
                }


                if (to.VoiceChannel != null)
                {
                    var prv = DBcontext.PrivateChannels.Count(x => x.guildid == glds.guildid);

                    if (glds.PrivateChannelID == to.VoiceChannel.Id)
                    {
                        if (((user as SocketGuildUser).Guild.Users.Where(x => x.Status == UserStatus.Online || x.Status == UserStatus.DoNotDisturb).Count() / 0.6) > prv)
                        {
                            //await new Privates().CheckPrivate(glds.guildId, (user as SocketGuildUser).Guild);
                            await new Privates().PrivateCreate(user as SocketGuildUser);
                        }// Проверка приваток
                    }

                    if (ot.VoiceChannel == null)
                    {
                        if ((user as SocketGuildUser).Guild.GetTextChannel(glds.voiceUserActions) != null)
                        {
                            var builder = emb.WithAuthor(" - Вход в голосовой чат", user.GetAvatarUrl()).AddField($"Вход в ", $"{to.VoiceChannel.Name}", true);
                            await (user as SocketGuildUser).Guild.GetTextChannel(glds.voiceUserActions).SendMessageAsync("", false, builder.Build());

                        } // Вход пользователя в голосовой чат
                          //await VoicePoint(user, to);
                    }
                }
            }
        }

        private async Task VoicePoint(SocketUser user, SocketVoiceState voice)
        {
            using (var DBcontext = new DBcontext())
            {
                var usr = DBcontext.Users.FirstOrDefault(x => x.userid == user.Id && x.guildId == (user as SocketGuildUser).Guild.Id);
                await DBcontext.SaveChangesAsync();
                var dt = DateTime.Now.AddSeconds(30);
                while (voice.VoiceChannel.Users.Contains(user))
                {
                    if (dt == DateTime.Now)
                    {
                        usr.XP += 80;
                        DBcontext.Users.Update(usr);
                        await DBcontext.SaveChangesAsync();
                        dt = DateTime.Now.AddSeconds(30);
                    }
                }
            }
        }

        private async Task Ready()
        {
            await SystemLoading.GuildCheck(_discord, _provider);
        }

        private async Task MessageReceived(SocketMessage message)
        {
            var msg = message as SocketUserMessage;
            if (msg == null || msg.Author.IsBot || msg.MentionedUsers.FirstOrDefault(x => x.IsBot) != null || SystemLoading.loading == false) return;
            var Context = new SocketCommandContext(_discord, msg);

            var User = SystemLoading.GetOrCreateUserCache(Context.User.Id,Context.Guild.Id);
            var Guild = SystemLoading.GetOrCreateGuldsCache(Context.Guild.Id);
            var Channel = SystemLoading.GetOrCreateChannelCache(Context.Channel.Id, Context.Guild.Id);
            if (await SystemLoading.ChatSystem(Context, Channel, Guild.Prefix)) return;

            int argPos = 0;
            if ((Channel.UseCommand || (Channel.UseRPcommand && _commands.Modules.FirstOrDefault(x => x.Name == "RPgif").Commands.FirstOrDefault(x => msg.Content.Contains(x.Name)) != null)) && msg.HasStringPrefix(Guild.Prefix, ref argPos))
            {
                var result = await _commands.ExecuteAsync(Context, argPos, _provider);
                if (!result.IsSuccess)
                {
                    var emb = SystemLoading.GetError(result.ErrorReason, _discord).Result;
                    await msg.Channel.SendMessageAsync("", false, emb.Build());
                }
                else
                {
                    if ((Guilds)cache.Get(Guild.guildid) == null)
                        cache.Set(Guild.guildid, Guild, new MemoryCacheEntryOptions()
                        { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) });

                    if ((Users)cache.Get((User.userid, Guild.guildid)) == null)
                        cache.Set((User.userid, Guild.guildid), User, new MemoryCacheEntryOptions()
                        { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) });

                    if ((Channels)cache.Get((Channel.channelid, Guild.guildid)) == null)
                        cache.Set((Channel.channelid, Guild.guildid), Channel, new MemoryCacheEntryOptions()
                        { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) });
                }
            }
            if (Channel.GiveXP) await SystemLoading.LVL(msg);
        }

        private async Task LeftGuild(SocketGuild Guild)
        {
            using (var DBcontext = new DBcontext())
            {
                var glds = DBcontext.Guilds.FirstOrDefault(x => x.guildid == Guild.Id);
                await SystemLoading.GuildDelete(glds);
            }
        }

        private async Task JoinedGuild(SocketGuild Guild)
        {
            using (var DBcontext = new DBcontext())
            {
                var guild = SystemLoading.GuildCreate(Guild).Result;
                var emb = new EmbedBuilder().WithColor(255, 0, 94)
                                            .WithAuthor($"Информация о боте {Guild.CurrentUser.Username}🌏", Guild.CurrentUser.GetAvatarUrl())
                                            .WithDescription(string.Format(SystemLoading.WelcomeText, guild.Prefix))
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
