using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using Image = SixLabors.ImageSharp.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static DarlingBotNet.Services.CommandHandler;
using System.Numerics;
using Pcg;
using ServiceStack;

namespace DarlingBotNet.Modules
{
    public class User : ModuleBase<SocketCommandContext>
    {
        private readonly IMemoryCache _cache;


        public User(IMemoryCache cache)
        {
            _cache = cache;
        }


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
            if (user == null) user = Context.User;
            var usr = _cache.GetOrCreateUserCache(user.Id, Context.Guild.Id);
            ulong count = (usr.Level * 80 * usr.Level);
            _cache.Removes(Context);
            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                                     .WithColor(255, 0, 94)
                                                     .WithAuthor($" - Level {user}", user.GetAvatarUrl())
                                                     .WithDescription($"Уровень: {usr.Level}\n" +
                                                     $"Опыт:{usr.XP - count}/{ (usr.Level + 1) * 80 * (usr.Level + 1) - count}")
                                                     .Build());
        }


        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task usertop()
        {
            _cache.Removes(Context);
            using (var DBcontext = new DBcontext())
            {
                var UsersTop = DBcontext.Users.AsQueryable().Where(x => x.guildId == Context.Guild.Id && !x.Leaved).OrderByDescending(x => (double)x.XP).Take(10);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - TOP 10 SERVER USERS", Context.Guild.IconUrl);
                int count = 0;
                foreach (var usr in UsersTop)
                {
                    count++;
                    var DiscordUser = Context.Guild.GetUser(usr.userid);
                    string text = "";
                    if (count == 1) text = "<a:1place:755825376550322337> ";
                    else if (count == 2) text = "<a:2place:755825717429796925> ";
                    else if (Context.Guild.Owner == DiscordUser) text = "<:ServerAdmin:755825374818074725> ";

                    text += $"{DiscordUser} - {(DateTime.Now - DiscordUser.JoinedAt).Value.Days} дней";
                    emb.AddField(text, $"LVL: {usr.Level} Money: {usr.ZeroCoin}");
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
                if (Context.User != user)
                {
                    var marryuser = DBcontext.Users.FirstOrDefault(x => x.userid == user.Id && x.guildId == (user as SocketGuildUser).Guild.Id);
                    var ContextUser = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                    if (ContextUser.marryedid != marryuser.userid)
                    {
                        var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                        _cache.Removes(Context);
                        if (ContextUser.marryedid == 0)
                        {
                            if (marryuser.marryedid == 0)
                            {
                                emb.WithAuthor("Marry");
                                var time = DateTime.Now.AddSeconds(30);
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
                                            ContextUser.marryedid = marryuser.userid;
                                            marryuser.marryedid = ContextUser.userid;
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
                            else await Context.Channel.SendMessageAsync("", false, emb.WithDescription($"{user} женат(а), нужно сначала развестись!").WithFooter($"Развестить - {GuildPrefix}divorce").Build());
                        }
                        else await Context.Channel.SendMessageAsync("", false, emb.WithDescription("Вы уже женаты, сначала разведитесь!").WithFooter($"Развестить - {GuildPrefix}divorce").Build());
                    }
                    else await Context.Channel.SendMessageAsync("", false, emb.WithDescription("Вы уже женаты на этом пользователе").Build());
                }
                else await Context.Channel.SendMessageAsync("", false, emb.WithDescription("Вы не можете жениться на себе!").Build());
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
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - Roulette", Context.Guild.IconUrl);
            using (var DBcontext = new DBcontext())
            {
                var DBuser = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                _cache.Removes(Context);
                if (DBuser.ZeroCoin > ZeroCoins)
                {
                    if (ZeroCoins > 99)
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

                                DBuser = DBcontext.Users.FirstOrDefault(x => x.userid == WinnerUserid && x.guildId == WinnderGuildid);
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
                    await Context.Channel.SendMessageAsync("", false, emb.WithDescription($"У вас недостаточно средств для ставки.\nВам нехватает: {ZeroCoins - DBuser.ZeroCoin}").Build());
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
                    emb.WithDescription($"Вы успешно развелись с <@{marryed.userid}>!");
                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        //[Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        //public async Task demotiv(string url,[Remainder]string text)
        //{
        //    var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - Демотиватор", Context.User.GetAvatarUrl());
        //    var urlPhoto = Image.Load(new WebClient().DownloadData(url));
        //    if (urlPhoto != null)
        //    {
        //        using (var img = new Image<Rgba32>((urlPhoto.Width + (int)(urlPhoto.Width*0.2)), (urlPhoto.Height + (int)(urlPhoto.Height * 0.8))))
        //        {
        //            FontFamily GothamSSm = new FontCollection().Install("UserProfile/Akrobat-Regular.ttf");
        //            var Font = GothamSSm.CreateFont(50);
        //            img.Mutate(x => x.BackgroundColor(new Rgba32(10, 10, 10)).DrawLines(new Rgba32(255, 255, 255), 1, new PointF[] { new PointF(10, 10), new PointF(10, 20) })
        //                             .DrawImage(urlPhoto, 1 , new Point( (int)(urlPhoto.Width * 0.2) / 2, (int)((urlPhoto.Height * 0.8) / 3)))
        //                             .DrawText(new TextGraphicsOptions(true)
        //                                              {
        //                                                  HorizontalAlignment = HorizontalAlignment.Center,
        //                                 WrapTextWidth = img.Width
        //                             }, text, Font, new Rgba32(255, 255, 255), new Vector2(0, (int)(img.Height * 0.8) + 50))


        //                             );
        //            await Context.Channel.SendFileAsync(img.ToStream(), "gg.png");
        //        }
        //    }
        //    else
        //    {
        //        emb.WithDescription("Ссылка не имеет вложенное изображение!");
        //        await Context.Channel.SendMessageAsync("",false,emb.Build());
        //    }
        //}

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task zcoin(SocketGuildUser user = null)
        {
            if (user == null) user = Context.User as SocketGuildUser;
            using (var DBcontext = new DBcontext())
            {
                ulong UserZeroCoins = _cache.GetOrCreateUserCache(user.Id, user.Guild.Id).ZeroCoin;
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                                         .WithColor(255, 0, 94)
                                                         .WithAuthor($" - ZeroCoin {user}", user.GetAvatarUrl())
                                                         .WithDescription($"zcoin: {UserZeroCoins}")
                                                         .Build());
            }

        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task kazino(string Fishka, string Stavka)
        {
            EmbedBuilder emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - Kazino", Context.User.GetAvatarUrl());
            using (var DBcontext = new DBcontext())
            {
                var account = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);

                ulong Coins = 0;

                if (Stavka == "all")
                    Coins = account.ZeroCoin;
                else if (Stavka.Count(x => x >= 48 && x <= 57) >= 3 && Stavka.Count(x => x >= 48 && x <= 57) <= 4)
                    Coins = Convert.ToUInt32(String.Concat(Stavka.Where(x => x >= 48 && x <= 57)));

                if (Coins >= 100 && Coins <= 20000)
                {
                    if (account.ZeroCoin >= Coins)
                    {
                        if (Fishka.ToLower() == "black" || Fishka.ToLower() == "zero" || Fishka.ToLower() == "red")
                        {
                            int ches = new PcgRandom().Next(11);
                            emb.WithAuthor(" - Kazino - ✔️ Win", Context.User.GetAvatarUrl());
                            if (ches % 2 == 1 && Fishka.ToLower() == "black" || ches != 10 && ches % 2 == 0 && Fishka.ToLower() == "red")
                                account.ZeroCoin += Coins;
                            else if (ches == 10 && Fishka.ToLower() == "zero")
                                account.ZeroCoin += Coins * 2;
                            else
                            {
                                account.ZeroCoin -= Coins;
                                emb.WithAuthor(" - Kazino - ❌ Lose", Context.User.GetAvatarUrl());
                            }
                            emb.WithDescription($"Выпало: {(ches % 2 == 1 ? "black" : (ches != 10 && ches % 2 == 0) ? "red" : "zero")}\nZeroCoin: {account.ZeroCoin}");

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
                        else
                        {
                            var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                            emb.WithDescription("Ваша ставка должна быть black,zero или red").WithFooter($"Пример - {Guild}kz [black/zero/red] [Кол-во ZeroCoin's]");
                        }
                    }
                    else emb.WithDescription($"{Context.User.Mention} на вашем счете недостаточно средств.\nВаш баланс: {account.ZeroCoin} ZeroCoin");
                }
                else emb.WithDescription($"Ставка может быть только больше 99 и меньше 9999, или же быть `all`").WithFooter("all - выставить все");
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
                    if (usr.clanId != 0 && usr.clanInfo != Users.UserClanRole.ready)
                    {
                        var clan = DBcontext.Clans.FirstOrDefault(x => x.guildId == Context.Guild.Id && x.Id == usr.clanId);
                        if(clan.ClanMoney < -50000)
                        {
                            var ClanDaily = (long)(amt * 0.15);
                            clan.ClanMoney += ClanDaily;
                            emb.Description += $"Клан получил {ClanDaily} zcoin, от daily!";
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
        public async Task profile(SocketGuildUser user = null)
        {
            if (user == null) user = Context.User as SocketGuildUser;
            using (var DBcontext = new DBcontext())
            {
                var usr = _cache.GetOrCreateUserCache(user.Id, Context.Guild.Id);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - profile {user} 📟", user.GetAvatarUrl()).WithThumbnailUrl(user.GetAvatarUrl());

                if (usr.clanId != 0 && usr.clanInfo != Users.UserClanRole.ready)
                {
                    var UserClan = DBcontext.Clans.FirstOrDefault(x => x.Id == usr.clanId);
                    emb.AddField($"Клан {UserClan.ClanName}", $"Участников: {UserClan.DefUsers.Count()}", true);
                }
                if (usr.ClanOwner != 0)
                {
                    var UserOwnerClan = DBcontext.Clans.FirstOrDefault(x => x.Id == usr.ClanOwner);
                    emb.AddField($"Создатель клана: {UserOwnerClan.ClanName}", $"Участников: {UserOwnerClan.DefUsers.Count()}", true);
                }
                if (usr.marryedid != 0)
                {
                    emb.Description += $"Женат(а) на <@{usr.marryedid}>\n";
                }
                var WarnsCount = DBcontext.Warns.Count(x => x.guildid == Context.Guild.Id);
                emb.Description += $"ZeroCoin's: {usr.ZeroCoin}\nБанк: {usr.Bank}\nLevel: {usr.Level}\nWarns: {usr.countwarns}/{WarnsCount}";

                var TimeToDaily = (usr.Daily - DateTime.Now);

                if (TimeToDaily.Seconds > 0)
                    emb.WithFooter($"До Daily - {TimeToDaily.Hours}:{TimeToDaily.Minutes}:{TimeToDaily.Seconds}");
                else
                    emb.WithFooter($"До сброса Daily - {24 + TimeToDaily.Hours}:{60 + TimeToDaily.Minutes}:{ 60 + TimeToDaily.Seconds}");

                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        private static ulong limituser = 100000;

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Bank()
        {
            using (var DBcontext = new DBcontext())
            {
                var usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - Bank 🏧", Context.User.GetAvatarUrl());
                var CoinsDay = Math.Truncate(usr.Bank * (1 + 4.5 / 100) - usr.Bank);
                if (usr.Bank < limituser)
                {
                    if (DateTime.Now > usr.BankLastTransit)
                    {
                        usr.BankLastTransit = DateTime.Now.AddDays(7);
                        usr.Bank += (ulong)CoinsDay;
                        DBcontext.Users.Update(usr);
                        await DBcontext.SaveChangesAsync();
                    }
                }

                if (usr.Bank > limituser) 
                    usr.Bank = limituser;

                emb.WithDescription($"Заложено: {(usr.Bank < limituser ? usr.Bank.ToString() : $"{usr.Bank} limit!")}\nКол-во Coin в день: {CoinsDay}");
                emb.WithFooter("Можно снять/положить: ");
                var TimeToTransit = usr.BankTimer - DateTime.Now;
                if (TimeToTransit.TotalSeconds < 604800)
                {
                    if(TimeToTransit.TotalDays > 1)
                        emb.Footer.Text += $"через {TimeToTransit.Days} дней {TimeToTransit.Hours} часов и {TimeToTransit.Minutes} минут!";
                    else if (TimeToTransit.TotalSeconds >= 3600)
                        emb.Footer.Text += $"через {TimeToTransit.Hours} часов и {TimeToTransit.Minutes} минут!";
                    else
                        emb.Footer.Text += $"через {(TimeToTransit.TotalSeconds > 60 ? $"{TimeToTransit.Minutes} минут и " : "")} {TimeToTransit.Seconds} секунд!";
                }
                else
                {
                    var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                    emb.AddField("Пополнить счёт", $"{GuildPrefix}BankAdd [coins]",true);
                    emb.AddField("Снять со счёта", $"{GuildPrefix}BankGive [coins]", true);
                    emb.Description += "**сейчас**";
                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task BankAdd(ulong coins)
        {
            using (var DBcontext = new DBcontext())
            {
                var usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
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
        public async Task BankGive(ulong coins)
        {
            using (var DBcontext = new DBcontext())
            {
                var usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - BankAdd 🏧", Context.User.GetAvatarUrl());
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
        public async Task myinvite()
        {
            _cache.Removes(Context);
            EmbedBuilder emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Инвайты на {Context.Guild.Name} от {Context.User}", Context.Guild.IconUrl);
            foreach (var invite in Context.Guild.GetInvitesAsync().Result.Where(x => x.Inviter.Id == Context.User.Id))
            {
                emb.AddField($"ID: {invite.Id}", $"Использований: {invite.Uses}/{invite.MaxUses}", true);
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
                var warns = DBcontext.Warns.AsQueryable().Where(x => x.guildid == Context.Guild.Id);
                foreach (var warn in warns)
                {
                    if (warn.ReportWarn.Contains("tban"))
                        warn.ReportWarn = $"Бан на {warn.ReportWarn.Substring(4, warn.ReportWarn.Length - 4)} минут";

                    else if (warn.ReportWarn.Contains("tmute"))
                        warn.ReportWarn = $"Мут на {warn.ReportWarn.Substring(5, warn.ReportWarn.Length - 5)} минут";

                    emb.Description += $"{warn.CountWarn}.{warn.ReportWarn}\n";
                }
                if (emb.Description == null) emb.WithDescription("На сервере еще нет варнов!");
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task levelrole()
        {
            using (var DBcontext = new DBcontext())
            {
                var lvl = DBcontext.LVLROLES.AsQueryable().Where(x => x.guildid == Context.Guild.Id).OrderBy(u => (uint)u.countlvl);
                var embed = new EmbedBuilder().WithAuthor($"🔨LevelRole - уровневые роли {(lvl.Count() != 0 ? "": "отсутствуют ⚠️")}")
                                              .WithColor(255, 0, 94);
                foreach (var LVL in lvl)
                        embed.Description += $"{LVL.countlvl} уровень - <@&{LVL.roleid}>\n";
                
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
    }
}
