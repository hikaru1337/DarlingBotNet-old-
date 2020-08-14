//using DarlingBotNet.DataBase;
//using DarlingBotNet.DataBase.Database;
//using DarlingBotNet.Services;
//using Discord;
//using Discord.Commands;
//using Discord.WebSocket;
//using SixLabors.ImageSharp;
//using System;
//using System.Data.Entity;
//using System.Drawing;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Threading.Tasks;

//namespace DarlingBotNet.Modules
//{
//    public class Clan : ModuleBase<SocketCommandContext>
//    {
//        private readonly DiscordSocketClient _discord;
//        private readonly DbService _db;

//        public Clan(DiscordSocketClient discord, DbService db)
//        {
//            _discord = discord;
//            _db = db;
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        public async Task clancreate(string name, string logourl)
//        {
//            using (var DBcontext = _db.GetDbContext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Create");
//                var clan = DBcontext.Clans.FirstOrDefault(x => x.guildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
//                var usr = DBcontext.Users.Get(Context.User.Id,Context.Guild.Id);
//                if (clan == null)
//                {
//                    if (usr.ZeroCoin >= 30000)
//                    {
//                        if (name.Length >= 4 && name.Length <= 32)
//                        {
//                            bool es = false;
//                            try
//                            {
//                                WebClient client = new WebClient();
//                                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36");
//                                var bytez = client.DownloadData(logourl);
//                                Bitmap bmp = new Bitmap(new MemoryStream(bytez));
//                                es = true;
//                            }
//                            catch (Exception)
//                            {
//                                emb.WithDescription("Ваша ссылка не содержит изображение.").WithFooter("Отправьте фото в дискорде, и скопируйте URL. Это может помочь.");
//                            }
//                            if (es)
//                            {
//                                DBcontext.Add(new Clans() { ClanName = name, OwnerId = Context.User.Id, LogoUrl = logourl, guildId = Context.Guild.Id });
//                                await DBcontext.SaveChangesAsync();
//                                emb.WithDescription("Вы успешно создали свой клан. Веселитесь 🤟");
//                            }
//                        }
//                        else emb.WithDescription("Название должно быть больше 4 и меньше 32 символов");
//                    }
//                    else emb.WithDescription($"У вас недостаточно средств для создания. Вам нехватает {30000 - usr.ZeroCoin} ZeroCoin's");
//                }
//                else emb.WithDescription("У вас уже есть свой клан.").WithFooter("Для того чтобы создать новый, вам нужно удалить или передать старый клан");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        public async Task clans(ulong page = 0, string clanname = null)
//        {
//            using (var DBcontext = _db.GetDbContext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clans List");
//                var clans = DBcontext.Clans.AsNoTracking().Where(x => x.guildId == Context.Guild.Id);
//                var glds = DBcontext.Guilds.Get(Context.Guild.Id);
//                if (clanname == null)
//                {
//                    if (page > Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)) emb.WithDescription("404 Not Found. Вы зашли в темный район, тут никого нет.")
//                                                                                              .WithFooter($"Страница { (page == 0 ? 1 : page)}/{ Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)}");
//                    else
//                    {
//                        emb.WithFooter($"Страница {(page == 0 ? 1 : page)}/{Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)} - {glds.Prefix}Clans [page] [clanname]");
//                        int circl = 0;
//                        foreach (var us in clans.Skip(Convert.ToInt32(page > 0 ? --page : page) * 5))
//                        {
//                            circl++;
//                            emb.AddField($"{us.ClanName} - TOP {clans.Where(x => x.DefUsers.Count() >= us.DefUsers.Count()).Count()}", $"Участников - {us.DefUsers.Count()}");
//                            if (circl >= 5) break;
//                        }
//                    }
//                }
//                else
//                {
//                    var thisclan = clans.FirstOrDefault(x => x.ClanName == clanname);
//                    if (thisclan != null)
//                    {
//                        var usr = DBcontext.Users.Get(Context.User.Id,Context.Guild.Id);
//                        usr.clanId = thisclan.ClanId;
//                        usr.clanInfo = "wait";
//                        DBcontext.Users.Update(usr);
//                        await DBcontext.SaveChangesAsync();
//                        emb.WithDescription("Вы успешно подали заявку. Ожидайте пока ее примут или отклонят.");
//                    }
//                    else emb.WithDescription("Клан с таким названием не существует.");
//                }

//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [PermissionClan]
//        public async Task clandelete(string name = null)
//        {
//            using (var DBcontext = _db.GetDbContext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Delete");
//                var clan = DBcontext.Clans.FirstOrDefault(x => x.guildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
//                if (name == null)
//                    emb.WithDescription("Для того чтобы удалить клан введите его название!");
//                else if (clan.ClanName == name)
//                {
//                    emb.WithDescription("Вы успешно удалили свой клан!");
//                    DBcontext.Remove(clan);
//                    await DBcontext.SaveChangesAsync();
//                }
//                else emb.WithDescription("Введенный клан не является вашим, или вовсе не существует!");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [PermissionClan]
//        public async Task clanleave(string name = null)
//        {
//            using (var DBcontext = _db.GetDbContext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Leave");
//                var usr = DBcontext.Users.Get(Context.User.Id,Context.Guild.Id);
//                var clan = DBcontext.Clans.FirstOrDefault(x => x.guildId == Context.Guild.Id && x.ClanId == usr.clanId);

//                if (name == null)
//                    emb.WithDescription("Для того чтобы выйти из клана введите его название!");
//                else if (clan.ClanName == name)
//                {
//                    emb.WithDescription("Вы успешно вышли из клана!");
//                    usr.clanId = 0;
//                    usr.clanInfo = "";
//                    DBcontext.Users.Update(usr);
//                    await DBcontext.SaveChangesAsync();
//                }
//                else emb.WithDescription("Вы не состоите в веденном клане, или он вовсе не существует!");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [PermissionClan]
//        public async Task claninfo()
//        {
//            using (var DBcontext = _db.GetDbContext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Info");
//                var glds = DBcontext.Guilds.Get(Context.Guild.Id);
//                var clanOwner = DBcontext.Clans.FirstOrDefault(x => x.guildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
//                var clans = DBcontext.Clans.FirstOrDefault(x => x.guildId == Context.Guild.Id && x.DefUsers.Where(x => x.userid == Context.User.Id) != null);

//                if (clanOwner != null) emb.AddField($"Ваш клан: {clanOwner.ClanName}",
//                                                    $"Участников: {clanOwner.DefUsers.Count()}/{clans.ClanSlots}")
//                                          .WithFooter($"Удалить клан {glds.Prefix}ClanDelete");


//                if (clans != null) emb.AddField($"Вы состоите в: {clans.ClanName}",
//                                                $"Участников: {clans.DefUsers.Count()}/{clans.ClanSlots}\n" +
//                                                $"Вы на {clans.DefUsers.Where(x => x.XP >= clans.DefUsers.FirstOrDefault(x => x.userid == Context.User.Id).XP).Count()} месте.")
//                                      .WithThumbnailUrl(clans.LogoUrl)
//                                      .WithFooter($"Выйти из клана {glds.Prefix}ClanLeave");

//                if (clans == null && clanOwner == null)
//                    emb.WithDescription("У вас нету своего клана и и так же вы ни где не состоите.");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [PermissionClan]
//        public async Task clanclaims(string clanname, ulong page = 0, SocketUser user = null, string Decision = null)
//        {
//            using (var DBcontext = _db.GetDbContext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Claims");
//                var clan = DBcontext.Clans.FirstOrDefault(x => x.guildId == Context.Guild.Id && x.ClanName == clanname && (x.OwnerId == Context.User.Id || x.UsersModerators.FirstOrDefault(x => x.userid == Context.User.Id) != null));
//                var glds = DBcontext.Guilds.Get(Context.Guild.Id);
//                if (Decision == null && user == null && clan != null)
//                {
//                    if (page > Math.Ceiling(Convert.ToDouble(clan.UsersWait.Count()) / 5)) emb.WithDescription("404 Not Found. Вы зашли в темный район, тут никого нет.")
//                            .WithFooter($"Страница { (page == 0 ? 1 : page)}/{ Math.Ceiling(Convert.ToDouble(clan.UsersWait.Count()) / 5)}");
//                    else
//                    {
//                        emb.WithFooter($"Страница {(page == 0 ? 1 : page)}/{Math.Ceiling(Convert.ToDouble(clan.UsersWait.Count()) / 5)} - {glds.Prefix}ClanClaims [page] [user] [accept/denied]");
//                        int circl = 0;
//                        foreach (var us in clan.UsersWait.Skip(Convert.ToInt32(page > 0 ? --page : page) * 5))
//                        {
//                            circl++;
//                            emb.AddField($"{Context.Guild.GetUser(us.userid).Mention}", $"Уровень: {us.Level}");
//                            if (circl >= 5) break;
//                        }
//                    }
//                }
//                else if (clan != null && user != null && (Decision.ToLower() == "accept" || Decision.ToLower() == "denied"))
//                {
//                    if (clan.ClanSlots > (ulong)clan.DefUsers.Count())
//                    {
//                        if (clan.UsersWait.FirstOrDefault(x => x.userid == user.Id) != null)
//                        {
//                            var usr = DBcontext.Users.FirstOrDefault(x => x.userid == user.Id);
//                            if (Decision.ToLower() == "accept")
//                            {
//                                usr.clanInfo = "ready";
//                                emb.WithDescription($"Пользователь {user.Mention} успешно принят в клан");
//                            }
//                            else
//                            {
//                                usr.clanId = 0;
//                                usr.clanInfo = "";
//                                emb.WithDescription($"Заявка в клан от пользователя {user.Mention} отклонена");
//                            }
//                            DBcontext.Users.Update(usr);
//                            await DBcontext.SaveChangesAsync();
//                        }
//                        else emb.WithDescription("Данный пользователь не подавала заявку в клан");
//                    }
//                    else emb.WithDescription("Для того чтобы принять больше пользователей, нужно купить слоты");
//                }
//                else if (clan != null && user != null && (Decision.ToLower() != "accept" || Decision.ToLower() != "denied"))
//                    emb.WithDescription("Ошибка. Вы ввели неправильный параметр. Вы можете или принять или отклонить заявку")
//                       .WithFooter($"{glds.Prefix}ClanClaims [page] [user] [accept/denied]");
//                else if (clan == null)
//                    emb.WithDescription("Вы не являетесь модератором или создателем этого клана!");


//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [PermissionClan]
//        public async Task clankick(SocketUser user)
//        {
//            using (var DBcontext = _db.GetDbContext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Kick");
//                var clan = DBcontext.Clans.FirstOrDefault(x => x.guildId == Context.Guild.Id && (x.OwnerId == Context.User.Id || x.UsersModerators.FirstOrDefault(x => x.userid == Context.User.Id) != null));
//                var usr = DBcontext.Users.FirstOrDefault(x => x.guildId == Context.Guild.Id && user.Id == x.userid && x.clanId == clan.ClanId && x.clanInfo != "wait");
//                if (usr != null)
//                {
//                    if (usr.clanInfo != "moder")
//                    {
//                        usr.clanId = 0;
//                        usr.clanInfo = "";
//                        DBcontext.Users.Update(usr);
//                        await DBcontext.SaveChangesAsync();
//                        emb.WithDescription($"Пользователь {user.Mention} кикнут из клана!");
//                    }
//                    else emb.WithDescription("Только создатель клана может кикать модераторов!");
//                }
//                else if (user.Id == clan.OwnerId)
//                    emb.WithDescription("Создателя клана нельзя выгнать!");
//                else
//                    emb.WithDescription("Пользователь не состоит в клане, или вы его еще не приняли в него!");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [PermissionClan, PermissionServerOwner]
//        public async Task clanownertake(SocketUser user, string clanname = null)
//        {
//            using (var DBcontext = _db.GetDbContext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Owner Take");
//                var clan = DBcontext.Clans.FirstOrDefault(x => x.guildId == Context.Guild.Id && x.OwnerId == Context.User.Id);
//                if (clanname == null)
//                    emb.WithDescription($"Для того чтобы передать клан другому человеку введите `{clan.ClanName}14500`");
//                else
//                {
//                    clan.OwnerId = user.Id;
//                    DBcontext.Update(clan);
//                    await DBcontext.SaveChangesAsync();
//                    emb.WithDescription($"Вы успешно передали клан пользователю {user.Mention}");
//                }
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [PermissionClan]
//        public async Task clanperm(SocketUser user = null, string permission = null)
//        {
//            using (var DBcontext = _db.GetDbContext())
//            {
//                var glds = DBcontext.Guilds.Get(Context.Guild.Id);
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clan Permission");
//                if (user == null && permission == null)
//                    emb.WithDescription($"Данная команда выдает клановые права пользователям\nПример: {glds.Prefix}clanperm [user] [moder/user]");
//                else
//                {
//                    if (permission.ToLower() == "moder" || permission.ToLower() == "user")
//                    {
//                        var usr = DBcontext.Users.FirstOrDefault(x => x.guildId == Context.Guild.Id && user.Id == x.userid && x.clanInfo != "wait");
//                        if (usr.clanInfo == "moder" && permission.ToLower() == "moder" || usr.clanInfo == "user" && permission.ToLower() == "user")
//                            emb.WithDescription($"Пользователь уже является {(permission.ToLower() == "moder" ? "модератором" : "пользователем")}");
//                        else
//                        {
//                            usr.clanInfo = permission.ToLower();
//                            DBcontext.Users.Update(usr);
//                            await DBcontext.SaveChangesAsync();
//                            if (usr.clanInfo == "user" && permission.ToLower() == "moder")
//                                emb.WithDescription($"Участник {user.Mention} повышен до модератора");
//                            else
//                                emb.WithDescription($"Участник {user.Mention} понижен до пользователя");
//                        }
//                    }
//                    else emb.WithDescription("Параметр Permission может содержать только moder или user");

//                }
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        public async Task clanstop(uint page = 0)
//        {
//            using (var DBcontext = _db.GetDbContext())
//            {
//                var clans = DBcontext.Clans.AsNoTracking().Where(x => x.guildId == Context.Guild.Id).OrderBy(x => x.DefUsers.Count()).Reverse();
//                var glds = DBcontext.Guilds.Get(Context.Guild.Id);

//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Clans Top");

//                if (page > Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)) emb.WithDescription("404 Not Found. Вы зашли в темный район, тут никого нет.")
//                                                                              .WithFooter($"Страница { (page == 0 ? 1 : page)}/{ Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)}");
//                else
//                {
//                    emb.WithFooter($"Страница {(page == 0 ? 1 : page)}/{Math.Ceiling(Convert.ToDouble(clans.Count()) / 5)} - {glds.Prefix}Clans [page] [clanname]");
//                    int circl = 0;
//                    foreach (var us in clans.Skip(Convert.ToInt32(page > 0 ? --page : page) * 5))
//                    {
//                        circl++;
//                        emb.AddField($"{us.ClanName} - TOP {clans.Where(x => x.DefUsers.Count() >= us.DefUsers.Count()).Count()}", $"Участников - {us.DefUsers.Count()}");
//                        if (circl >= 5) break;
//                    }
//                }
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }
//    }
//}
