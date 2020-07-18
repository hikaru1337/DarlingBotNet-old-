using DarlingBotNet.DataBase;
using DarlingBotNet.Modules;
using DarlingBotNet.Services.Sys;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace DarlingBotNet.Services
{
    class CommandHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        private readonly DiscordShardedClient _shard;

        public CommandHandler(DiscordSocketClient discord, CommandService commands, IServiceProvider provider, DiscordShardedClient shard)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _shard = shard;

            _discord.JoinedGuild += JoinedGuild;
            _discord.LeftGuild += LeftGuild;
            _discord.MessageReceived += MessageReceived;
            _discord.MessageDeleted += messsagedelete;
            _discord.MessageUpdated += messageupdate;
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

        private async Task OnChannelDestroyedAsync(SocketChannel channel) // Удаление каналов
        {
            await Task.Delay(1);
            if (new EEF<Channels>(new DBcontext()).GetF(x => x.channelid == channel.Id) != null && (channel as SocketTextChannel) != null)
                new EEF<Channels>(new DBcontext()).Remove(new EEF<Channels>(new DBcontext()).GetF(x => x.guildid == (channel as SocketGuildChannel).Guild.Id && x.channelid == channel.Id));

            var emjmess = new EEF<EmoteClick>(new DBcontext()).GetF(x => x.guildid == (channel as SocketGuildChannel).Guild.Id && x.channelid == channel.Id);
            if (emjmess != null) new EEF<EmoteClick>(new DBcontext()).Remove(emjmess);
        }

        private async Task ReactAdd(Cacheable<IUserMessage, ulong> mess, ISocketMessageChannel chnl, SocketReaction emj) // Проверка на эмодзи
        {
            await Task.Delay(1);
            if (emj.Emote.Name == "✅")
            {
                if (User.list.FirstOrDefault(x => x.messid == mess.Id && x.marryedid == emj.UserId) != null)
                    User.list.FirstOrDefault(x => x.messid == mess.Id && x.marryedid == emj.UserId).tar = 2;
            } // MarryedAccept
            if (emj.Emote.Name == "❎")
            {
                if (User.list.FirstOrDefault(x => x.messid == mess.Id && x.marryedid == emj.UserId) != null)
                    User.list.FirstOrDefault(x => x.messid == mess.Id && x.marryedid == emj.UserId).tar = 1;
            } // MarryedDenied

            var mes = new EEF<EmoteClick>(new DBcontext()).GetF(x => x.guildid == (chnl as SocketGuildChannel).Guild.Id && x.messageid == mess.Id && emj.Emote.Name == Emote.Parse(x.emote).Name);
            if (mes != null)
            {
                if (mess.GetOrDownloadAsync().Result != null)
                {
                    var usr = _discord.GetGuild((chnl as SocketGuildChannel).Guild.Id).GetUser(emj.User.Value.Id) as SocketGuildUser;
                    if (usr.Roles.Count(x => x.Id == mes.roleid) == 0 && mes.get == false)
                        await usr.AddRoleAsync(_discord.GetGuild(usr.Guild.Id).GetRole(mes.roleid));

                    else if (usr.Roles.Count(x => x.Id == mes.roleid) > 0 && mes.get)
                        await usr.RemoveRoleAsync(_discord.GetGuild(usr.Guild.Id).GetRole(mes.roleid));
                }
            }
        }

        private async Task ReactRem(Cacheable<IUserMessage, ulong> mess, ISocketMessageChannel chnl, SocketReaction emj)
        {
            var mes = new EEF<EmoteClick>(new DBcontext()).GetF(x => x.guildid == (chnl as SocketGuildChannel).Guild.Id && x.messageid == mess.Id && emj.Emote.Name == Emote.Parse(x.emote).Name);
            if (mes != null)
            {
                if (mess.GetOrDownloadAsync().Result != null)
                {
                    var usr = _discord.GetGuild((chnl as SocketGuildChannel).Guild.Id).GetUser(emj.User.Value.Id) as SocketGuildUser;
                    if (usr.Roles.Count(x => x.Id == mes.roleid) > 0 && mes.get == false)
                        await usr.RemoveRoleAsync(_discord.GetGuild(usr.Guild.Id).GetRole(mes.roleid));
                }
            }
        }

        private async Task OnChannelCreateAsync(SocketChannel chnl) // Создание каналов
        {
            await SystemLoading.ChannelCreate(chnl as SocketGuildChannel);
        }

        private async Task messageupdate(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channelmes)
        {
            IMessage msg = await before.GetOrDownloadAsync();
            if (msg == null || msg.Author.IsBot) return;
            var user = msg.Author as SocketGuildUser;
            var glds = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == user.Guild.Id);
            if (user != null && user.Guild.GetTextChannel(glds.mesdelchannel) != null)
            {
                var builder = new EmbedBuilder().WithColor(255, 0, 94).WithTimestamp(DateTimeOffset.Now.ToUniversalTime()).WithFooter(footer => { footer.WithText($"id: {msg.Author.Id}").WithIconUrl(msg.Author.GetAvatarUrl()); })
                            .WithAuthor(" - изменение сообщения", msg.Author.GetAvatarUrl())
                            .AddField("Прошлое Сообщение", $"{msg.Content}")
                            .AddField("Новое Сообщение", $"{after}")
                            .AddField("Отправитель", $"{msg.Author.Mention}", true)
                            .AddField("Канал", $"{(channelmes as SocketTextChannel).Mention}", true);
                await (user.Guild.GetChannel(glds.mesdelchannel) as ISocketMessageChannel).SendMessageAsync("", false, builder.Build());
            }

        } // Изменение сообщения Logging

        private async Task messsagedelete(Cacheable<IMessage, ulong> cachemess, ISocketMessageChannel channelmes)
        {
            IMessage msg = await cachemess.GetOrDownloadAsync();
            if (msg == null || msg.Author.IsBot) return;
            var user = msg.Author as SocketGuildUser;
            var glds = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == user.Guild.Id);
            var emjmess = new EEF<EmoteClick>(new DBcontext()).GetF(x => x.guildid == user.Guild.Id && x.channelid == channelmes.Id && x.messageid == msg.Id);
            if (emjmess != null) new EEF<EmoteClick>(new DBcontext()).Remove(emjmess);
            if (user != null && user.Guild.GetTextChannel(glds.mesdelchannel) != null)
            {
                var builder = new EmbedBuilder().WithColor(255, 0, 94) // отсутствие каких либо данных
                                            .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                            .WithFooter($"id: {msg.Author.Id}", msg.Author.GetAvatarUrl())
                                            .WithAuthor(" - удаление сообщения", msg.Author.GetAvatarUrl())
                                            .AddField("Сообщение Удалено", $"{msg.Content}")
                                            .AddField("Отправитель", $"{msg.Author.Mention}", true)
                                            .AddField("Канал", $"{(channelmes as SocketTextChannel).Mention}", true);
                await (user.Guild.GetChannel(glds.mesdelchannel) as ISocketMessageChannel).SendMessageAsync("", false, builder.Build());
            }

        }  // Удаление сообщения Logging

        private async Task UserUnBan(SocketUser user, SocketGuild guild)
        {
            var glds = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == guild.Id);
            if (guild.GetChannel(glds.unbanchannel) != null)
            {
                var builder = new EmbedBuilder().WithColor(255, 0, 94)
                                                .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                                .WithFooter($"id: {user.Id}")
                                                .WithAuthor(user.Mention)
                                                .AddField("Пользователь Разбанен", user.Mention);
                await (guild.GetChannel(glds.unbanchannel) as ISocketMessageChannel).SendMessageAsync("", false, builder.Build());
            }

        }  // Разбан пользователя Logging

        private async Task UserBan(SocketUser user, SocketGuild guild)
        {
            var glds = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == guild.Id);
            if (guild.GetChannel(glds.banchannel) != null)
            {
                var builder = new EmbedBuilder().WithColor(255, 0, 94)
                                                .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                                .WithFooter($"id: {user.Id}")
                                                .WithAuthor(user.Mention)
                                                .AddField("Пользователь Забанен", user.Mention);
                await (guild.GetChannel(glds.banchannel) as ISocketMessageChannel).SendMessageAsync("", false, builder.Build());
            }

        } // Бан пользователя Logging

        private async Task UserLeft(SocketGuildUser user)
        {
            var glds = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == user.Guild.Id);
            if (user.Guild.GetChannel(glds.leftchannel) != null)
            {
                var builder = new EmbedBuilder().WithColor(255, 0, 94)
                                                .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                                .WithFooter(x => x.WithText($"id: {user.Id}"))
                                                .WithAuthor($"Пользователь вышел: {user.Username}", user.GetAvatarUrl())
                                                .WithDescription($"Имя: {user.Mention}\n Участников: {user.Guild.MemberCount}\nid: {user.Id}");
                await (user.Guild.GetChannel(glds.leftchannel) as ISocketMessageChannel).SendMessageAsync("", false, builder.Build());
            }
            if (glds.LeaveChannel != 0)
            {
                var mes = MessageBuilder.EmbedUserBuilder(glds.LeaveMessage);
                await (user.Guild.GetChannel(glds.LeaveChannel) as ISocketMessageChannel).SendMessageAsync(mes.Title.Split("||")[1], false, mes.Build());
            }

        }  // Выход пользователя Logging

        private async Task UserJoined(SocketGuildUser user)
        {
            await SystemLoading.UserJoinCheck(user);
            var glds = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == user.Guild.Id);

            if (user.Guild.GetChannel(glds.joinchannel) != null)
            {
                var builder = new EmbedBuilder().WithColor(255, 0, 94)
                                                .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                                .WithFooter(x => x.WithText($"id: {user.Id}"))
                                                .WithAuthor($"Пользователь присоединился: {user.Username}", user.GetAvatarUrl())
                                                .WithDescription($"Имя: {user.Mention}\nУчастников: {user.Guild.MemberCount}\nid: {user.Id}");
                await (user.Guild.GetChannel(glds.joinchannel) as ISocketMessageChannel).SendMessageAsync("", false, builder.Build());
            }
            if (glds.WelcomeChannel != 0)
            {
                var mes = MessageBuilder.EmbedUserBuilder(glds.WelcomeMessage);
                await (user.Guild.GetChannel(glds.WelcomeChannel) as ISocketMessageChannel).SendMessageAsync(mes.Title.Split("||")[1], false, mes.Build());

            }
            if (glds.WelcomeRole != 0)
                await user.AddRoleAsync(user.Guild.GetRole(glds.WelcomeRole));
            if (glds.WelcomeDMmessage != null && user.GetOrCreateDMChannelAsync().Result != null)
            {
                var mes = MessageBuilder.EmbedUserBuilder(glds.WelcomeDMmessage);
                await user.SendMessageAsync(mes.Title.Split("||")[1], false, mes.Build());
            }

        }  // Вход пользователя Logging

        private async Task UserVoiceUpdate(SocketUser user, SocketVoiceState ot, SocketVoiceState to)
        {
            Guilds glds = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == (user as SocketGuildUser).Guild.Id);

            if (ot.VoiceChannel != null)
            {
                if (glds.PrivateChannelID != 0) // Проверка приваток
                {
                    if (new EEF<PrivateChannels>(new DBcontext()).GetF(x => x.userid == user.Id && x.channelid == ot.VoiceChannel.Id) != null) // Проверка приваток
                        await Privates.PrivateDelete(user as SocketGuildUser, ot);
                }
                if (to.VoiceChannel == null)
                {
                    if ((user as SocketGuildUser).Guild.GetTextChannel(glds.voiceUserActions) != null)
                    {
                        var builder = new EmbedBuilder()
                                               .WithColor(255, 0, 94)
                                               .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                               .WithFooter(x => x.WithText($"id: {user.Id}"))
                                               .WithAuthor(" - Выход из Голосового чата", user.GetAvatarUrl())
                                               .AddField($"Выход из", $"{ot.VoiceChannel.Name}", true)
                                               .AddField($"Пользователь", $"{user.Mention}", true);
                        await (user as SocketGuildUser).Guild.GetTextChannel(glds.voiceUserActions).SendMessageAsync("", false, builder.Build());
                    } // Выход пользователя из голосового чата
                }
            }

            if (ot.VoiceChannel != null && to.VoiceChannel != null)
            {
                if ((user as SocketGuildUser).Guild.GetTextChannel(glds.voiceUserActions) != null && ot.VoiceChannel != to.VoiceChannel)
                {
                    var builder = new EmbedBuilder().WithColor(255, 0, 94)
                                                    .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                                    .WithFooter(x => x.WithText($"id: {user.Id}"))
                                                    .WithAuthor(" - Переход в другой Голосовой канал", user.GetAvatarUrl())
                                                    .AddField($"Переход из", $"{ot.VoiceChannel.Name}", true)
                                                    .AddField($"Переход в", $"{to.VoiceChannel.Name}", true)
                                                    .AddField($"Пользователь", $"{user.Mention}", true);
                    await (user as SocketGuildUser).Guild.GetTextChannel(glds.voiceUserActions).SendMessageAsync("", false, builder.Build());
                } // Переход из одного чата в другой
            }

            if (to.VoiceChannel != null)
            {
                if (new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == (user as SocketGuildUser).Guild.Id).PrivateChannelID == to.VoiceChannel.Id)
                {
                    if (glds.PrivateChannelID != 0) // Проверка приваток
                    {
                        await Privates.CheckPrivate(glds.guildId, (user as SocketGuildUser).Guild);
                        await Privates.PrivateCreate(user as SocketGuildUser);
                    }
                }// Проверка приваток

                if (ot.VoiceChannel == null)
                {
                    if ((user as SocketGuildUser).Guild.GetTextChannel(glds.voiceUserActions) != null)
                    {
                        var builder = new EmbedBuilder().WithColor(255, 0, 94)
                                                        .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                                        .WithFooter(x => x.WithText($"id: {user.Id}"))
                                                        .WithAuthor(" - Вход в голосовой чат", user.GetAvatarUrl())
                                                        .AddField($"Вход в ", $"{to.VoiceChannel.Name}", true)
                                                        .AddField($"Пользователь", $"{user.Mention}", true);
                        await (user as SocketGuildUser).Guild.GetTextChannel(glds.voiceUserActions).SendMessageAsync("", false, builder.Build());
                    } // Вход пользователя в голосовой чат
                }
            }
        }



        private async Task Ready()
        {
            await SystemLoading.GuildCheck(_discord);
        }

        private async Task MessageReceived(SocketMessage message)
        {
            var msg = message as SocketUserMessage;
            if (msg == null || msg.Author.IsBot || SystemLoading.loading == false) return;
            var Context = new SocketCommandContext(_discord, msg);
            if (await SystemLoading.ChatSystem(Context)) return;

            var Guild = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == (msg.Author as SocketGuildUser).Guild.Id);
            var Channel = await SystemLoading.ChannelCreate(Context.Channel as SocketGuildChannel);
            int argPos = 0;
            if (Channel.UseCommand && msg.HasStringPrefix(Guild.Prefix, ref argPos))
            {
                var result = await _commands.ExecuteAsync(Context, argPos, _provider);
                if (!result.IsSuccess)
                {
                    var emb = SystemLoading.GetError(result.ErrorReason, _discord).Result;
                    await msg.Channel.SendMessageAsync("", false, emb.Build());
                }
            }
            if(Channel.GiveXP) await SystemLoading.LVL(msg);
        }

        private async Task LeftGuild(SocketGuild Guild)
        {
            Guilds guild = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == Guild.Id);
            await SystemLoading.GuildDelete(guild);
        }

        private async Task JoinedGuild(SocketGuild Guild)
        {
            Guilds guild = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == Guild.Id);

            if (guild != null)
            {
                guild.Leaved = false;
                new EEF<Guilds>(new DBcontext()).Update(guild);
                await SystemLoading.ChannelCreateRange(Guild.TextChannels);
            }
            else await SystemLoading.GuildCreate(Guild);
        }
    }
}
