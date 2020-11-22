using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using YamlDotNet.Serialization.ObjectFactories;
using static DarlingBotNet.Services.CommandHandler;

namespace DarlingBotNet.Modules
{
    public class Clan : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _provider;
        private readonly IMemoryCache _cache;
        public Clan(DiscordSocketClient discord, IServiceProvider provider, IMemoryCache cache)
        {
            _provider = provider;
            _discord = discord;
            _cache = cache;
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand, PermissionServerOwner]
        public async Task clanssettings(ulong count = 0, uint select = 0)
        {
            using (var DBcontext = new DBcontext())
            {
                var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Settings");
                if (count == 0 && select == 0)
                {

                    emb.AddField("1.Возможность покупки клановой роли", Guild.GiveClanRoles ? "Вкл" : "Выкл", true).WithFooter($"Вкл/Выкл - {Guild.Prefix}clanssettings 1");
                    if (Guild.GiveClanRoles)
                    {
                        emb.WithFooter($"Выбор - {Guild.Prefix}clanssettings [1/2/3] [null/count/count]");
                        emb.AddField("2.Стоимость покупки роли", Guild.PriceBuyRole + " ZeroCoins", true);
                        emb.AddField("3.Количество участников, для покупки роли", Guild.LimitRoleUserClan + " users", true);
                    }
                }
                else
                {
                    switch (count)
                    {
                        case 1:
                            emb.WithDescription($"Теперь клан {(Guild.GiveClanRoles ? "не " : "")} может купить клановую роль!");
                            Guild.GiveClanRoles = !Guild.GiveClanRoles;
                            break;
                        case 2:
                            emb.WithDescription($"Теперь личную роль можно купить за {select} ZeroCoin's");
                            Guild.PriceBuyRole = select;
                            break;
                        case 3:
                            emb.WithDescription($"Клановую роль можно купить только при {select} участниках в клане!");
                            Guild.LimitRoleUserClan = select;
                            break;
                        default:
                            emb.WithDescription("Выбор может быть от 1 до 3!");
                            break;
                    }
                    DBcontext.Guilds.Update(Guild);
                    await DBcontext.SaveChangesAsync();
                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan, PermissionClanMoneyMinus]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task clanshop(ulong count = 0)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Shop");
                var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id);
                var Clan = DBcontext.Clans.FirstOrDefault(x => x.OwnerId == Context.User.Id);
                Clan = await ClanPay(Clan);
                uint oneprice = 2500;
                uint twoprice = 3500;
                uint threeprice = 4500;
                if (count == 0)
                {
                    emb.WithFooter($"Купить - {Guild.Prefix}clanshop [number]");
                    emb.AddField("1.Аренда 5 слотов", $"Цена: {oneprice}", true);
                    emb.AddField("2.Аренда 10 слотов", $"Цена: {twoprice}/30% скидка - при [DarlingBoost](https://docs.darlingbot.ru/commands/darling-boost)", true);
                    emb.AddField("3.Аренда 15 слотов", $"Цена: {threeprice}/40% скидка  - при [DarlingBoost](https://docs.darlingbot.ru/commands/darling-boost)", true);
                    emb.AddField("4.Клановая роль", !Guild.GiveClanRoles ? "Отключена" : Guild.LimitRoleUserClan > (uint)Clan.DefUsers.Count() ? $"Для покупки вам нехватает {Guild.LimitRoleUserClan - (uint)Clan.DefUsers.Count()} участников!" : $"Цена: {Guild.PriceBuyRole}", true);
                }
                else
                {
                    var UserBoostEnds = DBcontext.DarlingBoost.FirstOrDefault(x=>x.UserId == Context.User.Id).Ends;
                    if(UserBoostEnds == null && UserBoostEnds < DateTime.Now)
                    {
                        twoprice = oneprice * 2;
                        threeprice = oneprice * 3;
                    }


                    switch (count)
                    {
                        case 1:
                            if (oneprice > Clan.ClanMoney)
                                emb.WithDescription($"Для покупки, вам нехватает {oneprice - Clan.ClanMoney} ZeroCoin's");
                            else
                            {
                                Clan.ClanMoney -= oneprice;
                                Clan.ClanSlots += 5;
                                emb.WithDescription("Вы арендовали 5 слотов для вашего клана!");
                            }
                            break;
                        case 2:
                            if (twoprice > Clan.ClanMoney)
                                emb.WithDescription($"Для покупки, вам нехватает {twoprice - Clan.ClanMoney} ZeroCoin's");
                            else
                            {
                                Clan.ClanMoney -= twoprice;
                                Clan.ClanSlots += 10;
                                emb.WithDescription("Вы арендовали 10 слотов для вашего клана!");
                            }
                            break;
                        case 3:
                            if (threeprice > Clan.ClanMoney)
                                emb.WithDescription($"Для покупки, вам нехватает {threeprice - Clan.ClanMoney} ZeroCoin's");
                            else
                            {
                                Clan.ClanMoney -= threeprice;
                                Clan.ClanSlots += 15;
                                emb.WithDescription("Вы арендовали 15 слотов для вашего клана!");
                            }
                            break;
                        case 4:
                            if (Guild.GiveClanRoles)
                            {
                                var ClanRoleDiscord = Context.Guild.GetRole(Clan.ClanRole);
                                if (ClanRoleDiscord != null)
                                {
                                    if (Guild.LimitRoleUserClan > (uint)Clan.DefUsers.Count())
                                        emb.WithDescription($"Для покупки вам нехватает {Guild.LimitRoleUserClan - (uint)Clan.DefUsers.Count()} участников!");
                                    else
                                    {

                                        if ((long)Guild.PriceBuyRole > Clan.ClanMoney)
                                            emb.WithDescription($"Для покупки роли вам нехватает {(long)Guild.PriceBuyRole - Clan.ClanMoney} Coin клана");
                                        else
                                        {
                                            Clan.ClanMoney -= (long)Guild.PriceBuyRole;
                                            var clanrole = await Context.Guild.CreateRoleAsync($"Clan: {Clan.ClanName}", new GuildPermissions(), Discord.Color.Gold, false, false);
                                            await clanrole.ModifyAsync(x => x.Position = (Context.Guild.EveryoneRole.Position + 1));
                                            Clan.ClanRole = clanrole.Id;

                                            await OtherSettings.CheckRoleValid(Context.User as SocketGuildUser, clanrole.Id, false);

                                            foreach (var User in Clan.DefUsers.Where(x => !x.Leaved))
                                            {
                                                var DiscordUser = Context.Guild.GetUser(User.UserId);
                                                await OtherSettings.CheckRoleValid(DiscordUser, clanrole.Id, false);
                                            }
                                            emb.WithDescription($"Вы успешно купили клановую роль - {clanrole.Mention}");
                                        }
                                    }
                                }
                                else
                                    emb.WithDescription($"У вас уже куплена клановая роль - {ClanRoleDiscord.Mention}");
                            }
                            else emb.WithDescription("Покупка роли отключена на сервере.").WithFooter($"Попросите создателя настроить клановую роль {Guild.Prefix}clansettings");
                            break;
                    }
                    DBcontext.Clans.Update(Clan);
                    await DBcontext.SaveChangesAsync();
                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }
        public static async Task<Clans> ClanPay(Clans Clan)
        {
            using (var DBcontext = new DBcontext())
            {
                if (DateTime.Now > Clan.LastClanSlotPays)
                {
                    if (Clan.DefUsers.Count() > 1)
                    {
                        if (Clan.ClanMoney > -50000)
                        {
                            Clan.LastClanSlotPays = DateTime.Now.AddDays(1);
                            Clan.ClanMoney -= (long)((Clan.DefUsers.Count() * 100) * (DateTime.Now - Clan.LastClanSlotPays).TotalDays);
                            DBcontext.Clans.Update(Clan);
                            await DBcontext.SaveChangesAsync();
                        }
                    }
                }
                return Clan;
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan]
        public async Task clancreate(string name, string logourl)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Create");
                var clan = DBcontext.Clans.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
                if (clan == null)
                {
                    uint priceCreate = 100000;
                    var usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                    if (usr.ZeroCoin >= priceCreate)
                    {
                        if (name.Length >= 4 && name.Length <= 32)
                        {
                                var x = DBcontext.Clans.Add(new Clans() { ClanName = name, LogoUrl = logourl, OwnerId = Context.User.Id, GuildId = Context.Guild.Id, ClanSlots = 5 }).Entity;
                                usr.ZeroCoin -= priceCreate;
                                usr.clanInfo = Users.UserClanRole.owner;
                                usr.ClanId = x.Id;
                                DBcontext.Users.Update(usr);
                                await DBcontext.SaveChangesAsync();
                                emb.WithDescription("Вы успешно создали свой клан. Веселитесь 🤟");
                            
                        }
                        else emb.WithDescription("Название должно быть больше 4 и меньше 32 символов");
                    }
                    else emb.WithDescription($"У вас недостаточно средств для создания. Вам нехватает {priceCreate - usr.ZeroCoin} ZeroCoin's");
                }
                else emb.WithDescription("У вас уже есть свой клан.").WithFooter("Для того чтобы создать новый, вам нужно удалить или передать старый клан");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());

            }
        }

        public static readonly List<Checking> slide = new List<Checking>();


        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan]
        public async Task clantransaction(uint money)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder();
                var usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                var Clan = DBcontext.Clans.AsQueryable().FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
                if (Clan == null)
                    Clan = DBcontext.Clans.AsQueryable().FirstOrDefault(x => x.OwnerId == usr.UserId);

                if (usr.ZeroCoin >= money)
                {
                    Clan = await ClanPay(Clan);
                    usr.ZeroCoin -= money;
                    Clan.ClanMoney += money;
                    emb.WithDescription($"Перевод в размере {money} zerocoin успешно прошел.");
                    DBcontext.Users.Update(usr);
                    DBcontext.Clans.Update(Clan);
                    await DBcontext.SaveChangesAsync();
                }
                else
                    emb.WithDescription($"На вашем балансе недостаточно средств\nZeroCoins: {usr.ZeroCoin}");

                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.WithColor(255, 0, 94).WithAuthor("Clans Transaction").Build());
            }
        }



        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task clans(uint clanid = 0)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clans List");
                if (clanid == 0)
                {
                    var clans = DBcontext.Clans.AsQueryable().Where(x => x.GuildId == Context.Guild.Id).AsEnumerable().OrderBy(x => x.DefUsers.Count()).ThenBy(x => x.Id);
                    if (clans.Count() != 0)
                    {
                        int page = 0;
                        await ListBuilder(page, clans, emb, "clans");
                    }
                    else
                    {
                        emb.WithDescription("Пока еще нет ни одного клана!");
                        await Context.Channel.SendMessageAsync("", false, emb.Build());
                    }

                }
                else
                {
                    var thisclan = DBcontext.Clans.FirstOrDefault(x => x.Id == clanid);
                    if (thisclan != null)
                    {
                        thisclan = await ClanPay(thisclan);
                        var usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                        if (usr.clanInfo != Users.UserClanRole.owner)
                        {
                            if (usr.ClanId == (uint)thisclan.Id)
                            {
                                switch (usr.clanInfo)
                                {
                                    case Users.UserClanRole.user:
                                        emb.WithDescription("Вы уже состоите в этом клане");
                                        break;
                                    case Users.UserClanRole.moder:
                                        emb.WithDescription("Вы уже являетесь модератором данного клана");
                                        break;
                                    case Users.UserClanRole.wait:
                                        emb.WithDescription("Вы уже подали заявку в клан. Ожидайте!");
                                        break;
                                }
                            }
                            else
                            {
                                if (usr.clanInfo == Users.UserClanRole.wait || usr.clanInfo == Users.UserClanRole.ready)
                                {
                                    usr.ClanId = (uint)thisclan.Id;
                                    usr.clanInfo = Users.UserClanRole.wait;
                                    DBcontext.Users.Update(usr);
                                    await DBcontext.SaveChangesAsync();
                                    emb.WithDescription("Вы успешно подали заявку. Ожидайте пока ее примут или отклонят.");
                                }
                                else emb.WithDescription("Для того чтобы подать заявку, выйдите из прошлого клана");
                            }
                        }
                        else
                            emb.WithDescription("Вы не можете подавать заявку в клан, имея свой клан!");

                    }
                    else emb.WithDescription("Клана с таким id не существует.");

                    await Context.Channel.SendMessageAsync("", false, emb.Build());
                }
                _cache.Removes(Context);
            }
        }

        private async Task ListBuilder(int page, IEnumerable<object> item, EmbedBuilder emb, string CommandName)
        {
            int countslots = 10;
            var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
            _cache.Removes(Context);
            emb = Lists(page, item.Skip(page * countslots).Take(countslots), GuildPrefix, CommandName, item.Count(), emb);
            var mes = await Context.Channel.SendMessageAsync("", false, emb.Build());
            if (item.Count() > countslots)
            {
                var check = new Checking() { userid = Context.User.Id, messid = mes.Id };
                IEmote emjto = new Emoji("▶️");
                IEmote emjot = new Emoji("◀️");
                var time = DateTime.Now.AddSeconds(30);
                slide.Add(check);
                await mes.AddReactionAsync(emjto);
                while (time > DateTime.Now)
                {
                    var res = slide.FirstOrDefault(x => x == check);
                    if (res.clicked != 0)
                    {
                        if (res.clicked == 2) page--;
                        else page++;

                        emb = Lists(page, item.Skip(page * countslots).Take(countslots), GuildPrefix, CommandName, item.Count(), emb);
                        await mes.ModifyAsync(x => x.Embed = emb.Build());
                        slide.FirstOrDefault(x => x == check).clicked = 0;

                        if (page > 1)
                            await mes.RemoveReactionAsync(emjot, Context.User);
                        if (page < Math.Ceiling(Convert.ToDouble(item.Count()) / 10))
                            await mes.RemoveReactionAsync(emjto, Context.User);

                        time = DateTime.Now.AddSeconds(30);
                    }
                }
                slide.Remove(check);
                await mes.RemoveAllReactionsAsync();
            }
        }


        private EmbedBuilder Lists(int page, IEnumerable<object> items, string GuildPrefix, string CommandName, int CountItems, EmbedBuilder emb)
        {
            int countslot = 10;

            emb.WithFooter($"Страница {(page == 0 ? 1 : page)}/{Math.Ceiling(Convert.ToDouble(CountItems) / countslot)}");

            if (CommandName == "clans"/* || CommandName == "clanstop"*/)
            {
                if (CommandName == "clans")
                    emb.Footer.Text += $" - {GuildPrefix}Clans [clanid]";

                foreach (var Clan in items.OfType<Clans>())
                {                                               /*{(Clan.DefUsers.Count() != 0 ? "" : $" - TOP {CountElement}")}*/
                    emb.AddField($"{Clan.Id}.{Clan.ClanName} ", $"Участников - {Clan.DefUsers.Count()}");
                }
            }
            else
            {
                if(CommandName == "clanclaims")
                    emb.Footer.Text += $" - {GuildPrefix}ClanClaims [clanname] [user] [accept/denied]";

                foreach (var User in items.OfType<Users>())
                {
                    if (Context.Guild.GetUser(User.UserId) != null)
                        emb.AddField($"{Context.Guild.GetUser(User.UserId)}", $"Уровень: {User.Level}");
                }
            }
            return emb;
        }


        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan]
        public async Task clandelete()
        {
            _cache.Removes(Context);
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Delete");
                var clan = DBcontext.Clans.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
                var usrs = DBcontext.Users.AsQueryable().Where(x => x.GuildId == Context.Guild.Id && x.ClanId == clan.Id);
                var DiscordRole = Context.Guild.GetRole(clan.ClanRole);
                if (DiscordRole != null)
                    await DiscordRole.DeleteAsync();

                foreach (var usr in usrs)
                {
                    usr.ClanId = 0;
                    usr.clanInfo = Users.UserClanRole.ready;
                }
                emb.WithDescription("Вы успешно удалили свой клан!");

                DBcontext.Users.UpdateRange(usrs);
                DBcontext.Clans.Remove(clan);
                await DBcontext.SaveChangesAsync();
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task clanusers(uint clanid = 0)
        {
            using (var DBcontext = new DBcontext())
            {
                _cache.Removes(Context);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Users");
                int page = 0;
                Clans clan = null;
                if(clanid != 0)
                    clan = DBcontext.Clans.AsQueryable().FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.Id == clanid);
                else
                {
                    clan = DBcontext.Clans.AsQueryable().Where(x => x.GuildId == Context.Guild.Id).AsEnumerable().FirstOrDefault(x=> x.OwnerId == Context.User.Id || x.UsersModerators.Count(x=>x.UserId == Context.User.Id) > 0);
                }

                if (clan != null)
                {
                    if (clan.DefUsers.Count() > 0)
                        await ListBuilder(page, clan.DefUsers, emb, "clanusers");
                    else
                        await Context.Channel.SendMessageAsync("", false, emb.WithDescription("В клане еще нет пользователей!").Build());
                }
                else
                    await Context.Channel.SendMessageAsync("", false, emb.WithDescription($"Клан с Id {clanid}, не найден!").Build());

            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan]
        public async Task clanleave()
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Leave");
                var usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                var clan = DBcontext.Clans.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.Id == usr.ClanId);
                emb.WithDescription("Вы успешно вышли из клана!");
                clan = await ClanPay(clan);
                if (clan.ClanRole != 0)
                {
                    await OtherSettings.CheckRoleValid(Context.User as SocketGuildUser, clan.ClanRole, true);
                }
                usr.ClanId = 0;
                usr.clanInfo = Users.UserClanRole.ready;
                DBcontext.Users.Update(usr);
                await DBcontext.SaveChangesAsync();

                await Context.Channel.SendMessageAsync("", false, emb.Build());
                _cache.Removes(Context);
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan]
        public async Task claninfo()
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Info");
                var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                var clanOwner = DBcontext.Clans.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
                var usr = _cache.GetOrCreateUserCache(Context.User.Id, Context.Guild.Id);
                var clans = DBcontext.Clans.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.Id == usr.ClanId);
                if (clans == null && clanOwner == null)
                    emb.WithDescription("У вас нет клана, и вы не состоите в каком либо клане!");
                else
                {
                    if (clanOwner != null)
                    {
                        if (clanOwner.ClanRole != 0 && (Context.User as SocketGuildUser).Roles.Count(x => x.Id == clanOwner.ClanRole) > 0)
                            await OtherSettings.CheckRoleValid(Context.User as SocketGuildUser, clanOwner.ClanRole, false);



                        clanOwner = await ClanPay(clanOwner);
                        var OwnerTop = DBcontext.Clans.AsQueryable().Where(x => x.GuildId == Context.Guild.Id).AsEnumerable().Where(x => x.DefUsers.Count() >= clanOwner.DefUsers.Count()).Count();
                        emb.AddField($"Ваш клан: {clanOwner.ClanName}",
                                                        $"Денег: {(clanOwner.ClanMoney <= -50000 ? $"{clanOwner.ClanMoney} **КЛАН ЗАМОРОЖЕН**" : clanOwner.ClanMoney.ToString())}\n" +
                                                        $"Участников: {clanOwner.DefUsers.Count()}/{clanOwner.ClanSlots}\n" +
                                                        $"Топ: {OwnerTop}\n" +
                                                        $"Удалить клан {GuildPrefix}ClanDelete").WithThumbnailUrl(clanOwner.LogoUrl);
                    }
                    else if (clans != null)
                    {
                        if (clans.ClanRole != 0 && (Context.User as SocketGuildUser).Roles.Count(x => x.Id == clans.ClanRole) > 0)
                            await OtherSettings.CheckRoleValid(Context.User as SocketGuildUser, clanOwner.ClanRole, false);

                        clans = await ClanPay(clans);
                        var clanTop = DBcontext.Clans.AsQueryable().Where(x => x.GuildId == Context.Guild.Id).AsEnumerable().Where(x => x.DefUsers.Count() >= clans.DefUsers.Count()).Count();
                        emb.AddField($"Вы состоите в: {clans.ClanName}",
                                                    $"Участников: {clans.DefUsers.Count()}/{clans.ClanSlots}\n" +
                                                    $"Топ: {clanTop}\n" +
                                                    $"Выйти из клана {GuildPrefix}ClanLeave");

                        if (clanOwner == null)
                            emb.WithThumbnailUrl(clans.LogoUrl);
                    }
                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan, PermissionClanMoneyMinus]
        public async Task clanclaims(SocketUser user = null, string Decision = null)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Claims");
                var clan = DBcontext.Clans.AsQueryable().Where(x => x.GuildId == Context.Guild.Id).AsEnumerable().FirstOrDefault(x => x.OwnerId == Context.User.Id || x.UsersModerators.FirstOrDefault(x => x.UserId == Context.User.Id) != null);
                if (clan != null)
                {
                    if (Decision == null && user == null)
                    {
                        if (clan.UsersWait.Count() > 0)
                        {
                            int page = 0;
                            await ListBuilder(page, clan.UsersWait, emb, "clanclaims");
                            return;
                        }
                        else
                        {
                            emb.WithDescription("Заявок в клан нету!");
                        }
                    }
                    else if (user != null && (Decision.ToLower() == "accept" || Decision.ToLower() == "denied"))
                    {
                        if (clan.UsersWait.FirstOrDefault(x => x.UserId == user.Id) != null)
                        {
                            var usr = _cache.GetOrCreateUserCache(user.Id, Context.Guild.Id);
                            if (Decision.ToLower() == "accept")
                            {
                                if (clan.ClanSlots > (ulong)clan.DefUsers.Count())
                                {
                                    usr.clanInfo = Users.UserClanRole.user;
                                    DBcontext.Users.Update(usr);
                                    await DBcontext.SaveChangesAsync();
                                    emb.WithDescription($"Пользователь {user.Mention} успешно принят в клан");
                                }
                                else emb.WithDescription("Для того чтобы принять больше пользователей, нужно купить слоты!");
                            }
                            else
                            {
                                usr.ClanId = 0;
                                usr.clanInfo = Users.UserClanRole.ready;
                                DBcontext.Users.Update(usr);
                                await DBcontext.SaveChangesAsync();
                                emb.WithDescription($"Заявка в клан от пользователя {user.Mention} отклонена!");
                            }
                        }
                        else emb.WithDescription("Данный пользователь не подавал заявку в клан");
                    }
                    else if (user != null && (Decision.ToLower() != "accept" || Decision.ToLower() != "denied"))
                    {
                        var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                        emb.WithDescription("Ошибка. Вы ввели неправильный параметр. Вы можете или принять или отклонить заявку")
                           .WithFooter($"{GuildPrefix}ClanClaims [clanname] [user] [accept/denied]");
                    }
                }
                else
                    emb.WithDescription("Вы не являетесь создателем или модератором введенного клана!");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan, PermissionClanMoneyMinus]
        public async Task clankick(SocketUser user)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Kick");
                var clan = DBcontext.Clans.AsQueryable().Where(x => x.GuildId == Context.Guild.Id).AsEnumerable().FirstOrDefault(x => x.OwnerId == Context.User.Id || x.UsersModerators.FirstOrDefault(x => x.UserId == Context.User.Id) != null);
                if (clan != null)
                {
                    if (user.Id != clan.OwnerId)
                    {
                        var usr = _cache.GetOrCreateUserCache(user.Id, Context.Guild.Id);
                        if (usr.ClanId == clan.Id && usr.clanInfo != Users.UserClanRole.wait)
                        {
                            if (clan.OwnerId != Context.User.Id && usr.clanInfo == Users.UserClanRole.moder)
                                emb.WithDescription("Только создатель клана может кикать модераторов!");
                            else
                            {
                                if (clan.ClanRole != 0)
                                {
                                    await OtherSettings.CheckRoleValid(user as SocketGuildUser, clan.ClanRole, true);
                                }
                                usr.ClanId = 0;
                                usr.clanInfo = Users.UserClanRole.ready;
                                DBcontext.Users.Update(usr);
                                await DBcontext.SaveChangesAsync();
                                emb.WithDescription($"Пользователь {user.Mention} кикнут из клана!");
                            }
                        }
                        else emb.WithDescription("Пользователь не состоит в клане, или вы его еще не приняли в него!");
                    }
                    else emb.WithDescription("Создателя клана нельзя выгнать!");
                }
                else emb.WithDescription("Вы не являетесь модератором или создателем введенного клана!");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan, PermissionClanMoneyMinus]
        public async Task clanownertake(SocketUser user)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Owner Take");
                var takedclan = DBcontext.Clans.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.OwnerId == user.Id);
                if (takedclan == null)
                {
                    var usr = _cache.GetOrCreateUserCache(user.Id, Context.Guild.Id);
                    var clan = DBcontext.Clans.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
                    if (clan.Id == usr.ClanId)
                    {
                        usr.clanInfo = 0;
                        usr.clanInfo = Users.UserClanRole.ready;
                        DBcontext.Users.Update(usr);
                    }
                    clan.OwnerId = user.Id;
                    DBcontext.Clans.Update(clan);
                    await DBcontext.SaveChangesAsync();
                    emb.WithDescription($"Вы успешно передали клан пользователю {user.Mention}");
                }
                else emb.WithDescription($"Пользователь {user.Mention} уже владеет кланом");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan, PermissionClanMoneyMinus]
        public async Task clanperm(SocketUser user = null, Users.UserClanRole permission = Users.UserClanRole.ready)
        {
            _cache.Removes(Context);
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Permission");
                if (user == null || permission == Users.UserClanRole.ready)
                {
                    var owner = DBcontext.Clans.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
                    foreach (var User in owner.UsersModerators)
                    {
                        emb.AddField($"{Context.Guild.GetUser(User.UserId)}", $"");
                    }
                    //var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                    //emb.WithDescription($"Данная команда выдает клановые права пользователям\nПример: {GuildPrefix}clanperm [user] [moder/user]");
                }
                else
                {
                    if (permission == Users.UserClanRole.moder || permission == Users.UserClanRole.user)
                    {
                        if (Context.User != user)
                        {
                            var owner = DBcontext.Clans.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
                            var usr = DBcontext.Users.FirstOrDefault(x=>x.GuildId == Context.Guild.Id && x.UserId == user.Id);
                            if (owner.Id != usr.ClanId)
                            {
                                if ((usr.clanInfo == Users.UserClanRole.user || usr.clanInfo == Users.UserClanRole.moder))
                                {
                                    if (usr.clanInfo == permission)
                                        emb.WithDescription($"Пользователь уже является {(permission == Users.UserClanRole.moder ? "модератором" : "пользователем")}");
                                    else
                                    {
                                        if (usr.clanInfo == Users.UserClanRole.user && permission == Users.UserClanRole.moder)
                                            emb.WithDescription($"Участник {user.Mention} повышен до модератора");
                                        else
                                            emb.WithDescription($"Участник {user.Mention} понижен до пользователя");

                                        usr.clanInfo = permission;
                                        DBcontext.Users.Update(usr);
                                        await DBcontext.SaveChangesAsync();
                                    }
                                }
                                else emb.WithDescription("Пользователь не состоит в вашем клане!");
                            }
                            else
                                emb.WithDescription("Я смотрю ты у нас пранкер, Эдвард Дебил...");
                        }
                        else emb.WithDescription("Вы не можете изменять свои привелегии.");
                    }
                    else emb.WithDescription("Параметр Permission может содержать только moder или user");
                }
                
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        //[Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        //public async Task clanstop()
        //{
        //    _cache.Removes(Context);
        //    using (var DBcontext = new DBcontext())
        //    {
        //        var clans = DBcontext.Clans.AsQueryable().Where(x => x.guildId == Context.Guild.Id).AsEnumerable().Where(x=>x.DefUsers.Count() > 0).OrderBy(x => x.DefUsers.Count()).ThenBy(x=>x.Id).Reverse();
        //        var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clans Top");
        //        if (clans.Count() > 0)
        //        {
        //            int page = 0;
        //            await ListBuilder(page, clans, emb, "clanstop");
        //        }
        //        else
        //        {
        //            emb.WithDescription("Пока еще нет ниодного клана.").WithFooter("Кланы с 0 участников, не входят в топ.");
        //            await Context.Channel.SendMessageAsync("", false, emb.Build());
        //        }
        //    }
        //}
    }

}
