using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DarlingBotNet.Modules
{
    [Name("Bot")]
    public class Bot : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _discord;
        private readonly IMemoryCache _cache;
        

        public Bot(DiscordSocketClient discord, IMemoryCache cache)
        {
            _discord = discord;
            _cache = cache;
        }

        private class UserPays
        {
            public ulong UserId { get; set; }
            public float Sum { get; set; }
            public DateTime DatePay { get; set; }
        }

        //[Aliases, Commands, Usage, Descriptions]
        //[PermissionBlockCommand]
        //public async Task boost()
        //{
        //    var emb = new EmbedBuilder().WithColor(255, 0, 94);
        //    string buynow = $"Вы можете купить его всего за 150 рублей прямо сейчас!\n\nКупить: [Клик]({String.Format(BotSettings.PayUserURL, Context.User.Id)})";
        //    var pays = BotSettings.PayURL.GetJsonFromUrl();
        //    if (pays.Length > 2)
        //    {
        //        var payz = pays.FromJson<UserPays>().InList().Where(x => x.UserId == Context.User.Id).ToList();
        //        var lastpay = payz.Max(x => x.DatePay);
        //        if (lastpay > DateTime.Now)
        //            emb.WithDescription($"Ваш буст оплачен до {lastpay.ToString("dd:MM:yy hh:mm:ss")}\n" +
        //                $"Благодаря тебе я до сих пор работаю ❤️\n\n" +
        //                $"Ты можешь продлить буст: [Клик]({String.Format(BotSettings.PayUserURL, Context.User.Id)})").WithAuthor("Boost 🟢");
        //        else
        //            emb.WithDescription($"Ваш буст закончился {((DateTime.Now - lastpay).Days == 0 ? $"{(DateTime.Now - lastpay).Hours} часов" : $"{(DateTime.Now - lastpay).Days} дней")} назад!" +
        //                $"\n{buynow}\n\n").WithAuthor("Boost 🟡");

        //        emb.WithDescription("Ваши последние транзакции:\n");
        //        foreach (var Pays in payz.OrderBy(x => x.DatePay).ThenByDescending(x => x.DatePay).Take(5))
        //        {
        //            emb.Description += $"{Pays.Sum} - {Pays.DatePay}";
        //        }
        //    }
        //    else emb.WithDescription($"Вы еще не купили ни одно Boost!\nЧто дает Boost, вы можете прочитать тут:[КЛИК]()\n\n{buynow}").WithAuthor("Boost 🔴");
           
        //    try
        //    {
        //        await Context.User.SendMessageAsync("", false, emb.Build());
        //    }
        //    catch (Exception)
        //    {
        //        emb.WithDescription("Для того чтобы посмотреть информацию о Boost включите отправку сообщений!").WithImageUrl(BotSettings.EnableDMmessageURL);
        //        await Context.Channel.SendMessageAsync("", false, emb.Build());
        //        return;
        //    }


        //}

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
        public async Task pre([Remainder] string predlojenie)
        {
            var channel = Context.Client.GetChannel(BotSettings.darlingpre) as ISocketMessageChannel;
            if (channel != null)
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder().WithDescription($"Текст:`{predlojenie}`")
                                                                                    .WithColor(255, 0, 94)
                                                                                    .WithAuthor("📛Pre - Спасибо за отправку отчета.")
                                                                                    .Build());

                var emb = new EmbedBuilder().WithAuthor($"📛Pre", Context.Guild.IconUrl).WithDescription($"{predlojenie}")
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
        public async Task sendusermessage(ulong userid, string message)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("SendUserMessage");
            var usr = Context.Client.GetUser(userid).GetOrCreateDMChannelAsync();
            if (usr == null)
                emb.WithDescription("У пользователя закрыта личка.");
            else
            {
                emb.WithDescription("Сообщение успешно отправлено пользователю.");
                await usr.Result.SendMessageAsync(message);
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [RequireOwner]
        public async Task getinvite([Remainder] string servername)
        {
            var serv = Context.Client.Guilds.Where(x => x.Name == servername).First();
            var emb = new EmbedBuilder().WithColor(255, 0, 94)
                                        .WithAuthor($"Invite {serv.Name}");
            if (serv == null)
                emb.WithDescription("Сервер не найден!");
            else
            {
                if (serv.GetInvitesAsync().Result == null)
                    emb.WithDescription("Инвайтов нет!");
                else
                    emb.WithDescription(serv.GetInvitesAsync().Result.First().Url);
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [RequireOwner]
        public async Task setlevel(SocketGuildUser user, uint level)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"SetLevel {user}");
            using (var DBcontext = new DBcontext())
            {
                var usr = _cache.GetOrCreateUserCache(user.Id, user.Guild.Id);
                //var usr = DBcontext.Users.GetOrCreate((user as SocketGuildUser)).Result;
                emb.WithDescription($"Уровень с {usr.Level} выставлен на {level}");
                usr.XP = level * (80 * level);
                _cache.Update(usr);
                DBcontext.Users.Update(usr);
                await DBcontext.SaveChangesAsync();
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
            
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [RequireOwner]
        public async Task setcoin(SocketGuildUser user, uint Coin)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"SetCoin {user}");
            using (var DBcontext = new DBcontext())
            {
                var usr = _cache.GetOrCreateUserCache(user.Id, user.Guild.Id);
                emb.WithDescription($"Zercoin's выставлен с {usr.ZeroCoin} на {Coin}");
                usr.ZeroCoin = Coin;
                _cache.Update(usr);
                DBcontext.Users.Update(usr);
                await DBcontext.SaveChangesAsync();
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
            
        }
    }
}
