using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SixLabors.ImageSharp;
using System;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DarlingBotNet.Modules
{
    public class Clan : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _discord;
        private readonly DbService _db;

        public Clan(DiscordSocketClient discord, DbService db)
        {
            _discord = discord;
            _db = db;
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task clancreate(string name, string logourl)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Create");
                var clan = DBcontext.Clans.GetOwnerClan(Context.Guild.Id, Context.User.Id);
                var usr = DBcontext.Users.Get(Context.User.Id, Context.Guild.Id);
                if (clan == null)
                {
                    if (usr.ZeroCoin >= 30000)
                    {
                        if (name.Length >= 4 && name.Length <= 32)
                        {
                            bool es = false;
                            try
                            {
                                WebClient client = new WebClient();
                                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36");
                                var bytez = client.DownloadData(logourl);
                                Bitmap bmp = new Bitmap(new MemoryStream(bytez));
                                es = true;
                            }
                            catch (Exception)
                            {
                                emb.WithDescription("Ваша ссылка не содержит изображение.").WithFooter("Отправьте фото в дискорд, и скопируйте URL. Это может помочь.");
                            }

                            if (es)
                            {
                                DBcontext.Clans.GetOrCreate(name, logourl, Context.User as SocketGuildUser);
                                usr.ZeroCoin -= 30000;
                                DBcontext.Users.Update(usr);
                                await DBcontext.SaveChangesAsync();
                                emb.WithDescription("Вы успешно создали свой клан. Веселитесь 🤟");
                            }
                        }
                        else emb.WithDescription("Название должно быть больше 4 и меньше 32 символов");
                    }
                    else emb.WithDescription($"У вас недостаточно средств для создания. Вам нехватает {30000 - usr.ZeroCoin} ZeroCoin's");
                }
                else emb.WithDescription("У вас уже есть свой клан.").WithFooter("Для того чтобы создать новый, вам нужно удалить или передать старый клан");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task clans(ulong page = 0, uint clanid = 0)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clans List");
                var clans = DBcontext.Clans.GetClans(Context.Guild.Id);
                var glds = DBcontext.Guilds.Get(Context.Guild.Id);
                if (clanid == 0)
                {
                    if (page > Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)) emb.WithDescription("404 Not Found. Вы зашли в темный район, тут никого нет.")
                                                                                              .WithFooter($"Страница { (page == 0 ? 1 : page)}/{ Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)}");
                    else
                    {
                        emb.WithFooter($"Страница {(page == 0 ? 1 : page)}/{Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)} - {glds.Prefix}Clans [page] [clanid]");
                        int circl = 0;
                        foreach (var us in clans.Skip(Convert.ToInt32(page > 0 ? --page : page) * 5))
                        {
                            circl++;
                            emb.AddField($"{us.Id}.{us.ClanName} - TOP {clans.Where(x => x.DefUsers.Count() >= us.DefUsers.Count()).Count()}", $"Участников - {us.DefUsers.Count()}");
                            if (circl >= 5) break;
                        }
                        if (emb.Fields.Count == 0)
                        {
                            emb.WithDescription("Пока еще нет ни одного клана!");
                            emb.Footer.Text = "";
                        }
                    }
                }
                else
                {
                    var thisclan = clans.Where(x => x.Id == clanid).FirstOrDefault();
                    if (thisclan != null)
                    {
                        var usr = DBcontext.Users.Get(Context.User.Id, Context.Guild.Id);
                        if(usr.clanId == (uint)thisclan.Id)
                        {
                            switch (usr.clanInfo)
                            {
                                case "ready":
                                    emb.WithDescription("Вы уже состоите в этом клане");
                                    break;
                                case "moder":
                                    emb.WithDescription("Вы уже являетесь модератором данного клана");
                                    break;
                                case "wait":
                                    emb.WithDescription("Вы уже подали заявку в клан. Ожидайте!");
                                    break;
                                default:
                                    if (usr.ClanOwner == thisclan.Id)
                                        emb.WithDescription("Вы не можете подать заявку в свой клан");
                                    break;
                            }
                        }
                        else
                        {
                            if(usr.clanInfo == "wait" || usr.clanInfo == null)
                            {
                                usr.clanId = (uint)thisclan.Id;
                                usr.clanInfo = "wait";
                                DBcontext.Users.Update(usr);
                                await DBcontext.SaveChangesAsync();
                                emb.WithDescription("Вы успешно подали заявку. Ожидайте пока ее примут или отклонят.");
                            }
                            else emb.WithDescription("Для того чтобы подать заявку, выйдите из прошлого клана");
                        }

                    }
                    else emb.WithDescription("Клана с таким id не существует.");
                }

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan]
        public async Task clandelete(string clanname)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Delete");
                var clan = DBcontext.Clans.GetOwnerClan(Context.Guild.Id, Context.User.Id);
                if (clan.ClanName == clanname)
                {
                    emb.WithDescription("Вы успешно удалили свой клан!");
                    DBcontext.Clans.Remove(clan);
                    await DBcontext.SaveChangesAsync();
                }
                else emb.WithDescription("Введенный клан не является вашим, или вовсе не существует!");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan]
        public async Task clanleave(string clanname)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Leave");
                var usr = DBcontext.Users.Get(Context.User.Id, Context.Guild.Id);
                var clan = DBcontext.Clans.GetUserClan(Context.Guild.Id, usr.clanId);
                if (clan != null)
                {
                    if (clan.ClanName == clanname)
                    {
                        emb.WithDescription("Вы успешно вышли из клана!");
                        usr.clanId = 0;
                        usr.clanInfo = null;
                        DBcontext.Users.Update(usr);
                        await DBcontext.SaveChangesAsync();
                    }
                    else emb.WithDescription("Вы не состоите в веденном клане!");
                }
                else emb.WithDescription("Вы не состоите в клане");

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan]
        public async Task claninfo()
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Info");
                var glds = DBcontext.Guilds.Get(Context.Guild.Id);
                var clanOwner = DBcontext.Clans.GetOwnerClan(Context.Guild.Id, Context.User.Id);
                var usr = DBcontext.Users.Get(Context.User.Id,Context.Guild.Id);
                var clans = DBcontext.Clans.GetUserClan(Context.Guild.Id, usr.clanId);

                if (clanOwner != null)
                {
                    var OwnerTop = DBcontext.Clans.GetClans(Context.Guild.Id).Where(x => x.DefUsers.Count() >= clanOwner.DefUsers.Count()).Count();
                    emb.AddField($"Ваш клан: {clanOwner.ClanName}",
                                                    $"Участников: {clanOwner.DefUsers.Count()}/{clanOwner.ClanSlots}\n" +
                                                    $"Топ: {OwnerTop}\n" +
                                                    $"Удалить клан {glds.Prefix}ClanDelete [clanname]");
                }
                else emb.WithThumbnailUrl(clans.LogoUrl);


                if (clans != null)
                {
                    var clanTop = DBcontext.Clans.GetClans(Context.Guild.Id).Where(x => x.DefUsers.Count() >= clans.DefUsers.Count()).Count();
                    emb.AddField($"Вы состоите в: {clans.ClanName}",
                                                $"Участников: {clans.DefUsers.Count()}/{clans.ClanSlots}\n" +
                                                $"Топ: {clanTop}\n" +
                                                $"Выйти из клана {glds.Prefix}ClanLeave [clanname]");
                }

                else emb.WithThumbnailUrl(clanOwner.LogoUrl);

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan]
        public async Task clanclaims(string clanname, ulong page = 0, SocketUser user = null, string Decision = null)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Claims");
                var clan = DBcontext.Clans.GetClans(Context.Guild.Id).FirstOrDefault(x => x.ClanName == clanname && (x.OwnerId == Context.User.Id || x.UsersModerators.FirstOrDefault(x => x.userid == Context.User.Id) != null));
                var glds = DBcontext.Guilds.Get(Context.Guild.Id);
                if (clan != null)
                {
                    if (Decision == null && user == null)
                    {
                        emb.WithFooter($"Страница {(page == 0 ? 1 : page)}/{Math.Ceiling(Convert.ToDouble(clan.UsersWait.Count()) / 5)}");

                        if (page > Math.Ceiling(Convert.ToDouble(clan.UsersWait.Count()) / 5)) emb.WithDescription("404 Not Found. Вы зашли в темный район, тут никого нет.");
                        else
                        {
                            emb.Footer.Text += $" - {glds.Prefix}ClanClaims [clanname] [page] [user] [accept/denied]";
                            int circl = 0;
                            foreach (var us in clan.UsersWait.Skip(Convert.ToInt32(page > 0 ? --page : page) * 5))
                            {
                                circl++;
                                if (Context.Guild.GetUser(us.userid) != null)
                                    emb.AddField($"{Context.Guild.GetUser(us.userid)}", $"Уровень: {us.Level}");
                                if (circl >= 5) break;
                            }
                            if(emb.Fields.Count == 0)
                            {
                                emb.WithDescription("Заявок в клан нет!");
                                emb.Footer.Text = "";
                            }
                        }
                    }
                    else if (user != null && (Decision.ToLower() == "accept" || Decision.ToLower() == "denied"))
                    {
                        if (clan.ClanSlots > (ulong)clan.DefUsers.Count())
                        {
                            if (clan.UsersWait.FirstOrDefault(x => x.userid == user.Id) != null)
                            {
                                var usr = DBcontext.Users.Get(user.Id, Context.Guild.Id);
                                if (Context.Guild.GetUser(usr.userid) != null)
                                {
                                    if (Decision.ToLower() == "accept")
                                    {
                                        usr.clanInfo = "ready";
                                        emb.WithDescription($"Пользователь {user.Mention} успешно принят в клан");
                                    }
                                    else
                                    {
                                        usr.clanId = 0;
                                        usr.clanInfo = null;
                                        emb.WithDescription($"Заявка в клан от пользователя {user.Mention} отклонена");
                                    }
                                    DBcontext.Users.Update(usr);
                                    await DBcontext.SaveChangesAsync();
                                }
                                else emb.WithDescription("Этого пользователя нельзя принять в клан, пока он не зайдет на сервер.");
                            }
                            else emb.WithDescription("Данный пользователь не подавала заявку в клан");
                        }
                        else emb.WithDescription("Для того чтобы принять больше пользователей, нужно купить слоты");
                    }
                    else if (user != null && (Decision.ToLower() != "accept" || Decision.ToLower() != "denied"))
                        emb.WithDescription("Ошибка. Вы ввели неправильный параметр. Вы можете или принять или отклонить заявку")
                           .WithFooter($"{glds.Prefix}ClanClaims [clanname] [page] [user] [accept/denied]");
                }
                else emb.WithDescription("Вы не являетесь создателем или модератором введенного клана!");


                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan]
        public async Task clankick(SocketUser user, uint clanid)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Kick");
                var clan = DBcontext.Clans.GetClans(Context.Guild.Id).FirstOrDefault(x => x.Id == clanid && (x.OwnerId == Context.User.Id || x.UsersModerators.FirstOrDefault(x => x.userid == Context.User.Id) != null));
                var usr = DBcontext.Users.Get(user.Id, Context.Guild.Id);
                if (user.Id != clan.OwnerId)
                {
                    if (usr.clanId == (ulong)clan.Id && usr.clanInfo != "wait")
                    {
                        if (clan.OwnerId != Context.User.Id && usr.clanInfo == "moder")
                            emb.WithDescription("Только создатель клана может кикать модераторов!");
                        else
                        {
                            usr.clanId = 0;
                            usr.clanInfo = null;
                            DBcontext.Users.Update(usr);
                            await DBcontext.SaveChangesAsync();
                            emb.WithDescription($"Пользователь {user.Mention} кикнут из клана!");
                        }
                    }
                    else emb.WithDescription("Пользователь не состоит в клане, или вы его еще не приняли в него!");
                }
                else emb.WithDescription("Создателя клана нельзя выгнать!");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan]
        public async Task clanownertake(SocketUser user)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Owner Take");
                var clan = DBcontext.Clans.GetOwnerClan(Context.Guild.Id, Context.User.Id);
                var takedclan = DBcontext.Clans.GetOwnerClan(Context.Guild.Id, user.Id);
                if (takedclan == null)
                {
                    clan.OwnerId = user.Id;
                    DBcontext.Clans.Update(clan);
                    await DBcontext.SaveChangesAsync();
                    emb.WithDescription($"Вы успешно передали клан пользователю {user.Mention}");
                }
                else emb.WithDescription($"Пользователь {user.Mention} уже владеет кланом");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan]
        public async Task clanperm(SocketUser user = null, string permission = null)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var glds = DBcontext.Guilds.Get(Context.Guild.Id);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Permission");
                if (user == null && permission == null)
                    emb.WithDescription($"Данная команда выдает клановые права пользователям\nПример: {glds.Prefix}clanperm [user] [moder/user]");
                else
                {
                    if (permission.ToLower() == "moder" || permission.ToLower() == "user")
                    {

                        var owner = DBcontext.Clans.GetOwnerClan(Context.Guild.Id, Context.User.Id);
                        if (Context.User != user)
                        {
                            var usr = DBcontext.Users.Get(user.Id, Context.Guild.Id);
                            if (usr.clanInfo != "wait" && usr.clanId == (ulong)owner.Id)
                            {
                                if (usr.clanInfo == "moder" && permission.ToLower() == "moder" || usr.clanInfo == "user" && permission.ToLower() == "user")
                                    emb.WithDescription($"Пользователь уже является {(permission.ToLower() == "moder" ? "модератором" : "пользователем")}");
                                else
                                {
                                    usr.clanInfo = permission.ToLower();
                                    DBcontext.Users.Update(usr);
                                    await DBcontext.SaveChangesAsync();
                                    if (usr.clanInfo == "user" && permission.ToLower() == "moder")
                                        emb.WithDescription($"Участник {user.Mention} повышен до модератора");
                                    else
                                        emb.WithDescription($"Участник {user.Mention} понижен до пользователя");
                                }
                            }
                            else emb.WithDescription("Пользователь не состоит в вашем клане!");
                        }
                        else emb.WithDescription("Вы не можете изменять свои привелегии.");
                    }
                    else emb.WithDescription("Параметр Permission может содержать только moder или user");

                }
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task clanstop(uint page = 0)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var clans = DBcontext.Clans.GetClans(Context.Guild.Id).OrderBy(x => x.DefUsers.Count()).Reverse();
                var glds = DBcontext.Guilds.Get(Context.Guild.Id);

                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clans Top");

                if (page > Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)) emb.WithDescription("404 Not Found. Вы зашли в темный район, тут никого нет.")
                                                                              .WithFooter($"Страница { (page == 0 ? 1 : page)}/{ Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)}");
                else
                {
                    emb.WithFooter($"Страница {(page == 0 ? 1 : page)}/{Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)} - {glds.Prefix}Clans [page] [clanname]");
                    int circl = 0;
                    foreach (var us in clans.Skip(Convert.ToInt32(page > 0 ? --page : page) * 5))
                    {
                        if (us.DefUsers.Count() != 0)
                        {
                            circl++;
                            emb.AddField($"{us.ClanName} - TOP {clans.Where(x => x.DefUsers.Count() >= us.DefUsers.Count()).Count()}", $"Участников - {us.DefUsers.Count()}");
                            if (circl >= 5) break;
                        }
                    }
                    if (emb.Fields.Count == 0)
                    {
                        emb.WithDescription("Пока еще нет ни одного клана!");
                        emb.Footer.Text = "";
                    }
                }
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }
    }
}
