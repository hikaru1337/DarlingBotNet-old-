using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.DataBase.Database.Models;
using DarlingBotNet.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
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

        public async Task RefreshPayments()
        {
            using (var DBcontext = new DBcontext())
            {
                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        QiwiTransactions[] json = JsonConvert.DeserializeObject<QiwiTransactions[]>(wc.DownloadString(BotSettings.PayURL));
                        foreach (QiwiTransactions token in json)
                        {

                            var payments = DBcontext.QiwiTransaction.AsQueryable().FirstOrDefault(x => x.discord_id == token.discord_id && x.invoice_date_add == token.invoice_date_add && x.invoice_ammount == token.invoice_ammount);
                            if (payments == null)
                            {
                                DBcontext.QiwiTransaction.Add(new QiwiTransactions()
                                {
                                    discord_id = payments.discord_id,
                                    invoice_date_add = payments.invoice_date_add,
                                    invoice_ammount = payments.invoice_ammount
                                });
                                var user = DBcontext.Users.FirstOrDefault(x => x.UserId == payments.discord_id);
                                user.RealCoin += payments.invoice_ammount;
                                await DBcontext.SaveChangesAsync();
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task boost(string buy = "nbuy")
        { 
            using (var DBcontext = new DBcontext())
            {
                await RefreshPayments();
                var emb = new EmbedBuilder().WithColor(255, 0, 94);
                var user = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                var UserBoost = DBcontext.DarlingBoost.FirstOrDefault(x=>x.UserId == Context.User.Id);
                if (UserBoost == null)
                {
                    UserBoost = new DarlingBoost() { UserId = Context.User.Id, Streak = 0, Ends = DateTime.Now.AddDays(-1) };
                    DBcontext.DarlingBoost.Add(UserBoost);
                    await DBcontext.SaveChangesAsync();
                }
                ulong price = 150;
                if (UserBoost.Streak > 0)
                    price -= ((price / 100) * 2 * UserBoost.Streak) / UserBoost.Streak;
                string buynow = $"[Пополнить баланс]({String.Format(BotSettings.PayUserURL, Context.User.Id,price)})";
                if (buy.ToLower() == "buy")
                {
                    
                    if (user.RealCoin > price)
                    {
                        emb.WithTitle($"DarlingBoost {BotSettings.EmoteBoost}");

                        if (UserBoost.Ends > DateTime.Now)
                        {
                            emb.WithDescription("Вы продлили DarlingBoost на 1 месяц вперед. Спасибо ❤️");
                            emb.Description += $"\nЗа продление вы получили скидку {150 - price} RealCoin.";
                            UserBoost.Streak++;
                        }   
                        else
                        {
                            UserBoost.Streak = 1;
                            emb.WithDescription("Вы купили DarlingBoost на 1 месяц. Спасибо ❤️");
                        }

                        UserBoost.Ends = UserBoost.Ends.AddMonths(1);
                        
                        user.RealCoin -= price;
                        DBcontext.Users.Update(user);
                        DBcontext.DarlingBoost.Update(UserBoost);
                        await DBcontext.SaveChangesAsync();
                    }
                    else
                        emb.WithTitle($"DarlingBoost").WithDescription($"На вашем счете недостаточно средств для покупки DarlingBoost\nБаланс: {user.RealCoin} - {buynow}");
                }
                else
                {
                    var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                    emb.WithDescription($"RealCoin: {user.RealCoin} - {buynow}\n");
                    if (UserBoost.Streak > 0 && UserBoost.Ends.Year == 1)
                    {
                        if (UserBoost.Ends > DateTime.Now)
                        {
                            emb.Description += $"Буст оплачен до {UserBoost.Ends.ToString("dd:MM:yy hh:mm")}\n\n" +
                                               $"Благодаря тебе я до сих пор работаю ❤️\n" +
                                               $"Ты можешь продлить буст: {GuildPrefix}boost buy";
                            emb.WithTitle($"DarlingBoost {BotSettings.EmoteBoost}");
                        }
                        else
                        {
                            if ((DateTime.Now - UserBoost.Ends).TotalDays < 7)
                                emb.WithTitle($"DarlingBoost {BotSettings.EmoteBoostNo}");

                            else if ((DateTime.Now - UserBoost.Ends).TotalDays == 7)
                                emb.WithTitle($"DarlingBoost {BotSettings.EmoteBoostLastDay}");
                            else
                            {
                                emb.WithTitle($"DarlingBoost {BotSettings.EmoteBoostNot}");
                                UserBoost.Streak = 0;
                                DBcontext.DarlingBoost.Update(UserBoost);
                                await DBcontext.SaveChangesAsync();
                            }

                            emb.Description += $"Ваш буст закончился {((DateTime.Now - UserBoost.Ends).Days == 0 ? $"{(DateTime.Now - UserBoost.Ends).TotalHours} часов" : $"{(DateTime.Now - UserBoost.Ends).TotalDays} дней")} назад!\n" +
                                $"Ты можешь продлить буст: {GuildPrefix}boost buy";
                        }
                    }
                    else
                    {
                        emb.WithTitle($"DarlingBoost {BotSettings.EmoteBoostNot}").Description += $"Вы еще не купили ни одно Boost!\nЧто дает Boost, вы можете прочитать тут: [КЛИК](https://docs.darlingbot.ru/commands/darling-boost)";
                    }
                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
            
        }


        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task invitebot()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - Invite {application.Name}", application.IconUrl);
            emb.WithDescription($"Добавить <@{application.Id}> на ваш сервер -> [Клик!](https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot)");
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task bug([Remainder] string ErrorMessage)
        {
            _cache.Removes(Context);
            var channel = Context.Client.GetChannel(BotSettings.darlingbug) as ISocketMessageChannel;
            if (channel != null)
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder().WithDescription($"Текст:`{ErrorMessage}`")
                                                                                    .WithColor(255, 0, 94)
                                                                                    .WithAuthor("📛bug - Спасибо за отправку отчета.")
                                                                                    .Build());

                var emb = new EmbedBuilder().WithAuthor($"📛bug", Context.Guild.IconUrl).WithDescription($"{ErrorMessage}")
                                            .AddField("Отправитель: ", Context.User.Id, true).AddField($"Сервер {Context.Guild.Name}", Context.Guild.Id)
                                            .WithFooter("Время отправки: " + DateTimeOffset.Now.ToUniversalTime()).WithColor(255, 0, 94);
                await channel.SendMessageAsync("", false, emb.Build());
            }
            else
                await (Context.Client.GetChannel(BotSettings.SystemMessage) as ISocketMessageChannel)
                    .SendMessageAsync($"{Context.Channel.GetUserAsync(BotSettings.hikaruid).Result.Mention} канал для багов не действительный");
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task pre([Remainder] string Predlojenie)
        {
            _cache.Removes(Context);
            var channel = Context.Client.GetChannel(BotSettings.darlingpre) as ISocketMessageChannel;
            if (channel != null)
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder().WithDescription($"Текст:`{Predlojenie}`")
                                                                                    .WithColor(255, 0, 94)
                                                                                    .WithAuthor("📛Pre - Спасибо за отправку отчета.")
                                                                                    .Build());

                var emb = new EmbedBuilder().WithAuthor($"📛Pre", Context.Guild.IconUrl).WithDescription($"{Predlojenie}")
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
        public async Task serverinfo(ulong ServerId = 0)
        {
            _cache.Removes(Context);
            if (ServerId == 0) ServerId = Context.Guild.Id;
            var emb = new EmbedBuilder().WithColor(255, 0, 94);
            var gldss = _discord.GetGuild(ServerId);
            if (gldss == null) emb.WithAuthor($" serverinfo {ServerId}").WithDescription("Сервер не найден!");
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
        public async Task sendusermessage(ulong UserId, [Remainder]string Message)
        {
            _cache.Removes(Context);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("SendUserMessage");
            try
            {
                var usr = Context.Client.GetUser(UserId).GetOrCreateDMChannelAsync();
                emb.WithDescription("Сообщение успешно отправлено пользователю.");
                await usr.Result.SendMessageAsync(Message);
            }
            catch (Exception)
            {
                emb.WithDescription("У пользователя закрыта личка.");
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [RequireOwner]
        public async Task getinvite([Remainder] string servername)
        {
            _cache.Removes(Context);
            var serv = Context.Client.Guilds.FirstOrDefault(x => x.Name == servername);
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
            _cache.Removes(Context);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"SetLevel {user}");
            using (var DBcontext = new DBcontext())
            {
                var usr = DBcontext.Users.FirstOrDefault(x=>x.UserId == user.Id && x.GuildId == user.Guild.Id);
                emb.WithDescription($"Уровень с {usr.Level} выставлен на {level}");
                usr.XP = level * (80 * level);
                DBcontext.Users.Update(usr);
                await DBcontext.SaveChangesAsync();
                
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
            
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [RequireOwner]
        public async Task banksetcoin(SocketGuildUser user, uint coin)
        {
            _cache.Removes(Context);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"SetBankCoin {user}");
            using (var DBcontext = new DBcontext())
            {
                var usr = DBcontext.Users.FirstOrDefault(x => x.UserId == user.Id && x.GuildId == user.Guild.Id);
                emb.WithDescription($"Банковский счет пользователя {user.Mention} выставлен с {usr.Bank} на {coin}zc");
                usr.Bank = coin;
                DBcontext.Users.Update(usr);
                await DBcontext.SaveChangesAsync();

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [RequireOwner]
        public async Task banksetdate(SocketGuildUser user, [Remainder]string date)
        {
            _cache.Removes(Context);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"SetBankDate {user}");
            using (var DBcontext = new DBcontext())
            {
                var usr = DBcontext.Users.FirstOrDefault(x => x.UserId == user.Id && x.GuildId == user.Guild.Id);
                emb.WithDescription($"Дата пополнения банковского счета пользователя {user.Mention} выставлена с {usr.BankTimer} на {date}");
                try
                {
                    usr.BankTimer = Convert.ToDateTime(date);
                    DBcontext.Users.Update(usr);
                    await DBcontext.SaveChangesAsync();
                }
                catch (Exception)
                {
                    emb.WithDescription("Дата введена неправильно!");
                }
                
                

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [RequireOwner]
        public async Task setpoint(SocketGuildUser user, uint point)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"SetPoint {user}");
            using (var DBcontext = new DBcontext())
            {
                var usr = _cache.GetOrCreateUserCache(user.Id, user.Guild.Id);
                emb.WithDescription($"опыт с {usr.XP} выставлен на {point}");
                usr.XP = point;
                DBcontext.Users.Update(usr);
                await DBcontext.SaveChangesAsync();
                _cache.Removes(Context);
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
                DBcontext.Users.Update(usr);
                await DBcontext.SaveChangesAsync();
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
            
        }
    }
}
