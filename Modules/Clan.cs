using DarlingBotNet.DataBase;

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
using System.Threading.Tasks;

namespace DarlingBotNet.Modules
{
    public class Clan : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _discord;


        public Clan(DiscordSocketClient discord)
        {
            _discord = discord;

        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task clancreate(string name, string logourl)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Create");
                var clan = DBcontext.Clans.AsNoTracking().FirstOrDefault(x => x.guildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
                var usr = DBcontext.Users.AsNoTracking().FirstOrDefault(x => x.guildId == Context.Guild.Id && x.userid == Context.User.Id);
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
                                DBcontext.Clans.Add(new Clans() { ClanName = name, LogoUrl = logourl, OwnerId = (Context.User as SocketGuildUser).Id, guildId = Context.Guild.Id, ClanSlots = 5 });
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
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clans List");

                var glds = DBcontext.Guilds.AsNoTracking().FirstOrDefault(x => x.guildid == Context.Guild.Id);
                if (clanid == 0)
                {
                    var clans = DBcontext.Clans.AsNoTracking().Where(x => x.guildId == Context.Guild.Id);
                    if (page > Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)) emb.WithDescription("404 Not Found. Вы зашли в темный район, тут никого нет.")
                                                                                              .WithFooter($"Страница { (page == 0 ? 1 : page)}/{ Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)}");
                    else
                    {
                        emb.WithFooter($"Страница {(page == 0 ? 1 : page)}/{Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)} - {glds.Prefix}Clans [page] [clanid]");
                        int circl = 0;
                        foreach (var us in clans.Skip(Convert.ToInt32(page > 0 ? --page : page) * 5))
                        {
                            circl++;
                            emb.AddField($"{us.Id}.{us.ClanName} {(clans.AsEnumerable().Count() == 0 ? "" : $" - TOP { clans.AsEnumerable().Count(x => x.DefUsers.Count() >= us.DefUsers.Count())}")}", $"Участников - {us.DefUsers.Count()}");
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
                    var thisclan = DBcontext.Clans.AsNoTracking().FirstOrDefault(x => x.Id == clanid);
                    if (thisclan != null)
                    {
                        var usr = DBcontext.Users.AsNoTracking().FirstOrDefault(x => x.guildId == Context.Guild.Id && x.userid == Context.User.Id);
                        if (usr.ClanOwner == thisclan.Id)
                            emb.WithDescription("Вы не можете подать заявку в свой клан");
                        else
                        {
                            if (usr.clanId == (uint)thisclan.Id)
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
                                    usr.clanId = (uint)thisclan.Id;
                                    usr.clanInfo =  Users.UserClanRole.wait;
                                    DBcontext.Users.Update(usr);
                                    await DBcontext.SaveChangesAsync();
                                    emb.WithDescription("Вы успешно подали заявку. Ожидайте пока ее примут или отклонят.");
                                }
                                else emb.WithDescription("Для того чтобы подать заявку, выйдите из прошлого клана");
                            }
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
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Delete");
                var clan = DBcontext.Clans.AsNoTracking().FirstOrDefault(x => x.guildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
                if (clan.ClanName == clanname)
                {
                    var usrs = DBcontext.Users.AsNoTracking().Where(x => x.guildId == Context.Guild.Id && x.clanId == clan.Id);
                    foreach (var usr in usrs)
                    {
                        usr.clanId = 0;
                        usr.clanInfo = Users.UserClanRole.ready;
                    }
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
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Leave");
                var usr = DBcontext.Users.AsNoTracking().FirstOrDefault(x => x.guildId == Context.Guild.Id && x.userid == Context.User.Id);
                var clan = DBcontext.Clans.AsNoTracking().FirstOrDefault(x => x.guildId == Context.Guild.Id && x.Id == usr.clanId);
                if (clan != null)
                {
                    if (clan.ClanName == clanname)
                    {
                        emb.WithDescription("Вы успешно вышли из клана!");
                        usr.clanId = 0;
                        usr.clanInfo = Users.UserClanRole.ready;
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
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Info");
                var glds = DBcontext.Guilds.AsNoTracking().FirstOrDefault(x => x.guildid == Context.Guild.Id);
                var clanOwner = DBcontext.Clans.AsNoTracking().FirstOrDefault(x => x.guildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
                var usr = DBcontext.Users.AsNoTracking().FirstOrDefault(x => x.userid == Context.User.Id && x.guildId == Context.Guild.Id);
                var clans = DBcontext.Clans.AsNoTracking().FirstOrDefault(x => x.guildId == Context.Guild.Id && x.Id == usr.clanId);

                if (clanOwner != null)
                {
                    var OwnerTop = DBcontext.Clans.AsNoTracking().Where(x => x.guildId == Context.Guild.Id).AsEnumerable().Where(x => x.DefUsers.Count() >= clanOwner.DefUsers.Count()).Count();
                    emb.AddField($"Ваш клан: {clanOwner.ClanName}",
                                                    $"Участников: {clanOwner.DefUsers.Count()}/{clanOwner.ClanSlots}\n" +
                                                    $"Топ: {OwnerTop}\n" +
                                                    $"Удалить клан {glds.Prefix}ClanDelete [clanname]");
                }
                else emb.WithThumbnailUrl(clans.LogoUrl);


                if (clans != null)
                {
                    var clanTop = DBcontext.Clans.AsNoTracking().Where(x => x.guildId == Context.Guild.Id).AsEnumerable().Where(x => x.DefUsers.Count() >= clans.DefUsers.Count()).Count();
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
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Claims");
                var clan = DBcontext.Clans.AsNoTracking().Where(x => x.guildId == Context.Guild.Id && x.ClanName == clanname).AsEnumerable().FirstOrDefault(x => x.OwnerId == Context.User.Id || x.UsersModerators.FirstOrDefault(x => x.userid == Context.User.Id) != null);
                var glds = DBcontext.Guilds.AsNoTracking().FirstOrDefault(x => x.guildid == Context.Guild.Id).Prefix;
                if (clan != null)
                {
                    if (Decision == null && user == null)
                    {
                        emb.WithFooter($"Страница {(page == 0 ? 1 : page)}/{Math.Ceiling(Convert.ToDouble(clan.UsersWait.Count()) / 5)}");

                        if (page > Math.Ceiling(Convert.ToDouble(clan.UsersWait.Count()) / 5)) emb.WithDescription("404 Not Found. Вы зашли в темный район, тут никого нет.");
                        else
                        {
                            emb.Footer.Text += $" - {glds}ClanClaims [clanname] [page] [user] [accept/denied]";
                            int circl = 0;
                            foreach (var us in clan.UsersWait.Skip(Convert.ToInt32(page > 0 ? --page : page) * 5))
                            {
                                circl++;
                                if (Context.Guild.GetUser(us.userid) != null)
                                    emb.AddField($"{Context.Guild.GetUser(us.userid)}", $"Уровень: {us.Level}");
                                if (circl >= 5) break;
                            }
                            if (emb.Fields.Count == 0)
                            {
                                emb.WithDescription("Заявок в клан нет!");
                                emb.Footer.Text = "";
                            }
                        }
                    }
                    else if (user != null && (Decision.ToLower() == "accept" || Decision.ToLower() == "denied"))
                    {
                        if (clan.UsersWait.FirstOrDefault(x => x.userid == user.Id) != null)
                        {
                            var usr = DBcontext.Users.AsNoTracking().FirstOrDefault(x => x.guildId == Context.Guild.Id && x.userid == user.Id);
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
                                usr.clanId = 0;
                                usr.clanInfo = Users.UserClanRole.ready;
                                DBcontext.Users.Update(usr);
                                await DBcontext.SaveChangesAsync();
                                emb.WithDescription($"Заявка в клан от пользователя {user.Mention} отклонена!");
                            }
                        }
                        else emb.WithDescription("Данный пользователь не подавал заявку в клан");
                    }
                    else if (user != null && (Decision.ToLower() != "accept" || Decision.ToLower() != "denied"))
                        emb.WithDescription("Ошибка. Вы ввели неправильный параметр. Вы можете или принять или отклонить заявку")
                           .WithFooter($"{glds}ClanClaims [clanname] [page] [user] [accept/denied]");
                }
                else emb.WithDescription("Вы не являетесь создателем или модератором введенного клана!");


                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan]
        public async Task clankick(SocketUser user, uint clanid)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Kick");
                var clan = DBcontext.Clans.AsNoTracking().Where(x => x.guildId == Context.Guild.Id && x.Id == clanid).AsEnumerable().FirstOrDefault(x => x.OwnerId == Context.User.Id || x.UsersModerators.FirstOrDefault(x => x.userid == Context.User.Id) != null);
                var usr = DBcontext.Users.AsNoTracking().FirstOrDefault(x => x.guildId == Context.Guild.Id && x.userid == user.Id);
                if (clan != null)
                {
                    if (user.Id != clan.OwnerId)
                    {
                        if (usr.clanId == clan.Id && usr.clanInfo != Users.UserClanRole.wait)
                        {
                            if (clan.OwnerId != Context.User.Id && usr.clanInfo == Users.UserClanRole.moder)
                                emb.WithDescription("Только создатель клана может кикать модераторов!");
                            else
                            {
                                usr.clanId = 0;
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
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        [PermissionClan]
        public async Task clanownertake(SocketUser user)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Owner Take");
                var clan = DBcontext.Clans.AsNoTracking().FirstOrDefault(x => x.guildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
                var takedclan = DBcontext.Clans.AsNoTracking().FirstOrDefault(x => x.guildId == Context.Guild.Id && x.OwnerId == user.Id);
                if (takedclan == null)
                {
                    var usr = DBcontext.Users.AsNoTracking().FirstOrDefault(x => x.userid == user.Id && x.guildId == Context.Guild.Id);
                    if(clan.Id == usr.clanId)
                    {
                        usr.clanId = 0;
                        usr.clanInfo = Users.UserClanRole.ready;
                    }
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
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Permission");
                if (user == null || permission == null)
                {
                    var glds = DBcontext.Guilds.AsNoTracking().FirstOrDefault(x => x.guildid == Context.Guild.Id).Prefix;
                    emb.WithDescription($"Данная команда выдает клановые права пользователям\nПример: {glds}clanperm [user] [moder/user]");
                }
                else
                {
                    permission = permission.ToLower();
                    if (permission == "moder" || permission == "user")
                    {
                        Users.UserClanRole role = Users.UserClanRole.user;
                        if (permission == "moder") role = Users.UserClanRole.moder;

                        if (Context.User != user)
                        {
                            var owner = DBcontext.Clans.AsNoTracking().FirstOrDefault(x => x.guildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
                            var usr = DBcontext.Users.AsNoTracking().FirstOrDefault(x => x.guildId == Context.Guild.Id && x.userid == user.Id);
                            if (owner.Id == usr.clanId && (usr.clanInfo == Users.UserClanRole.user || usr.clanInfo == Users.UserClanRole.moder))
                            {
                                if (usr.clanInfo == role)
                                    emb.WithDescription($"Пользователь уже является {(permission== "moder" ? "модератором" : "пользователем")}");
                                else
                                {
                                    if (usr.clanInfo == Users.UserClanRole.user && role == Users.UserClanRole.moder)
                                        emb.WithDescription($"Участник {user.Mention} повышен до модератора");
                                    else
                                        emb.WithDescription($"Участник {user.Mention} понижен до пользователя");
                                    
                                    usr.clanInfo = role;
                                    DBcontext.Users.Update(usr);
                                    await DBcontext.SaveChangesAsync();
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
            using (var DBcontext = new DBcontext())
            {
                var clans = DBcontext.Clans.AsNoTracking().Where(x => x.guildId == Context.Guild.Id).AsEnumerable().OrderBy(x => x.DefUsers.Count()).Reverse();
                var glds = DBcontext.Guilds.AsNoTracking().FirstOrDefault(x => x.guildid == Context.Guild.Id);

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
                            emb.AddField($"{us.ClanName} - TOP {clans.AsEnumerable().Where(x => x.DefUsers.Count() >= us.DefUsers.Count()).Count()}", $"Участников - {us.DefUsers.Count()}");
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
