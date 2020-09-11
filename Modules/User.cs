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
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Image = SixLabors.ImageSharp.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static DarlingBotNet.Services.CommandHandler;
using System.Numerics;

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
                var usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                ulong count = (usr.Level * 80 * usr.Level);
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                                         .WithColor(255, 0, 94)
                                                         .WithAuthor($" - Level {user}", user.GetAvatarUrl())
                                                         .WithDescription($"Уровень: {usr.Level}\n" +
                                                         $"Опыт:{usr.XP - count}/{ (usr.Level + 1) * 80 * (usr.Level + 1) - count}")
                                                         .Build());
        }



        public static readonly List<Checking> list = new List<Checking>();

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task marry(SocketUser user)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Marry");
                if (Context.User != user)
                {
                    var marryuser = _cache.GetOrCreateUserCache(user.Id,(user as SocketGuildUser).Guild.Id);
                    var ContextUser = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                    if (ContextUser.marryedid != marryuser.userid)
                    {
                        var Prefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                        if (ContextUser.marryedid == 0)
                        {
                            if (marryuser.marryedid == 0)
                            {

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
                                            _cache.Update(ContextUser);
                                            _cache.Update(marryuser);
                                            DBcontext.Users.Update(ContextUser);
                                            DBcontext.Users.Update(marryuser);
                                            await DBcontext.SaveChangesAsync();
                                            emb.WithDescription($"Теперь вы женаты!");
                                        }
                                        else
                                            emb.WithDescription($"{user.Mention} отказался от свадьбы!");

                                        break;
                                    }
                                    else if ((time - DateTime.Now).Seconds < 2)
                                    {
                                        emb.WithDescription($"{user.Mention} не успел принять заявку!");
                                        break;
                                    }
                                }
                                await mes.RemoveAllReactionsAsync();
                                await mes.ModifyAsync(x => x.Embed = emb.Build());
                                list.Remove(check);
                            }
                            else await Context.Channel.SendMessageAsync("", false, emb.WithDescription($"{user} женат, нужно сначала развестись!").WithFooter($"Развестить - {Prefix}divorce").WithAuthor($"💞 - Ошибка").Build());
                        }
                        else await Context.Channel.SendMessageAsync("", false, emb.WithDescription("Вы уже женаты, сначала разведитесь!").WithFooter($"Развестить - {Prefix}divorce").WithAuthor($"💞 - Ошибка").Build());
                    }
                    else await Context.Channel.SendMessageAsync("", false, emb.WithDescription("Вы уже женаты на этом пользователе").WithAuthor($"💞 - Ошибка").Build());
                }
                else await Context.Channel.SendMessageAsync("", false, emb.WithDescription("Вы не можете жениться на себе!").WithAuthor($"💞 - Ошибка").Build());
            }
        }



        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task divorce()
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - divorce", Context.User.GetAvatarUrl());
            using (var DBcontext = new DBcontext())
            {
                Users ContextUser = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                if (ContextUser.marryedid == 0) emb.WithDescription($"Вы не женаты!");
                else
                {
                    var marryed = _cache.GetOrCreateUserCache(ContextUser.marryedid, Context.Guild.Id);
                    marryed.marryedid = 0;
                    ContextUser.marryedid = 0;
                    _cache.Update(ContextUser);
                    _cache.Update(marryed);
                    DBcontext.Users.Update(ContextUser);
                    DBcontext.Users.Update(marryed);
                    await DBcontext.SaveChangesAsync();
                    emb.WithDescription($"Вы успешно развелись с {Context.Guild.GetUser(marryed.userid).Mention}!");
                }
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
                var usr = _cache.GetOrCreateUserCache(user.Id,user.Guild.Id);
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                                         .WithColor(255, 0, 94)
                                                         .WithAuthor($" - ZeroCoin {user}", user.GetAvatarUrl())
                                                         .WithDescription($"zcoin: {usr.ZeroCoin}")
                                                         .Build());
            }

        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task kazino(string Fishka, string Stavka)
        {
            EmbedBuilder emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - Kazino", Context.User.GetAvatarUrl());
            using (var DBcontext = new DBcontext())
            {
                Users account = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);

                ulong Coins = 0;

                if (Stavka == "all")
                    Coins = account.ZeroCoin;
                else if (Stavka.Count(x => x >= 48 && x <= 57) >= 3 && Stavka.Count(x => x >= 48 && x <= 57) <= 4)
                    Coins = Convert.ToUInt64(String.Concat(Stavka.Where(x => x >= 48 && x <= 57)));

                if (Coins >= 100 && Coins <= 9999)
                {
                    if (account.ZeroCoin >= Coins)
                    {
                        if (Fishka.ToLower() == "black" || Fishka.ToLower() == "zero" || Fishka.ToLower() == "red")
                        {
                            int ches = new Pcg.PcgRandom().Next(11);
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
                            _cache.Update(account);
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
                    usr.ZeroCoin += amt;
                    usr.Daily = DateTime.Now.AddDays(1);
                    emb.WithDescription($"Получено: {amt} ZeroCoin's!\nStreak: {usr.Streak}");
                    _cache.Update(usr);
                    DBcontext.Users.Update(usr);
                    await DBcontext.SaveChangesAsync();
                }
                else
                {
                    if ((usr.Daily - DateTime.Now).TotalSeconds >= 3600)
                        emb.WithDescription($"Дождитесь {(usr.Daily - DateTime.Now).Hours} часов и {(usr.Daily - DateTime.Now).Minutes} минут чтобы получить Daily!");

                    if ((usr.Daily - DateTime.Now).TotalSeconds <= 3600)
                        emb.WithDescription($"Дождитесь {((usr.Daily - DateTime.Now).TotalSeconds > 60 ? $"{(usr.Daily - DateTime.Now).Minutes} минут и " : "")} {(usr.Daily - DateTime.Now).Seconds} секунд чтобы получить Daily!");
                }
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }


        //[Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        //public async Task addc()
        //{
        //    using (var x = new DBcontext())
        //    {
        //        var usr = await SystemLoading.UserCreate(Context.User.Id, Context.Guild.Id);
        //        usr.ZeroCoin++;
        //        x.Update(usr);
        //        await x.SaveChangesAsync();
        //        await Context.Channel.SendMessageAsync(usr.ZeroCoin.ToString());
        //    }
        //}


        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task transfer(SocketGuildUser user, ulong coin)
        {
            EmbedBuilder emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"{Context.User} 💱 {user}");
            using (var DBcontext = new DBcontext())
            {
                if (user != Context.User)
                {
                    Users usr = _cache.GetOrCreateUserCache(Context.User.Id,Context.Guild.Id);
                    if (usr.ZeroCoin >= coin)
                    {
                        if (coin <= 10000)
                        {
                            var transfuser = _cache.GetOrCreateUserCache(user.Id, Context.Guild.Id);
                            usr.ZeroCoin -= coin;
                            transfuser.ZeroCoin += coin;
                            emb.WithDescription($"Перевод в размере {coin} zerocoin успешно прошел.");
                            _cache.Update(usr);
                            _cache.Update(transfuser);
                            DBcontext.Users.Update(usr);
                            DBcontext.Users.Update(transfuser);
                            await DBcontext.SaveChangesAsync();
                        }
                        else emb.WithDescription($"Перевести больше 10000 zerocoin нельзя.");
                    }
                    else emb.WithDescription($"У вас недостаточно средств для перевода. Вам нехватает {coin - usr.ZeroCoin}");
                }
                else emb.WithDescription("Переводить деньги самому себе нельзя!");
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task myinvite()
        {
            EmbedBuilder emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Инвайты на {Context.Guild.Name} от {Context.User}", Context.Guild.IconUrl);
            foreach (var invite in Context.Guild.GetInvitesAsync().Result.Where(x => x.Inviter.Id == Context.User.Id))
            {
                emb.AddField($"ID: {invite.Id}", $"Использований: {invite.Uses}/{invite.MaxUses}", true);
            }
            if (emb.Fields.Count == 0) emb.WithDescription("Инвайты отсутствуют.");
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionViolation]
        public async Task warns()
        {
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
                var embed = new EmbedBuilder().WithAuthor("🔨LevelRole - уровневые роли отсутствуют ⚠️").WithColor(255, 0, 94);
                var lvl = DBcontext.LVLROLES.AsQueryable().Where(x => x.guildid == Context.Guild.Id);
                if (lvl.Count() != 0)
                {
                    lvl = lvl.OrderBy(u => (uint)u.countlvl);
                    embed.WithAuthor($"🔨LevelRole - уровневые роли");
                    foreach (var LVL in lvl)
                        embed.Description += $"{LVL.countlvl} уровень - {Context.Guild.GetRole(LVL.roleid).Mention}\n";
                }
                if (Context.Guild.Owner == Context.User)
                {
                    var glds = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                    embed.AddField("Добавить роль", $"{glds}lr.Add [ROLE] [LEVEL]");
                    embed.AddField("Удалить роль", $"{glds}lr.Del [ROLE]");
                }

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }
    }
}
