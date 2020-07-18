using DarlingBotNet.DataBase;
using DarlingBotNet.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DarlingBotNet.Modules
{
    public class User : ModuleBase<SocketCommandContext>
    {
        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task level(SocketUser user = null)
        {
            if (user == null) user = Context.User;
            await SystemLoading.CreateUser(user);
            var usr = new EEF<Users>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id && x.userid == user.Id);
            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                                     .WithColor(255, 0, 94)
                                                     .WithAuthor($" - Level {user}", user.GetAvatarUrl())
                                                     .WithDescription($"Уровень: {usr.Level}\n" +
                                                     $"Опыт:{usr.XP}/{ (usr.Level + 1) * 80 * (usr.Level + 1)}")
                                                     .Build());

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
            var emb = new EmbedBuilder().WithColor(255, 0, 94);
                if (Context.User != user)
                {
                    var ContextUser = new EEF<Users>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id && x.userid == Context.User.Id);
                    var marryuser = await SystemLoading.CreateUser(user as SocketGuildUser);
                    if (ContextUser.marryedid != marryuser.userid)
                    {
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
                                    new EEF<Users>(new DBcontext()).Update(ContextUser);
                                    new EEF<Users>(new DBcontext()).Update(marryuser);
                                    await Context.Channel.SendMessageAsync("", false, emb.WithAuthor($"{Context.User} 💞 {user}").WithDescription($"Теперь вы женаты!").Build());

                                }
                                else if (list.FirstOrDefault(x => x.marryedid == user.Id && x.messid == mes.Id).tar == 1 && timer != 0)
                                    await Context.Channel.SendMessageAsync("", false, emb.WithAuthor($"{Context.User} 💞 {user}").WithDescription($"{user.Mention} отказался от свадьбы!").Build());
                                else if (list.FirstOrDefault(x => x.marryedid == user.Id && x.messid == mes.Id).tar == 0 && timer == 0)
                                    await Context.Channel.SendMessageAsync("", false, emb.WithAuthor($"{Context.User} 💞 {user}").WithDescription($"{user.Mention} не успел принять заявку!").Build());
                                list.Remove(list.FirstOrDefault(x => x.marryedid == user.Id && x.messid == mes.Id));
                            }
                            else await Context.Channel.SendMessageAsync("", false, emb.WithDescription($"{user} женат, нужно сначала развестись!").WithFooter($"Развестить - {new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id).Prefix}divorce").WithAuthor($"💞 - Ошибка").Build());
                        }
                        else await Context.Channel.SendMessageAsync("", false, emb.WithDescription("Вы уже женаты, сначала разведитесь!").WithFooter($"Развестить - {new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id).Prefix}divorce").WithAuthor($"💞 - Ошибка").Build());
                    }
                    else await Context.Channel.SendMessageAsync("", false, emb.WithDescription("Вы уже женаты на этом пользователе").WithAuthor($"💞 - Ошибка").Build());
                }
                else await Context.Channel.SendMessageAsync("", false, emb.WithDescription("Вы не можете жениться на себе!").WithAuthor($"💞 - Ошибка").Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task divorce()
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - divorce", Context.User.GetAvatarUrl());
            Users ContextUser = new EEF<Users>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id && x.userid == Context.User.Id);
            if (ContextUser.marryedid == 0) emb.WithDescription($"Вы не женаты!");
            else
            {
                Users marryed = new EEF<Users>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id && x.userid == ContextUser.marryedid);
                marryed.marryedid = 0;
                ContextUser.marryedid = 0;
                new EEF<Users>(new DBcontext()).Update(marryed);
                new EEF<Users>(new DBcontext()).Update(ContextUser);
                emb.WithDescription($"Вы успешно развелись с {Context.Guild.GetUser(marryed.userid).Mention}!");
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task zcoin(SocketUser user = null)
        {
            if (user == null) user = Context.User;
            await SystemLoading.CreateUser(user);
            var usr = new EEF<Users>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id && x.userid == user.Id);
            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                                     .WithColor(255, 0, 94)
                                                     .WithAuthor($" - ZeroCoin {user}", user.GetAvatarUrl())
                                                     .WithDescription($"zcoin: {usr.ZeroCoin}")
                                                     .Build());

        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task kazino(string Fishka, string Stavka)
        {
            Users account = new EEF<Users>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id && x.userid == Context.User.Id);
            Guilds Guild = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id);
            EmbedBuilder emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - Kazino", Context.User.GetAvatarUrl());
            ulong Pz = 0;
            var ar = Stavka.ToCharArray().Where(x => x >= 48 && x <= 57);
            if (Stavka.ToLower() == "all")
                Pz = account.ZeroCoin;
            else if (ar.Count() >= 3 && ar.Count() < 5)
            {
                Pz = Convert.ToUInt32(Stavka);
                if (Fishka.ToLower() == "black" || Fishka.ToLower() == "zero")
                {
                    if (account.ZeroCoin >= Pz)
                    {
                        int ches = new Pcg.PcgRandom().Next(0, 2);

                        if(ches == 1)
                        {
                            if (Fishka.ToLower() == "zero")
                            {
                                account.ZeroCoin += Pz;
                                emb.WithAuthor(" - Kazino - ✔️ Win", Context.User.GetAvatarUrl())
                                   .WithDescription($"Выпало: Zero\nZeroCoin: {account.ZeroCoin}");
                            }
                            else
                            {
                                account.ZeroCoin -= Pz;
                                emb.WithAuthor(" - Kazino - ❌ Lose", Context.User.GetAvatarUrl())
                                   .WithDescription($"Выпало: Black\nZeroCoin: {account.ZeroCoin}");
                            }
                        }
                        else
                        {
                            if (Fishka.ToLower() == "black")
                            {
                                account.ZeroCoin += Pz;
                                emb.WithAuthor(" - Kazino - ✔️ Win", Context.User.GetAvatarUrl())
                                   .WithDescription($"Выпало: Black\nZeroCoin: {account.ZeroCoin}");
                            }
                            else
                            {
                                account.ZeroCoin -= Pz;
                                emb.WithAuthor(" - Kazino - ❌ Lose", Context.User.GetAvatarUrl())
                                   .WithDescription($"Выпало: Zero\nZeroCoin: {account.ZeroCoin}");
                            }
                        }
                        
                        new EEF<Users>(new DBcontext()).Update(account);
                    }
                    else emb.WithDescription($"{Context.User.Username} на вашем счете недостаточно средств.\nВаш баланс: {account.ZeroCoin} ZeroCoin");
                }
                else emb.WithDescription($"Вы должны написать на что ставить, на Black или Zero.").WithFooter($"Пример - {Guild.Prefix}kazino [black/zero] [ZeroCoin]");
            }
            else emb.WithDescription($"Ставка может быть только больше 99 и меньше {UInt64.MaxValue}, или же быть `all`").WithFooter("all - выставить все");


            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task daily()
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - daily 🏧", Context.User.GetAvatarUrl());
            var usr = new EEF<Users>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id && x.userid == Context.User.Id);

            if (DateTime.Now > usr.Daily)
            {
                if (Math.Round((DateTime.Now - usr.Daily).TotalDays, 1) == 0)
                    usr.Streak++;
                else
                    usr.Streak = 1;

                ulong amt = 500 + ((500 / 35) * usr.Streak);
                usr.ZeroCoin += amt;
                usr.Daily = DateTime.Now.AddDays(1);
                emb.WithDescription($"Получено: {amt} ZeroCoin's!\nStreak: {usr.Streak}");
                new EEF<Users>(new DBcontext()).Update(usr);
            }
            else
            {
                if ((usr.Daily - DateTime.Now).TotalSeconds >= 3600)
                    emb.WithDescription($"Дождитесь {(usr.Daily - DateTime.Now).Hours} часов и {(usr.Daily - DateTime.Now).Minutes} минут чтобы получить Daily!");

                if ((usr.Daily - DateTime.Now).TotalSeconds >= 60 && (usr.Daily - DateTime.Now).TotalSeconds <= 3600)
                    emb.WithDescription($"Дождитесь {(usr.Daily - DateTime.Now).Minutes} минут и {(usr.Daily - DateTime.Now).Seconds} секунд чтобы получить Daily!");

                if ((usr.Daily - DateTime.Now).TotalSeconds < 60)
                    emb.WithDescription($"Дождитесь {(usr.Daily - DateTime.Now).Seconds} секунд чтобы получить Daily!");
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task transfer(SocketGuildUser user, ulong coin)
        {

            EmbedBuilder emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"{Context.User} 💱 {user}");
            Users usr = new EEF<Users>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id && x.userid == Context.User.Id);
            if (user != Context.User)
            {
                if (usr.ZeroCoin >= coin)
                {
                    if (coin <= 10000)
                    {
                        await SystemLoading.CreateUser(user);
                        var transfuser = new EEF<Users>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id && x.userid == user.Id);
                        usr.ZeroCoin -= coin;
                        transfuser.ZeroCoin += coin;
                        emb.WithDescription($"Перевод в размере {coin} zerocoin успешно прошел.");
                        new EEF<Users>(new DBcontext()).Update(usr);
                        new EEF<Users>(new DBcontext()).Update(transfuser);
                    }
                    else emb.WithDescription($"Перевести больше 10000 zerocoin нельзя.");
                }
                else emb.WithDescription($"У вас недостаточно средств для перевода. Вам нехватает {coin - usr.ZeroCoin}");
            }
            else emb.WithDescription("Переводить деньги самому себе нельзя!");
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

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task clubinfo()
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - daily 🏧", Context.User.GetAvatarUrl());

            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }

    }
}
