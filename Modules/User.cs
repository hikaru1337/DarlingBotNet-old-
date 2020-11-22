using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DarlingBotNet.Services.CommandHandler;
using Pcg;
using ServiceStack;

namespace DarlingBotNet.Modules
{
    public class User : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        private readonly IMemoryCache _cache;

        public User(DiscordSocketClient discord, CommandService commands, IServiceProvider provider, IMemoryCache cache)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _cache = cache;

        } // Подключение компонентов


        //[Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        //public async Task osuStat([Remainder] string username)
        //{
        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();
        //    OsuSharp.User usr;
        //    //var osuSharpClient = new OsuClient(new OsuSharpConfiguration { ApiKey = "97185d75deafb555d2785390405d121875a5a77f" });
        //    //usr = osuSharpClient.GetUserByUsernameAsync(username, GameMode.Standard).Result;
        //    var OsuChrapClient = new CSharpOsu.OsuClient("97185d75deafb555d2785390405d121875a5a77f");
        //    var user = OsuChrapClient.GetUser(username).First();
        //    //var score = osuSharpClient.GetUserBestsByUsernameAsync(usr.Username, GameMode.Standard).Result.FirstOrDefault();
        //    sw.Stop();
        //    Console.WriteLine(sw.Elapsed);
        //    return;
        //    if (usr == null) await Context.Channel.SendMessageAsync("", false, new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("OsuStat").WithDescription("Пользователь не найден").Build());
        //    else
        //    {
        //        //using (var img = new Image<Rgba32>(1320,352))
        //        //{
        //        //    FontFamily GothamSSm = new FontCollection().Install("images/GothamSSm.ttf");
        //        //    var RedUserName = GothamSSm.CreateFont(86, FontStyle.Italic);
        //        //    var RedTopSocre = GothamSSm.CreateFont(60, FontStyle.Italic);
        //        //    var Scores = GothamSSm.CreateFont(30, FontStyle.Italic);
        //        //    var usravatardown = Image.Load(new WebClient().DownloadData(OsuChrapClient.GetUser(usr.UserId.ToString()).FirstOrDefault().image));
        //        //    var avatar = SixLaborsImage.ApplyRoundedCorners(usravatardown, Math.Max(usravatardown.Width, usravatardown.Width) / 2);

        //        //    img.Mutate(x => x.DrawImage(Image.Load("images/osustat.jpg"),1)
        //        //                   .DrawImage(avatar, new Point(20,19), 1)
        //        //                   .DrawImage(Image.Load("images/circleosustat.png"), new Point(0, 1), 1)
        //        //                   .DrawText($"PP: {usr.PerformancePoints}", GothamSSm.CreateFont(22.24f, FontStyle.Italic), new Rgba32(255, 255, 255), new PointF(323.1f, 192.8f))
        //        //                   .DrawText($"АККУРАТНОСТЬ: {Math.Round(usr.Accuracy, 1)}%", GothamSSm.CreateFont(22.24f, FontStyle.Italic), new Rgba32(255, 255, 255), new PointF(323.1f, 169.5f))
        //        //                   .DrawText($"РЕЙТИНГ В СТРАНЕ: {usr.CountryRank}", GothamSSm.CreateFont(18.81f, FontStyle.Italic), new Rgba32(255, 255, 255), new PointF(323.1f, 147.3f))
        //        //                   .DrawText($"РЕЙТИНГ В МИРЕ: {usr.Rank}", GothamSSm.CreateFont(20.25f, FontStyle.Italic), new Rgba32(255, 255, 255), new PointF(323.1f, 124.1f))
        //        //                   .DrawText(usr.Username, RedUserName, new Rgba32(242, 14, 68), new PointF(323.1f, 30.2f))
        //        //                   .DrawText($"В ИГРЕ: {usr.TimePlayed.Days}D {usr.TimePlayed.Hours}H {usr.TimePlayed.Minutes}M ", GothamSSm.CreateFont(22.24f, FontStyle.Italic), new Rgba32(255, 255, 255), new PointF(323.1f, 237.3f))
        //        //                   .DrawText("TOP SCORE", RedTopSocre, new Rgba32(242, 14, 68), new PointF(916.8f, 40.5f))
        //        //                   .DrawText(score.GetBeatmapAsync().Result.Title, GothamSSm.CreateFont(20.25f, FontStyle.Italic), new Rgba32(255, 255, 255), new PointF(916.8f, 103.8f))
        //        //                   .DrawText($"300: {score.Count300}", Scores, new Rgba32(255, 255, 255), new PointF(916.8f, 144.1f))
        //        //                   .DrawText($"100: {score.Count100}", Scores, new Rgba32(255, 255, 255), new PointF(1107.7f, 144.1f))
        //        //                   .DrawText($"50: {score.Count50}", Scores, new Rgba32(255, 255, 255), new PointF(916.8f, 189.1f))
        //        //                   .DrawText($"MISS: {score.Miss}", Scores, new Rgba32(255, 255, 255), new PointF(1107.7f, 189.1f))
        //        //                   .DrawText($"{Math.Round(score.Accuracy,1)}% acc", Scores, new Rgba32(255, 255, 255), new PointF(916.8f, 237.2f))
        //        //                   .DrawText($"{Math.Round(Convert.ToDouble(score.PerformancePoints),0)} pp", Scores, new Rgba32(255, 255, 255), new PointF(1107.7f, 237.2f))
        //        //                   );
        //        //    await Context.Channel.SendFileAsync(img.ToStream(), "gg.png");
        //        //}

        //    }
        //}

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task level(SocketUser user = null)
        {
            using (var DBcontext = new DBcontext())
            {
                var usr = new Users();
                if (user == null) user = Context.User;

                if (user == Context.User)
                    usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                else
                    usr = DBcontext.Users.FirstOrDefault(x => x.UserId == user.Id && x.GuildId == Context.Guild.Id);


                ulong count = (usr.Level * 80 * usr.Level);
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                                         .WithColor(255, 0, 94)
                                                         .WithAuthor($" - Level {user}", user.GetAvatarUrl())
                                                         .WithDescription($"Уровень: {usr.Level}\n" +
                                                         $"Опыт:{usr.XP - count}/{ (usr.Level + 1) * 80 * (usr.Level + 1) - count}")
                                                         .Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task usertop()
        {
            using (var DBcontext = new DBcontext())
            {
                _cache.Removes(Context);
                var UsersTop = DBcontext.Users.AsQueryable().Where(x => x.GuildId == Context.Guild.Id && !x.Leaved).OrderByDescending(x => (double)x.XP).Take(10);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - TOP 10 SERVER USERS", Context.Guild.IconUrl);
                int count = 0;
                foreach (var usr in UsersTop)
                {
                    count++;
                    var DiscordUser = Context.Guild.GetUser(usr.UserId);
                    Emote emj = null;
                    if (count == 1) emj = Emote.Parse("<a:1place:755825376550322337>");
                    else if (count == 2) emj = Emote.Parse("<a:2place:755825717429796925>");
                    else if (Context.Guild.Owner == DiscordUser) emj = Emote.Parse("<:ServerAdmin:755825374818074725>");
                    emb.AddField($"{(emj == null ? "" : $"{emj} ")}{DiscordUser} - {(DateTime.Now - DiscordUser.JoinedAt).Value.Days} дней", $"LVL: {usr.Level} Money: {usr.ZeroCoin}");
                }
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }

        }

        public static readonly List<Checking> list = new List<Checking>();

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task marry(SocketUser user)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"💞 - Ошибка");
                var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                var ContextUser = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                _cache.Removes(Context);

                bool checks = false;
                if (Context.User != user)
                {
                    var marryuser = DBcontext.Users.FirstOrDefault(x => x.UserId == user.Id && x.GuildId == (user as SocketGuildUser).Guild.Id);
                    if (ContextUser.marryedid != marryuser.UserId)
                    {
                        if (ContextUser.marryedid == 0)
                        {
                            if (marryuser.marryedid == 0)
                            {
                                checks = true;
                                emb.WithAuthor("Marry");
                                var time = DateTime.Now.AddSeconds(60);
                                string text = $"Заявка отправлена {user.Mention}\nПринять: :white_check_mark: Отклонить: :negative_squared_cross_mark:";
                                var mes = await Context.Channel.SendMessageAsync("", false, emb.WithAuthor($"{Context.User} 💞 {user}").WithDescription(text).Build());
                                await mes.AddReactionAsync(new Emoji("✅"));
                                await mes.AddReactionAsync(new Emoji("❎"));
                                var check = new Checking() { userid = user.Id, messid = mes.Id };
                                list.Add(check);
                                while (time > DateTime.Now)
                                {
                                    if ((time - DateTime.Now).Seconds % 10 == 0)
                                        await mes.ModifyAsync(x => x.Embed = emb.WithDescription($"{text}\nОсталось: {(time - DateTime.Now).Seconds} секунд").Build());

                                    var res = list.FirstOrDefault(x => x == check).clicked;
                                    if (res != 0)
                                    {
                                        if (res == 2)
                                        {
                                            ContextUser.marryedid = marryuser.UserId;
                                            marryuser.marryedid = ContextUser.UserId;
                                            DBcontext.Users.Update(ContextUser);
                                            DBcontext.Users.Update(marryuser);
                                            await DBcontext.SaveChangesAsync();
                                            emb.WithDescription($"Теперь вы женаты!");
                                        }
                                        else
                                            emb.WithDescription($"{user.Mention} отказался(лась) от свадьбы!");

                                        break;
                                    }
                                    else if ((time - DateTime.Now).Seconds < 2)
                                    {
                                        emb.WithDescription($"{user.Mention} не успел(а) принять заявку!");
                                        break;
                                    }
                                }

                                await mes.RemoveAllReactionsAsync();
                                await mes.ModifyAsync(x => x.Embed = emb.Build());
                                list.Remove(check);
                            }
                            else emb.WithDescription($"{user} женат(а), нужно сначала развестись!").WithFooter($"Развестить - {GuildPrefix}divorce");
                        }
                        else emb.WithDescription("Вы уже женаты, сначала разведитесь!").WithFooter($"Развестить - {GuildPrefix}divorce");
                    }
                    else emb.WithDescription("Вы уже женаты на этом пользователе");
                }
                else emb.WithDescription("Вы не можете жениться на себе!");

                if (!checks)
                    await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }


        public static readonly List<RouletteBase> RouletteList = new List<RouletteBase>();
        public class RouletteBase
        {
            public ulong userid { get; set; }
            public ulong guildid { get; set; }
            public uint ZeroCoins { get; set; }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task roulette(uint ZeroCoins)
        {
            using (var DBcontext = new DBcontext())
            {
                var DBuser = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                _cache.Removes(Context);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - Roulette", Context.Guild.IconUrl);
                if (DBuser.ZeroCoin >= ZeroCoins)
                {
                    if (ZeroCoins >= 100)
                    {
                        bool Started = false;
                        var UserList = RouletteList.Where(x => x.guildid == Context.Guild.Id);
                        if (UserList.Count() > 0)
                        {
                            if (UserList.FirstOrDefault(x => x.userid == Context.User.Id) == null)
                            {
                                emb.WithDescription($"Вы сделали ставку в размере {ZeroCoins}");
                                Started = true;
                            }
                            else
                                emb.WithDescription("Вы уже сделали ставку!");
                        }
                        else
                        {
                            emb.WithDescription($"Вы запустили казино, сделав ставку в размере {ZeroCoins}");
                            Started = true;
                        }
                        await Context.Channel.SendMessageAsync("", false, emb.Build());

                        if (Started)
                        {
                            RouletteList.Add(new RouletteBase() { guildid = Context.Guild.Id, userid = Context.User.Id, ZeroCoins = ZeroCoins });
                            DBuser.ZeroCoin -= ZeroCoins;
                            DBcontext.Users.Update(DBuser);

                            if (UserList.Where(x => x.userid != Context.User.Id).Count() == 0)
                            {
                                var time = DateTime.Now.AddSeconds(60);
                                string text = $"Рулетка запущена!\nБанк: {UserList.Sum(x => x.ZeroCoins)}\nОсталось: {(time - DateTime.Now).Seconds} секунд";
                                var mes = await Context.Channel.SendMessageAsync("", false, emb.WithDescription(text).Build());

                                while (time > DateTime.Now)
                                {
                                    if ((time - DateTime.Now).Seconds % 10 == 0)
                                    {
                                        UserList = RouletteList.Where(x => x.guildid == Context.Guild.Id);
                                        text = $"Рулетка запущена!\nБанк: {UserList.Sum(x => x.ZeroCoins)}\nОсталось: {(time - DateTime.Now).Seconds} секунд";
                                        await mes.ModifyAsync(x => x.Embed = emb.WithDescription(text).Build());
                                    }
                                }
                                var items = RoulleteWinner(UserList);
                                ulong WinnerUserid = items.Item1;
                                ulong WinnderGuildid = items.Item2;
                                ulong WinCoins = (ulong)UserList.Sum(x => x.ZeroCoins);

                                DBuser = DBcontext.Users.FirstOrDefault(x => x.UserId == WinnerUserid && x.GuildId == WinnderGuildid);
                                DBuser.ZeroCoin += WinCoins;
                                DBcontext.Users.Update(DBuser);

                                RouletteList.RemoveAll(x => x.guildid == WinnderGuildid);

                                emb.WithDescription($"Рулетка остановлена!\nПобедитель: <@{WinnerUserid}>\nВыигрыш: {WinCoins}");
                                await Context.Channel.SendMessageAsync("", false, emb.Build());
                            }
                            await DBcontext.SaveChangesAsync();
                        }
                    }
                    else
                        await Context.Channel.SendMessageAsync("", false, emb.WithDescription("Ставка не может быть меньше 99 ZeroCoin's!").Build());
                }
                else
                {
                    await Context.Channel.SendMessageAsync("", false, emb.WithDescription($"У вас недостаточно средств для ставки.").Build());
                }
            }
        }
        private static (ulong, ulong) RoulleteWinner(IEnumerable<RouletteBase> users)
        {

            ulong bank = (uint)users.Sum(x => x.ZeroCoins);

            List<(ulong, ulong)> usersID = new List<(ulong, ulong)>(); //сами проценты        

            // распределение процентов от кол-ва денег
            foreach (var user in users)
            {
                double percent = ((Convert.ToDouble(user.ZeroCoins) / bank) * 100);
                for (int i = 0; i < Math.Round(percent); i++)
                    usersID.Add((user.userid, user.guildid));
            }

            //проверка распределения на точность
            double[] onepercent = new double[users.Count()];
            if (usersID.Count < 100)
            {
                double max = 0;
                int maxI = 0;

                for (int i = 0; i < users.Count(); i++)
                {
                    onepercent[i] = (Convert.ToDouble(users.ElementAt(i).ZeroCoins) / bank * 100) - Math.Truncate(Convert.ToDouble(users.ElementAt(i).ZeroCoins) / bank * 100);
                    if (onepercent[i] > max)
                    {
                        max = onepercent[i];
                        maxI = i;
                    }
                }

                var Userelement = users.ElementAt(maxI);

                usersID.Add((Userelement.userid, Userelement.guildid));
            }
            else if (usersID.Count > 100)
            {
                double min = 0;
                int minI = 0;
                for (int i = 0; i < users.Count(); i++)
                {
                    onepercent[i] = (Convert.ToDouble(users.ElementAt(i).ZeroCoins) / bank * 100) - Math.Truncate(Convert.ToDouble(users.ElementAt(i).ZeroCoins) / bank * 100);
                    if (onepercent[i] < min)
                    {
                        min = onepercent[i];
                        minI = i;
                    }
                }
                var Userelement = users.ElementAt(minI);

                usersID.Remove((Userelement.userid, Userelement.guildid));
            }
            //генерация победителя
            return usersID[new Random().Next(0, 100)];
        }


        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task divorce()
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - divorce", Context.User.GetAvatarUrl());
            using (var DBcontext = new DBcontext())
            {
                Users ContextUser = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                if (ContextUser.marryedid == 0)
                    emb.WithDescription($"Вы не женаты!");
                else
                {
                    var marryed = _cache.GetOrCreateUserCache(ContextUser.marryedid, Context.Guild.Id);
                    marryed.marryedid = 0;
                    ContextUser.marryedid = 0;
                    DBcontext.Users.Update(ContextUser);
                    DBcontext.Users.Update(marryed);
                    await DBcontext.SaveChangesAsync();
                    emb.WithDescription($"Вы успешно развелись с <@{marryed.UserId}>!");
                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        //[Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        //public async Task demotiv(string url, [Remainder] string text)
        //{
        //    var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - Демотиватор", Context.User.GetAvatarUrl());
        //    var urlPhoto = Image.Load(new WebClient().DownloadData(url));
        //    if (urlPhoto != null)
        //    {
        //        using (var img = new Image<Rgba32>((urlPhoto.Width + (int)(urlPhoto.Width * 0.2)), (urlPhoto.Height + (int)(urlPhoto.Height * 0.8))))
        //        {
        //            FontFamily GothamSSm = new FontCollection().Install("UserProfile/Akrobat-Regular.ttf");
        //            var Font = GothamSSm.CreateFont(50);
        //            img.Mutate(x => x.BackgroundColor(new Rgba32(10, 10, 10)).DrawLines(new Rgba32(255, 255, 255), 1, new PointF[] { new PointF(10, 10), new PointF(10, 20) })
        //                             .DrawImage(urlPhoto, 1, new Point((int)(urlPhoto.Width * 0.2) / 2, (int)((urlPhoto.Height * 0.8) / 3)))
        //                             .DrawText(new TextGraphicsOptions(true)
        //                             {
        //                                 HorizontalAlignment = HorizontalAlignment.Center,
        //                                 WrapTextWidth = img.Width
        //                             }, text, Font, new Rgba32(255, 255, 255), new Vector2(0, (int)(img.Height * 0.8) + 50))


        //                             );
        //            await Context.Channel.SendFileAsync(img.ToStream(), "gg.png");
        //        }
        //    }
        //    else
        //    {
        //        emb.WithDescription("Ссылка не имеет вложенное изображение!");
        //        await Context.Channel.SendMessageAsync("", false, emb.Build());
        //    }
        //}

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task zcoin(SocketUser user = null)
        {
            using (var DBcontext = new DBcontext())
            {
                ulong UserZeroCoins = 0;
                if (user == null) user = Context.User;

                if (user == Context.User)
                    UserZeroCoins = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id).ZeroCoin;
                else
                    UserZeroCoins = DBcontext.Users.FirstOrDefault(x => x.UserId == user.Id && x.GuildId == Context.Guild.Id).ZeroCoin;
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                                         .WithColor(255, 0, 94)
                                                         .WithAuthor($" - ZeroCoin {user}", user.GetAvatarUrl())
                                                         .WithDescription($"zcoin: {UserZeroCoins}")
                                                         .Build());
            }
        }

        public enum Fishka
        {
            allzero,
            allblack,
            allred,
            zero,
            black,
            red
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task kazino(Fishka Fishka, ulong Stavka = 0)
        {
            EmbedBuilder emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - Kazino", Context.User.GetAvatarUrl());
            using (var DBcontext = new DBcontext())
            {
                var account = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                if (Fishka == Fishka.allblack || Fishka == Fishka.allred || Fishka == Fishka.allzero)
                {
                    if (account.ZeroCoin >= 30000)
                        Stavka = 30000;
                    else
                        Stavka = account.ZeroCoin;
                }
                if (Stavka >= 100 && Stavka <= 30000)
                {
                    if (account.ZeroCoin >= Stavka)
                    {
                        int ches = new PcgRandom().Next(6);
                        emb.WithAuthor(" - Kazino - ✔️ Win", Context.User.GetAvatarUrl());
                        if ((Fishka == Fishka.black || Fishka == Fishka.allblack) && ches % 2 == 1 && ches != 5)
                            account.ZeroCoin += Stavka;
                        else if ((Fishka == Fishka.red || Fishka == Fishka.allred) && ches % 2 == 0)
                            account.ZeroCoin += Stavka;
                        else if (ches == 5 && (Fishka == Fishka.zero || Fishka == Fishka.allzero))
                        {
                            var UserBoost = DBcontext.DarlingBoost.FirstOrDefault(x=>x.UserId == Context.User.Id);
                            if(UserBoost != null && UserBoost.Ends > DateTime.Now)
                                account.ZeroCoin += Stavka * 10;
                            else
                                account.ZeroCoin += Stavka * 5;
                        }
                        else
                        {
                            account.ZeroCoin -= Stavka;
                            emb.WithAuthor(" - Kazino - ❌ Lose", Context.User.GetAvatarUrl());
                        }
                        emb.WithDescription($"Выпало: {(ches != 5 && ches % 2 == 1 ? "black" : (ches % 2 == 0) ? "red" : "zero")}\nZeroCoin: {account.ZeroCoin}");

                        if (emb.Author.Name == " - Kazino - ✔️ Win")
                        {
                            int rnd = new PcgRandom(1488).Next(0, 1000);
                            if (rnd <= 100)
                            {
                                int moneyrnd = new PcgRandom(1488).Next(300, 3000);
                                account.ZeroCoin += (uint)moneyrnd;
                                if (rnd >= 0 && rnd <= 25)
                                    emb.Description += $"\n\nSyst3mm er0r g1ved u {moneyrnd} coin's";
                                else if (rnd > 25 && rnd <= 50)
                                    emb.Description += $"\n\nОш11бка, в2дан7 с2мма {moneyrnd} coin's";
                                else if (rnd > 50 && rnd <= 75)
                                    emb.Description += $"\n\nПолучена сумма {moneyrnd} coin's";
                                else if (rnd > 75 && rnd <= 100)
                                    emb.Description += $"\n\n{moneyrnd} coin's выдано {Context.User.Mention}";
                            }
                        }

                        DBcontext.Users.Update(account);
                        await DBcontext.SaveChangesAsync();
                    }
                }
                else
                    emb.WithDescription($"Ставка может быть только больше 100 и меньше 30000");



                //    ulong Coins = 0;
                //    if (Stavka == "all")
                //        Coins = account.ZeroCoin;
                //    else if (Stavka.Count(x => x >= 48 && x <= 57) >= 3 && Stavka.Count(x => x >= 48 && x <= 57) <= 4)
                //        Coins = Convert.ToUInt32(String.Concat(Stavka.Where(x => x >= 48 && x <= 57)));

                //    if (Coins >= 100 && Coins <= 20000)
                //    {
                //        if (account.ZeroCoin >= Coins)
                //        {
                //            if (Fishka.ToLower() == "black" || Fishka.ToLower() == "zero" || Fishka.ToLower() == "red")
                //            {
                //                int ches = new PcgRandom().Next(6);
                //                emb.WithAuthor(" - Kazino - ✔️ Win", Context.User.GetAvatarUrl());
                //                if (ches != 5 && ches % 2 == 1 && Fishka.ToLower() == "black" || ches % 2 == 0 && Fishka.ToLower() == "red")
                //                    account.ZeroCoin += Coins;
                //                else if (ches == 5 && Fishka.ToLower() == "zero")
                //                    account.ZeroCoin += Coins * 5;
                //                else
                //                {
                //                    account.ZeroCoin -= Coins;
                //                    emb.WithAuthor(" - Kazino - ❌ Lose", Context.User.GetAvatarUrl());
                //                }
                //                emb.WithDescription($"Выпало: {(ches % 2 == 1 ? "black" : (ches != 10 && ches % 2 == 0) ? "red" : "zero")}\nZeroCoin: {account.ZeroCoin}");

                //                if (emb.Author.Name == " - Kazino - ✔️ Win")
                //                {
                //                    int rnd = new PcgRandom(1488).Next(0, 1000);
                //                    if (rnd <= 100)
                //                    {
                //                        int moneyrnd = new PcgRandom(1488).Next(300, 3000);
                //                        account.ZeroCoin += (uint)moneyrnd;
                //                        if (rnd >= 0 && rnd <= 25)
                //                            emb.Description += $"\n\nSyst3mm er0r g1ved u {moneyrnd} coin's";
                //                        else if (rnd > 25 && rnd <= 50)
                //                            emb.Description += $"\n\nОш11бка, в2дан7 с2мма {moneyrnd} coin's";
                //                        else if (rnd > 50 && rnd <= 75)
                //                            emb.Description += $"\n\nПолучена сумма {moneyrnd} coin's";
                //                        else if (rnd > 75 && rnd <= 100)
                //                            emb.Description += $"\n\n{moneyrnd} coin's выдано {Context.User.Mention}";
                //                    }
                //                }

                //                DBcontext.Users.Update(account);
                //                await DBcontext.SaveChangesAsync();
                //            }
                //            else
                //            {
                //                var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                //                emb.WithDescription("Ваша ставка должна быть black,zero или red").WithFooter($"Пример - {Guild}kz [black/zero/red] [Кол-во ZeroCoin's]");
                //            }
                //        }
                //        else emb.WithDescription($"{Context.User.Mention} на вашем счете недостаточно средств.\nВаш баланс: {account.ZeroCoin} ZeroCoin");
                //    }
                //    else emb.WithDescription($"Ставка может быть только больше 99 и меньше 9999, или же быть `all`").WithFooter("all - выставить все");
            }
            _cache.Removes(Context);
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task daily()
        {
            using (var DBcontext = new DBcontext())
            {
                var usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - daily 🏧", Context.User.GetAvatarUrl());

                if (DateTime.Now > usr.Daily)
                {
                    if (Math.Abs(DateTime.Now.Day - usr.Daily.Day) > 1)
                        usr.Streak = 1;
                    else
                        usr.Streak++;

                    ulong amt = 500 + ((500 / 35) * usr.Streak);


                    usr.Daily = DateTime.Now.AddDays(1);
                    emb.WithDescription($"Получено: {amt} ZeroCoin's!\nStreak: {usr.Streak}");
                    var UserBoost = DBcontext.DarlingBoost.FirstOrDefault(x=>x.UserId == Context.User.Id);
                    if(UserBoost != null && UserBoost.Ends > DateTime.Now)
                    {
                        usr.XP += 100;
                        emb.Description +="DarlingBoost: 100 XP";
                    }
                    if (usr.Bank > 5000 && usr.Streak > 10)
                    { 
                        ulong bankMoney = (ulong)Math.Truncate(usr.Bank * 0.005 * (1 + usr.Streak * 0.15));
                        emb.Description += $"\nКэшБэк от банка🤑: +{bankMoney}";
                    }

                    
                    if (usr.ClanId != 0 && usr.clanInfo != Users.UserClanRole.ready)
                    {
                        var clan = DBcontext.Clans.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.Id == usr.ClanId);
                        if (clan.ClanMoney > -50000)
                        {
                            var ClanDaily = (long)(amt * 0.15);
                            clan.ClanMoney += ClanDaily;
                            emb.Description += $"\nКлан получил {ClanDaily} zcoin, от daily!";
                            amt -= (ulong)ClanDaily;
                        }
                        await Clan.ClanPay(clan);
                    }
                    usr.ZeroCoin += amt;

                    int rnd = new PcgRandom(1488).Next(0, 1000);
                    if (rnd <= 100)
                    {
                        int moneyrnd = new PcgRandom(1488).Next(300, 3000);
                        usr.ZeroCoin += (uint)moneyrnd;
                        emb.Description += $"\n\nВозвращаясь домой, на дороге вы нашли {moneyrnd} coin's";
                    }
                    else if(rnd >= 900)
                    {
                        int moneyrnd = new PcgRandom(1488).Next(0, Convert.ToInt32(usr.ZeroCoin*0.5));
                        usr.ZeroCoin -= (uint)moneyrnd;
                        emb.Description += $"\n\nВозвращаясь домой, на вас напали и забрали {moneyrnd} coin's";
                    }
                    DBcontext.Users.Update(usr);
                    await DBcontext.SaveChangesAsync();
                }
                else
                {
                    var TimeToDaily = (usr.Daily - DateTime.Now);
                    if (TimeToDaily.TotalSeconds >= 3600)
                        emb.WithDescription($"Дождитесь {TimeToDaily.Hours} часов и {TimeToDaily.Minutes} минут чтобы получить Daily!");

                    if (TimeToDaily.TotalSeconds <= 3600)
                        emb.WithDescription($"Дождитесь {(TimeToDaily.TotalSeconds > 60 ? $"{TimeToDaily.Minutes} минут и " : "")} {TimeToDaily.Seconds} секунд чтобы получить Daily!");
                }

                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task profile(SocketUser user = null)
        {
            using (var DBcontext = new DBcontext())
            {
                var usr = new Users();
                if (user == null) user = Context.User;

                if (user == Context.User)
                    usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                else
                    usr = DBcontext.Users.FirstOrDefault(x => x.UserId == user.Id && x.GuildId == Context.Guild.Id);


                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithTitle($"Profile {user}").WithThumbnailUrl(user.GetAvatarUrl());
                var UserBoost = DBcontext.DarlingBoost.FirstOrDefault(x=>x.UserId == Context.User.Id);
                if (UserBoost != null && UserBoost.Ends > DateTime.Now)
                {
                    if ((UserBoost.Ends - DateTime.Now).TotalDays < 7)
                        emb.Title += $" - {BotSettings.EmoteBoostNo}";

                    if ((UserBoost.Ends - DateTime.Now).TotalDays == 7)
                        emb.Title += $" - {BotSettings.EmoteBoostLastDay}";

                    else
                        emb.Title += $" - {BotSettings.EmoteBoostNot}";
                }
                var Clan = DBcontext.Clans.FirstOrDefault(x => x.Id == usr.ClanId);
                if (usr.ClanId != 0 && usr.clanInfo != Users.UserClanRole.ready)
                {
                    emb.AddField($"Клан {Clan.ClanName}", $"Участников: {Clan.DefUsers.Count()}", true);
                }
                else if (usr.clanInfo == Users.UserClanRole.owner)
                {
                    emb.AddField($"Создатель клана: {Clan.ClanName}", $"Участников: {Clan.DefUsers.Count()}", true);
                }
                if (usr.marryedid != 0)
                {
                        emb.Description += $"Женат(а) на <@{usr.marryedid}>\n";
                }
                var WarnsCount = DBcontext.Warns.Count(x => x.GuildId == Context.Guild.Id);
                emb.Description += $"ZeroCoin's: {usr.ZeroCoin}\nDaily Streak: {usr.Streak}\nБанк: {usr.Bank}\nLevel: {usr.Level}\nWarns: {usr.countwarns}/{WarnsCount}";

                var TimeToDaily = (usr.Daily - DateTime.Now);

                if (TimeToDaily.Seconds > 0)
                    emb.WithFooter($"До Daily - {TimeToDaily.Hours}:{TimeToDaily.Minutes}:{TimeToDaily.Seconds}");
                else
                    emb.WithFooter($"До сброса Daily - {24 + TimeToDaily.Hours}:{60 + TimeToDaily.Minutes}:{ 60 + TimeToDaily.Seconds}");

                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }



        //[Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        //public async Task Lottery(ulong Id = 0, uint NumberLot = 0)
        //{
        //    using (var DBcontext = new DBcontext())
        //    {

        //        var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Lottery");
        //        if (Id == 0 && NumberLot == 0)
        //        {
        //            var Lotterys = DBcontext.Lottery.AsQueryable().Where(x => x.TimeStart > DateTime.Now).AsEnumerable().Where(x=> x.LotsList == null || x.LotsList.Count < x.CountLots);
        //            int i = 0;
        //            foreach (var Lottery in Lotterys)
        //            {
        //                i++;
        //                emb.Description += $"{i}.№{Lottery.Id} - ПРИЗ:{Lottery.MoneyWins} | WINNERS:{Lottery.CountWins} | ЯЧЕЙКИ:{(Lottery.LotsList == null ? "0" : Lottery.LotsList.Count.ToString())}/{Lottery.CountLots}";
        //            }
        //            await Context.Channel.SendMessageAsync("", false, emb.Build());
        //        }
        //        else if (Id != 0 && NumberLot == 0)
        //        {
        //            var sw = new Stopwatch();
        //            sw.Start();
        //            SocketUser WinnerUser = null;
        //            Image UserAvatar = Image.Load("UserProfile/LotteryDEFFON.png");
        //            var LotteryId = DBcontext.Lottery.AsEnumerable().ElementAt((int)--Id);
        //            if (LotteryId.TimeEnd < DateTime.Now)
        //            {
        //                WinnerUser = _discord.GetUser(LotteryId.WinId);
        //                UserAvatar = Image.Load(new WebClient().DownloadData(WinnerUser.GetAvatarUrl(ImageFormat.Png,1024)));
        //                if (UserAvatar.Width < 700)
        //                {
        //                    UserAvatar = UserAvatar.Clone(context => context
        //                    .Resize(new ResizeOptions
        //                    {
        //                        Mode = ResizeMode.Max,
        //                        Size = new Size(722, 722)
        //                    }));
        //                }
        //            }
        //            Image BlockImage = null;
        //            string Path = $"UserProfile/LotteryNumber{LotteryId.CountLots}.png";
        //            bool Num = false;
        //            if (LotteryId.LotsList != null && LotteryId.LotsList.Count(x => x > 0) / 2 > LotteryId.CountLots)
        //            {
        //                    Num = true;
        //                    BlockImage = SixLabors.ImageSharp.Image.Load($"UserProfile/Lottery{LotteryId.CountLots}CubeNotNum.png");
        //            }
        //            else
        //            {
        //                BlockImage = SixLabors.ImageSharp.Image.Load($"UserProfile/Lottery{LotteryId.CountLots}CubeNum.png");
        //            }

        //            using (var img = new Image<Rgba32>(2000, 721))
        //            {
        //                Image TimeGrad = Image.Load($"UserProfile/LotterySTARTGRAD.png");
        //                var Akrobat = new FontCollection().Install("UserProfile/Gotham.ttf");
        //                Font LotteryText = Akrobat.CreateFont(80f, FontStyle.Bold);
        //                var LotText = Akrobat.CreateFont(40f, FontStyle.Bold);
        //                img.Mutate(x => x.BackgroundColor(new Argb32(40, 40, 40))
        //                               .DrawImage(UserAvatar, new Point(1280, 0), 0.7f)
        //                               .DrawImage(Image.Load("UserProfile/LotteryBG.png"), 1)
        //                               .DrawImage(TimeGrad, 1).DrawImage(BlockImage, 1)
        //                               .DrawText($"ЛОТЕРЕЯ №{LotteryId.Id}", LotteryText, new Rgba32(255, 255, 255), new PointF(51, 60))
        //                               .DrawText($"КОЛ-ВО ЯЧЕЕК: {LotteryId.CountLots}", LotText, new Rgba32(255, 255, 255), new PointF(51, 152))
        //                               .DrawText($"ПРИЗ: {LotteryId.MoneyWins}", LotText, new Rgba32(255, 255, 255), new PointF(51, 214))
        //                               .DrawText($"НАЧАЛО: {LotteryId.TimeStart.ToString("dd:MM:yyyy HH:mm")}", LotText, new Rgba32(255, 255, 255), new PointF(51, 624))
                                       
        //                               );

        //                int Height = 1237;
        //                int Width = 307;
        //                switch (LotteryId.CountLots)
        //                {
        //                    case 60:
        //                        Height = 544;
        //                        Width = 324;
        //                        break;
        //                    case 120:
        //                        Height = 1094;
        //                        Width = 324;
        //                        break;
        //                }

        //                using (var imgBlock = new Image<Rgba32>(Height, Width))
        //                {
        //                    int xx = 0;
        //                    for (int i = 0; i < LotteryId.LotsMassive.GetLength(0); i++)
        //                    {
        //                        int yy = 0;
        //                        for (int j = 0; j < LotteryId.LotsMassive.GetLength(1); j++)
        //                        {

        //                                //imgBlock.Mutate(x => x.DrawImage(Image.Load($"UserProfile/LotteryPixel{(LotteryId.CountLots == 400 ? "400" : "")}{(!Num ? "Not" : "")}Num.png"), new Point(xx, yy), GraphicsOptions.Default));


        //                            if (!Num)
        //                            {
        //                                if (LotteryId.LotsMassive[i, j] == 0)
        //                                    imgBlock.Mutate(x => x.DrawImage(Image.Load($"UserProfile/LotteryPixel{(LotteryId.CountLots == 400 ? "400" : "")}NotNum.png"), new Point(yy, xx), GraphicsOptions.Default));
        //                            }
        //                            else
        //                            {
        //                                if (LotteryId.LotsMassive[i, j] != 0)
        //                                    imgBlock.Mutate(x => x.DrawImage(Image.Load($"UserProfile/LotteryPixel{(LotteryId.CountLots == 400 ? "400" : "")}Num.png"), new Point(yy, xx), GraphicsOptions.Default));
        //                            }
        //                            if(LotteryId.CountLots != 400)
        //                                yy += 55;
        //                            else
        //                                yy += 31;

        //                        }
        //                        if (LotteryId.CountLots != 400)
        //                            yy += 55;
        //                        else
        //                            yy += 31;
        //                    }
        //                    img.Mutate(x => x.DrawImage(imgBlock,new Point(51,277),1).DrawImage(Image.Load(Path), 1));
        //                }
        //                sw.Stop();
        //                await Context.Channel.SendFileAsync(img.ToStream(), "gg.png");
        //                await Context.Channel.SendMessageAsync(sw.Elapsed.ToString());
        //            }

        //        }
        //        else
        //        {
        //            var LotteryId = DBcontext.Lottery.AsEnumerable().ElementAt((int)Id);
        //            if (LotteryId != null)
        //            {
        //                var ListLots = LotteryId.LotsList;
        //                ListLots[(int)NumberLot] = Context.User.Id;
        //                LotteryId.LotsList = ListLots;
        //                emb.WithDescription($"Вы успешно купили ячейку {NumberLot} в билете №{LotteryId.Id}");
        //            }
        //            else
        //                emb.WithDescription($"Лотерея с номером {Id} не найдена!");
        //            await Context.Channel.SendMessageAsync("", false, emb.Build());
        //        }
        //    }
        //}

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task bank()
        {
            using (var DBcontext = new DBcontext())
            {
                var usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                ulong limituser = 100000;
                var UserBoost = DBcontext.DarlingBoost.FirstOrDefault(x=>x.UserId == Context.User.Id);
                if(UserBoost != null && UserBoost.Ends > DateTime.Now)
                    limituser = 150000;
                
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - Bank 🏧", Context.User.GetAvatarUrl());
                //var CoinsDay = Math.Truncate(usr.Bank * (1 + 4.5 / 100) - usr.Bank);
                double CoinsDayDaily = Math.Truncate(usr.Bank * 0.005 * (1 + usr.Streak * 0.15));
                if (usr.Streak > 10)
                {
                    if (usr.Bank > 0 && usr.Bank < limituser)
                    {
                        if (DateTime.Now > usr.BankLastTransit)
                        {
                            for (int i = 0; i < (DateTime.Now - usr.BankLastTransit).TotalDays; i++)
                            {
                                CoinsDayDaily = Math.Truncate(usr.Bank * 0.005 * (1 + usr.Streak * 0.15));
                                //CoinsDayDaily = Math.Truncate(usr.Bank * (1 + (0.027 + Math.Log(usr.Streak  - 10)) / 100) - usr.Bank);
                                usr.Bank += (ulong)CoinsDayDaily;
                            }
                            usr.BankLastTransit = DateTime.Now.AddDays(1);
                            DBcontext.Users.Update(usr);
                            await DBcontext.SaveChangesAsync();
                        }
                    }
                }

                if (usr.Bank > limituser)
                    usr.Bank = limituser;

                emb.WithDescription($"Заложено: {(usr.Bank < limituser ? usr.Bank.ToString() : $"{usr.Bank} limit!")}\nКол-во Coin в день: {CoinsDayDaily}");
                emb.WithFooter("Можно снять/положить: ");
                if (usr.BankTimer.Year == 1)
                    usr.BankTimer = DateTime.Now;
                var TimeToTransit = usr.BankTimer - DateTime.Now;
                if (TimeToTransit.TotalSeconds > 0)
                {
                    if (TimeToTransit.TotalDays > 1)
                        emb.Footer.Text += $"через {TimeToTransit.Days} дней {TimeToTransit.Hours} часов и {TimeToTransit.Minutes} минут!";
                    else if (TimeToTransit.TotalSeconds >= 3600)
                        emb.Footer.Text += $"через {TimeToTransit.Hours} часов и {TimeToTransit.Minutes} минут!";
                    else
                        emb.Footer.Text += $"через {(TimeToTransit.TotalSeconds > 60 ? $"{TimeToTransit.Minutes} минут и " : "")} {TimeToTransit.Seconds} секунд!";
                }
                else
                {
                    var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                    emb.AddField("Пополнить счёт", $"{GuildPrefix}BankAdd [coins]", true);
                    emb.AddField("Снять со счёта", $"{GuildPrefix}BankGive [coins]", true);
                    emb.Footer.Text += "сейчас";
                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task bankadd(ulong coins)
        {
            using (var DBcontext = new DBcontext())
            {
                var usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                ulong limituser = 100000;
                var UserBoost = DBcontext.DarlingBoost.FirstOrDefault(x => x.UserId == Context.User.Id);
                if (UserBoost != null && UserBoost.Ends > DateTime.Now)
                    limituser = 150000;
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - BankAdd 🏧", Context.User.GetAvatarUrl());
                if (usr.Bank < limituser)
                {
                    if ((limituser - usr.Bank) >= coins)
                    {
                        if (coins <= 30000)
                        {
                            if (usr.ZeroCoin >= coins)
                            {
                                if (usr.BankTimer < DateTime.Now)
                                {
                                    usr.BankTimer = DateTime.Now.AddDays(7);
                                    if (usr.BankLastTransit.Year < DateTime.Now.AddYears(-1).Year)
                                        usr.BankLastTransit = DateTime.Now;
                                    usr.Bank += coins;
                                    usr.ZeroCoin -= coins;
                                    DBcontext.Users.Update(usr);
                                    await DBcontext.SaveChangesAsync();
                                    emb.WithDescription($"Банк пополнен на {coins} zerocoins!\n\nВаш банковский счет: {usr.Bank}\nВаш наличный счет: {usr.ZeroCoin}");
                                }
                                else
                                {
                                    var TimeToTransit = usr.BankTimer - DateTime.Now;
                                    if (TimeToTransit.TotalDays > 1)
                                        emb.WithDescription($"Вы сможете пополнить счет через {TimeToTransit.Days} дней {TimeToTransit.Hours} часов и {TimeToTransit.Minutes} минут!");
                                    else if (TimeToTransit.TotalSeconds >= 3600)
                                        emb.WithDescription($"Вы сможете пополнить счет через {TimeToTransit.Hours} часов и {TimeToTransit.Minutes} минут!");
                                    else
                                        emb.WithDescription($"Вы сможете пополнить счет через {(TimeToTransit.TotalSeconds > 60 ? $"{TimeToTransit.Minutes} минут и " : "")} {TimeToTransit.Seconds} секунд!");

                                }
                            }
                            else
                                emb.WithDescription($"У вас недосаточно средств на наличном счете!\nВаш счет: {usr.ZeroCoin} ZeroCoins");
                        }
                        else
                            emb.WithDescription("Перевод больше 30000 ZeroCoins на банковский счет невозможен!");
                    }
                    else
                        emb.WithDescription($"Сумма пополнения выходит за лимит аккаунта на {limituser - usr.Bank} zerocoins");
                }
                else emb.WithDescription($"Ваш банк заполнен! Лимит вашего аккаунта {limituser}!");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task bankgive(ulong coins)
        {
            using (var DBcontext = new DBcontext())
            {
                var usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - BankGive 🏧", Context.User.GetAvatarUrl());
                if (usr.Bank >= coins)
                {
                    if (usr.BankTimer < DateTime.Now)
                    {
                        usr.BankTimer = DateTime.Now.AddDays(7);
                        usr.Bank -= coins;
                        usr.ZeroCoin += coins;
                        DBcontext.Users.Update(usr);
                        await DBcontext.SaveChangesAsync();
                        emb.WithDescription($"Со счёта списано {coins} zerocoins!\nВаш банковский счет: {usr.Bank}\nВаш наличный счет: {usr.ZeroCoin}");
                    }
                    else
                    {
                        var TimeToTransit = usr.BankTimer - DateTime.Now;
                        if (TimeToTransit.TotalDays > 1)
                            emb.WithDescription($"Вы сможете снять деньги через {TimeToTransit.Days} дней {TimeToTransit.Hours} часов и {TimeToTransit.Minutes} минут!");
                        else if (TimeToTransit.TotalSeconds >= 3600)
                            emb.WithDescription($"Вы сможете снять деньги через {TimeToTransit.Hours} часов и {TimeToTransit.Minutes} минут!");
                        else
                            emb.WithDescription($"Вы сможете снять деньги через {(TimeToTransit.TotalSeconds > 60 ? $"{TimeToTransit.Minutes} минут и " : "")} {TimeToTransit.Seconds} секунд!");

                    }
                }
                else
                    emb.WithDescription($"На вашем банковском счёте нехватает {coins - usr.Bank} ZeroCoins, для транзакции!");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task transfer(SocketGuildUser user, ushort coin)
        {
            EmbedBuilder emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"{Context.User} 💱 {user}");
            using (var DBcontext = new DBcontext())
            {
                if (user != Context.User)
                {
                    if (coin <= 10000)
                    {
                        var usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                        if (usr.ZeroCoin >= coin)
                        {
                            var transfuser = _cache.GetOrCreateUserCache(user.Id, Context.Guild.Id);
                            usr.ZeroCoin -= coin;
                            transfuser.ZeroCoin += coin;
                            emb.WithDescription($"Перевод в размере {coin} zerocoin успешно прошел.");
                            DBcontext.Users.Update(usr);
                            DBcontext.Users.Update(transfuser);
                            await DBcontext.SaveChangesAsync();
                        }
                        else emb.WithDescription($"У вас недостаточно средств для перевода. Вам нехватает {coin - usr.ZeroCoin}");
                    }
                    else emb.WithDescription($"Перевести больше 10000 zerocoin нельзя.");
                }
                else emb.WithDescription("Переводить деньги самому себе нельзя!");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [RequireBotPermission(GuildPermission.ManageGuild)]
        public async Task myinvite()
        {
            _cache.Removes(Context);
            EmbedBuilder emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Инвайты на {Context.Guild.Name} от {Context.User}", Context.Guild.IconUrl);
            foreach (var invite in Context.Guild.GetInvitesAsync().Result.Where(x => x.Inviter.Id == Context.User.Id))
            {
                string text = null;
                if (invite.MaxAge > 0)
                {
                    //var Times = DateTime.Now.AddSeconds(invite.MaxAge.Value);
                    var Time = (invite.CreatedAt.Value.AddSeconds((double)invite.MaxAge) - DateTime.Now);

                    if (Time.TotalSeconds > 3600)
                        text += $"{Time.Hours}h и {Time.Minutes}m!";
                    else if (Time.TotalSeconds <= 3600)
                        text += $"{(Time.TotalSeconds > 60 ? $"{Time.Minutes}m и" : "")} {Time.Seconds}s!";
                }
                emb.AddField($"ID: {invite.Id}",
                             $"Использований: {invite.Uses}/{(invite.MaxUses == 0 ? "∞" : invite.MaxUses.ToString())}\n" +
                             $"{(invite.MaxAge != 0 ? $"Осталось: {text}" : "")}",
                             true);

            }
            if (emb.Fields.Count == 0)
                emb.WithDescription("Инвайты отсутствуют.");
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionViolation]
        public async Task warns()
        {
            _cache.Removes(Context);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Warns");
            using (var DBcontext = new DBcontext())
            {
                var warns = DBcontext.Warns.AsQueryable().Where(x => x.GuildId == Context.Guild.Id);
                foreach (var warn in warns)
                {
                    string text = warn.ReportTypes.ToString();

                    if (warn.ReportTypes == Warns.ReportType.tban)
                        text = $"Бан на {warn.MinutesWarn} минут";
                    else if (warn.ReportTypes == Warns.ReportType.tmute)
                        text = $"Мут на {warn.MinutesWarn} минут";

                    emb.Description += $"{warn.CountWarn}.{text}\n";
                }
                if (warns.Count() == 0)
                    emb.WithDescription("На сервере еще нет варнов!");
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task levelrole()
        {
            using (var DBcontext = new DBcontext())
            {
                var lvl = DBcontext.LVLROLES.AsQueryable().Where(x => x.GuildId == Context.Guild.Id).OrderBy(u => (uint)u.CountLvl);
                var embed = new EmbedBuilder().WithAuthor($"🔨LevelRole - уровневые роли {(lvl.Count() != 0 ? "" : "отсутствуют ⚠️")}")
                                              .WithColor(255, 0, 94);
                foreach (var LVL in lvl)
                    embed.Description += $"{LVL.CountLvl} уровень - <@&{LVL.RoleId}>\n";

                if (Context.Guild.Owner == Context.User)
                {
                    var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                    embed.AddField("Добавить роль", $"{GuildPrefix}lr.Add [ROLE] [LEVEL]");
                    embed.AddField("Удалить роль", $"{GuildPrefix}lr.Del [ROLE]");
                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        public static readonly List<BuysRole> BuyRole = new List<BuysRole>();
        public class BuysRole
        {
            public ulong userid { get; set; }
            public ulong guildid { get; set; }
            public uint SelectItem { get; set; }
            public ulong MessageId { get; set; }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task buyrole()
        {
            using (var DBcontext = new DBcontext())
            {
                var embed = new EmbedBuilder().WithColor(255, 0, 94);
                var user = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                _cache.Removes(Context);

                var DBroles = DBcontext.RoleSwaps.AsQueryable().Where(x => x.GuildId == Context.Guild.Id && x.type == RoleSwaps.RoleType.RoleBuy).AsEnumerable();
                if (DBroles.Count() > 0)
                {
                    embed.WithAuthor($"🔨BuyRole - маркет ролей");
                    DBroles = DBroles.OrderBy(u => u.Price);
                    int i = 0;
                    foreach (var DBrole in DBroles)
                    {
                        i++;
                        embed.Description += $"{i}.<@&{DBrole.RoleId}> - {DBrole.Price} ZeroCoins\n";
                    }
                    var time = DateTime.Now.AddSeconds(61);
                    embed.WithFooter($"Осталось: {Math.Truncate((time - DateTime.Now).TotalSeconds)} секунд");
                    var mes = await Context.Channel.SendMessageAsync("", false, embed.Build());
                    var emotes = new[]
                    {
                        new Emoji("1️⃣"), new Emoji("2️⃣"),
                        new Emoji("3️⃣"), new Emoji("4️⃣"),
                        new Emoji("5️⃣"), new Emoji("6️⃣"),
                        new Emoji("7️⃣"), new Emoji("8️⃣"),
                        new Emoji("9️⃣"), new Emoji("🔟")
                    };
                    await mes.AddReactionsAsync(emotes.Take(DBroles.Count()).ToArray());

                    var check = new BuysRole() { userid = Context.User.Id, guildid = Context.Guild.Id, SelectItem = 0, MessageId = mes.Id };
                    BuyRole.Add(check);
                    while (time > DateTime.Now)
                    {
                        if ((time - DateTime.Now).Seconds % 10 == 0)
                            await mes.ModifyAsync(x => x.Embed = embed.WithFooter($"Осталось: {Math.Truncate((time - DateTime.Now).TotalSeconds)} секунд").Build());

                        var res = BuyRole.FirstOrDefault(x => x == check).SelectItem;
                        if (res != 0)
                        {
                            var DBrole = DBroles.ToList()[(int)(res - 1)];
                            var Role = Context.Guild.GetRole(DBrole.RoleId);

                            if (user.ZeroCoin >= DBrole.Price)
                            {
                                if (!(Context.User as SocketGuildUser).Roles.Contains(Role))
                                {
                                        user.ZeroCoin -= DBrole.Price;
                                        DBcontext.Users.Update(user);
                                        await DBcontext.SaveChangesAsync();
                                        await OtherSettings.CheckRoleValid(Context.User as SocketGuildUser, Role.Id, false);
                                        embed.WithDescription($"Вы успешно купили {Role.Mention} за {DBrole.Price} ZeroCoins");
                                }
                                else embed.WithDescription($"Вы уже купили роль {Role.Mention}");
                            }
                            else embed.WithDescription($"У вас недостаточно средств на счете!\nВаш баланс: {user.ZeroCoin} ZeroCoins");
                        }
                        if ((time - DateTime.Now).TotalSeconds < 2 || res != 0)
                        {
                            embed.Footer.Text = "";
                            await mes.ModifyAsync(x => x.Embed = embed.Build());
                            break;
                        }
                    }
                    await mes.RemoveAllReactionsAsync();
                    BuyRole.Remove(check);
                }
                else
                {

                    if (Context.Guild.Owner == Context.User)
                    {

                        embed.AddField("Добавить роль", $"{GuildPrefix}br.Add [ROLE] [PRICE]", true);
                        embed.AddField("Удалить роль", $"{GuildPrefix}br.Del [ROLE]", true);
                    }

                    await Context.Channel.SendMessageAsync("", false,
                    embed.WithAuthor("🔨BuyRole - Роли не выставлены на продажу ⚠️")
                         .WithDescription("Попросите создателя сервера выставить роли на продажу <3").Build());
                }

            }
        }
    }
}
