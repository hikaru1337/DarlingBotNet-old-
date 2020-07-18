using DarlingBotNet.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DarlingBotNet.Modules
{
    [Name("Bot")]
    public class Bot : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _discord;

        public Bot(DiscordSocketClient discord, CommandService commands, IServiceProvider provider)
        {
            _discord = discord;
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task bug([Remainder] string error)
        {
            var channel = Context.Client.GetChannel(BotSettings.darlingbug) as ISocketMessageChannel;
            if (channel != null)
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder().WithDescription($"Текст:`{error}`")
                                                                                    .WithColor(255, 0, 94)
                                                                                    .WithAuthor("📛bug - Спасибо за отправку отчета.")
                                                                                    .Build());

                var emb = new EmbedBuilder().WithAuthor($"📛bug", Context.Guild.IconUrl).WithDescription($"{error}")
                                            .AddField("Отправитель: ", Context.User.Id, true).AddField("Сервер", Context.Guild.Id)
                                            .WithFooter("Время отправки: " + DateTimeOffset.Now.ToUniversalTime()).WithColor(255, 0, 94);
                await channel.SendMessageAsync("", false, emb.Build());
            }
            else
                await (Context.Client.GetChannel(BotSettings.SystemMessage) as ISocketMessageChannel)
                    .SendMessageAsync($"{Context.Channel.GetUserAsync(BotSettings.hikaruid).Result.Mention} канал для багов не действительный");
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [RequireOwner]
        public async Task serverinfo(ulong serverid = 0)
        {
            if (serverid == 0) serverid = Context.Guild.Id;
            var emb = new EmbedBuilder().WithColor(255, 0, 94);
            var gldss = _discord.GetGuild(serverid);
            if (gldss == null) emb.WithAuthor($" serverinfo {serverid}").WithDescription("Сервер не найден!");
            else emb.WithAuthor($" - serverinfo {gldss.Name}", gldss.IconUrl)
                    .AddField("Информация о участниках", $"Members: {gldss.MemberCount}\n" +
                                                     $"Online: {gldss.Users.Where(x => x.Status == UserStatus.Online).Count()}\n" +
                                                     $"Offline: {gldss.Users.Where(x => x.Status == UserStatus.Offline).Count()}\n" +
                                                     $"Afk: {gldss.Users.Where(x => x.Status == UserStatus.AFK).Count()}\n" +
                                                     $"Invisible: {gldss.Users.Where(x => x.Status == UserStatus.Invisible).Count()}\n", true)
                    .AddField("Информация о сервере", $"VoiceChannels: {gldss.VoiceChannels.Count}\n" +
                                                      $"TextChannels: {gldss.TextChannels.Count}\n" +
                                                      $"Roles: {gldss.Roles.Count}\n", true)
                    .AddField("Информация об Админе", $"ping: {gldss.Owner.Mention}\n" +
                                                      $"id: {gldss.OwnerId}\n" +
                                                      $"Аккаунт создан: {gldss.Owner.CreatedAt.UtcDateTime}\n", true);
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [RequireOwner]
        public async Task getinvite([Remainder]string servername)
        {
            var serv = Context.Client.Guilds.Where(x=>x.Name == servername).First();
            var emb = new EmbedBuilder().WithColor(255, 0, 94)
                                        .WithAuthor($"Invite {serv.Name}");
            if (serv == null) 
                emb.WithDescription("Сервер не найден!");
            else
            {
                if(serv.GetInvitesAsync().Result == null)
                    emb.WithDescription("Инвайтов нет!");
                else
                    emb.WithDescription(serv.GetInvitesAsync().Result.First().Url);
            }                       
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }
    }
}
