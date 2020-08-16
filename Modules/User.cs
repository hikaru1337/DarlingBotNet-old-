using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DarlingBotNet.Modules
{
    public class User : ModuleBase<SocketCommandContext>
    {
        private readonly DbService _db;

        public User(DbService db)
        {
            _db = db;
        }


        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task level(SocketUser user = null)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                if (user == null) user = Context.User;
                var usr = DBcontext.Users.Get(user.Id, Context.Guild.Id);
                await DBcontext.SaveChangesAsync();
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                                         .WithColor(255, 0, 94)
                                                         .WithAuthor($" - Level {user}", user.GetAvatarUrl())
                                                         .WithDescription($"Уровень: {usr.Level}\n" +
                                                         $"Опыт:{usr.XP}/{ (usr.Level + 1) * 80 * (usr.Level + 1)}")
                                                         .Build());
            }

        }

        public class marryz
        {
            public ulong messid { get; set; }
            public ulong marryedid { get; set; }
            public ushort tar { get; set; }
        }

        public static readonly List<marryz> list = new List<marryz>();

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task marry(SocketUser user)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94);
                if (Context.User != user)
                {
                    var marryuser = DBcontext.Users.GetOrCreate(user as SocketGuildUser);
                    await DBcontext.SaveChangesAsync();
                    var ContextUser = DBcontext.Users.Get(Context.User.Id, Context.Guild.Id);
                    if (ContextUser.marryedid != marryuser.userid)
                    {
                        var Prefix = DBcontext.Guilds.Get(Context.Guild).Prefix;
                        if (ContextUser.marryedid == 0)
                        {
                            if (marryuser.marryedid == 0)
                            {
                                var mes = await Context.Channel.SendMessageAsync("", false, emb.WithAuthor($"{Context.User} 💞 {user}").WithDescription($"Заявка отправлена {user.Mention}\nПринять: :white_check_mark: Отклонить: :negative_squared_cross_mark: ").Build());
                                await mes.AddReactionAsync(new Emoji("✅"));
                                await mes.AddReactionAsync(new Emoji("❎"));
                                list.Add(new marryz() { marryedid = user.Id, messid = mes.Id });
                                uint timer = 60;
                                while (timer >= 0)
                                {
                                    await Task.Delay(5000);
                                    timer -= 10;
                                    if (list.FirstOrDefault(x => x.marryedid == user.Id && x.messid == mes.Id).tar != 0) break;
                                    if (timer == 0) break;
                                    await mes.ModifyAsync(x => x.Embed = emb.WithDescription($"Заявка отправлена {user.Mention}\nПринять: :white_check_mark: Отклонить: :negative_squared_cross_mark: \nОсталось: {timer} секунд").Build());
                                }
                                await mes.DeleteAsync();
                                if (list.FirstOrDefault(x => x.marryedid == user.Id && x.messid == mes.Id).tar == 2 && timer != 0)
                                {
                                    ContextUser.marryedid = marryuser.userid;
                                    marryuser.marryedid = ContextUser.userid;
                                    DBcontext.Users.Update(ContextUser);
                                    DBcontext.Users.Update(marryuser);
                                    await DBcontext.SaveChangesAsync();
                                    await Context.Channel.SendMessageAsync("", false, emb.WithAuthor($"{Context.User} 💞 {user}").WithDescription($"Теперь вы женаты!").Build());

                                }
                                else if (list.FirstOrDefault(x => x.marryedid == user.Id && x.messid == mes.Id).tar == 1 && timer != 0)
                                    await Context.Channel.SendMessageAsync("", false, emb.WithAuthor($"{Context.User} 💞 {user}").WithDescription($"{user.Mention} отказался от свадьбы!").Build());
                                else if (list.FirstOrDefault(x => x.marryedid == user.Id && x.messid == mes.Id).tar == 0 && timer == 0)
                                    await Context.Channel.SendMessageAsync("", false, emb.WithAuthor($"{Context.User} 💞 {user}").WithDescription($"{user.Mention} не успел принять заявку!").Build());
                                list.Remove(list.FirstOrDefault(x => x.marryedid == user.Id && x.messid == mes.Id));
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
            using (var DBcontext = _db.GetDbContext())
            {
                Users ContextUser = DBcontext.Users.Get(Context.User.Id, Context.Guild.Id);
                if (ContextUser.marryedid == 0) emb.WithDescription($"Вы не женаты!");
                else
                {
                    Users marryed = DBcontext.Users.GetOrCreate(ContextUser.marryedid, Context.Guild.Id);
                    marryed.marryedid = 0;
                    ContextUser.marryedid = 0;
                    DBcontext.Users.Update(marryed);
                    DBcontext.Users.Update(ContextUser);
                    await DBcontext.SaveChangesAsync();
                    emb.WithDescription($"Вы успешно развелись с {Context.Guild.GetUser(marryed.userid).Mention}!");
                }
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task zcoin(SocketUser user = null)
        {
            if (user == null) user = Context.User;
            using (var DBcontext = _db.GetDbContext())
            {
                var usr = DBcontext.Users.GetOrCreate(user.Id, Context.Guild.Id);
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
            using (var DBcontext = _db.GetDbContext())
            {
                Users account = DBcontext.Users.Get(Context.User.Id, Context.Guild.Id);

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
                            DBcontext.SaveChangesAsync();
                        }
                        else
                        {
                            var Guild = DBcontext.Guilds.Get(Context.Guild).Prefix;
                            emb.WithDescription("Ваша ставка должна быть black,zero или red").WithFooter($"Пример - {Guild}kz [black/zero/red] [Кол-во ZeroCoin's]");
                        }
                    }
                    else emb.WithDescription($"{Context.User.Mention} на вашем счете недостаточно средств.\nВаш баланс: {account.ZeroCoin} ZeroCoin");
                }
                else emb.WithDescription($"Ставка может быть только больше 99 и меньше 9999, или же быть `all`").WithFooter("all - выставить все");
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        //[Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        //public async Task addc()
        //{
        //    using (var DBcontext = _db.GetDbContext())
        //    {
        //        var usr = DBcontext.Users.GetOrCreate(Context.User.Id, Context.Guild.Id);
        //        usr.ZeroCoin++;
        //        DBcontext.Users.Update(usr);
        //        await DBcontext.SaveChangesAsync();
        //        await Context.Channel.SendMessageAsync(usr.ZeroCoin.ToString());
        //    }
        //}

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task daily()
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var usr = DBcontext.Users.Get(Context.User.Id, Context.Guild.Id);
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

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task transfer(SocketGuildUser user, ulong coin)
        {
            EmbedBuilder emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"{Context.User} 💱 {user}");
            using (var DBcontext = _db.GetDbContext())
            {
                var transfuser = DBcontext.Users.Get(user.Id, Context.Guild.Id);
                Users usr = DBcontext.Users.Get(Context.User.Id, Context.Guild.Id);
                if (user != Context.User)
                {
                    if (usr.ZeroCoin >= coin)
                    {
                        if (coin <= 10000)
                        {

                            usr.ZeroCoin -= coin;
                            transfuser.ZeroCoin += coin;
                            emb.WithDescription($"Перевод в размере {coin} zerocoin успешно прошел.");
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
            using (var DBcontext = _db.GetDbContext())
            {
                var Guild = DBcontext.Guilds.Get(Context.Guild);
                if (Guild.ViolationSystem == 0) emb.WithDescription("На сервере не включена система нарушений.");
                else if (Guild.ViolationSystem == 1) emb.WithDescription("На сервере выбрана другая система нарушений");
                else
                {
                    var warns = DBcontext.Warns.Get(Context.Guild.Id);
                    foreach (var warn in warns)
                    {
                        if (warn.ReportWarn.Contains("tban"))
                            warn.ReportWarn = $"Бан на {warn.ReportWarn.Substring(4, warn.ReportWarn.Length - 4)} минут";

                        else if (warn.ReportWarn.Contains("tmute"))
                            warn.ReportWarn = $"Мут на {warn.ReportWarn.Substring(5, warn.ReportWarn.Length - 5)} минут";

                        emb.Description += $"{warn.CountWarn}.{warn.ReportWarn}";
                        emb.Description += "\n";
                    }
                    if (emb.Description == null) emb.WithDescription("На сервере еще нет варнов!");
                }
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task levelrole()
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var glds = DBcontext.Guilds.Get(Context.Guild);
                var embed = new EmbedBuilder().WithAuthor("🔨LevelRole - уровневые роли отсутствуют ⚠️").WithColor(255, 0, 94);

                var lvl = DBcontext.LVLROLES.Get(Context.Guild);
                if (lvl.Count() != 0)
                {
                    embed.WithAuthor($"🔨LevelRole - уровневые роли");
                    lvl = lvl.OrderByDescending(u => u.countlvl);
                    foreach (var LVL in lvl)
                        embed.Description += $"{LVL.countlvl} уровень - {Context.Guild.GetRole(LVL.roleid).Mention}\n";
                }
                if (Context.Guild.Owner == Context.User)
                {
                    embed.AddField("Добавить роль", $"{glds.Prefix}lr.Add [ROLE] [LEVEL]");
                    embed.AddField("Удалить роль", $"{glds.Prefix}lr.Del [ROLE]");
                }

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }
    }
}
