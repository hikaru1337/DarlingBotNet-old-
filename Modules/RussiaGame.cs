//using DarlingBotNet.DataBase;
//using DarlingBotNet.DataBase.Database;
//using DarlingBotNet.DataBase.RussiaGame;
//using DarlingBotNet.Services;
//using Discord;
//using Discord.Commands;
//using Discord.WebSocket;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Caching.Memory;
//using Pcg;
//using SixLabors.ImageSharp.Processing;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Security.Cryptography.X509Certificates;
//using System.Threading;
//using System.Threading.Tasks;
//using static DarlingBotNet.Services.CommandHandler;

//namespace DarlingBotNet.Modules
//{
//    public class RussiaGame : ModuleBase<SocketCommandContext>
//    {
//        private readonly IMemoryCache _cache;


//        public RussiaGame(IMemoryCache cache)
//        {
//            _cache = cache;
//        }

//        public static async Task<RussiaGame_Profile> CreateUser(SocketUser user)
//        {
//            using (var DBcontext = new DBcontext())
//            {
//                var usr = DBcontext.RG_Profile.FirstOrDefault(x => x.userid == user.Id && x.guildid == (user as SocketGuildUser).Guild.Id);
//                if (usr == null) usr = DBcontext.RG_Profile.Add(new RussiaGame_Profile() { guildid = (user as SocketGuildUser).Guild.Id, userid = user.Id, money = 1000 }).Entity;
//                await DBcontext.SaveChangesAsync();
//                return usr;
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        public async Task RGstudy(ulong studyid = 0)
//        {
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Выбор учебы");
//                var studys = DBcontext.RG_Study.AsEnumerable();
//                int page = 0;
//                if (page == 0 || studyid == 0)
//                {
//                    if (studys.Count() == 0) emb.WithDescription("Учебных заведений нет!");
//                    else
//                    {
//                        if (page > Math.Ceiling(Convert.ToDouble(studys.Count()) / 5)) emb.WithDescription("404 Not Found. Вы зашли в темный район, тут никого нет.").WithFooter($"Страница { (page == 0 ? 1 : page)}/{ Math.Ceiling(Convert.ToDouble(studys.Count()) / 5)}");
//                        else
//                        {
//                            await ListBuilder(page, studys, emb, "RGstudy");
//                        }
//                        emb.Footer.Text += $"Страница { (page == 0 ? 1 : page)}/{ Math.Ceiling(Convert.ToDouble(studys.Count()) / 5)}";
//                    }
//                }
//                else
//                {
//                    var thisstudy = DBcontext.RG_Study.FirstOrDefault(x => x.studyid == studyid);
//                    if (thisstudy != null)
//                    {
//                        var usr = DBcontext.RG_Profile.FirstOrDefault(x => x.userid == Context.User.Id && x.guildid == Context.Guild.Id);
//                        if (usr.StudyNowid != thisstudy.studyid)
//                        {
//                            if (usr.Studys.FirstOrDefault(x => x.studyid == thisstudy.studyid) == null)
//                            {
//                                if (usr.money >= (long)thisstudy.StudyMoney)
//                                {
//                                    usr.DaysStudy = thisstudy.DayStudying;
//                                    usr.StudyNowid = thisstudy.studyid;
//                                    usr.money -= (long)thisstudy.StudyMoney;
//                                    DBcontext.RG_Profile.Update(usr);
//                                    await DBcontext.SaveChangesAsync();
//                                    emb.WithDescription($"Вы успешно поступили в {thisstudy.studyName}");
//                                }
//                                else emb.WithDescription($"Вам не хватает {(long)thisstudy.StudyMoney - usr.money} для поступления!");
//                            }
//                            else emb.WithDescription("Вы уже обучались в этом заведении!");
//                        }
//                        else emb.WithDescription("Вы уже обучаетесь в этом заведении!");
//                    }
//                    else emb.WithDescription("Такого учебного заведения нет.");
//                }
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//                _cache.Remove(_cache);
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        public async Task RGwork(ulong workid = 0)
//        {
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Выбор работы");
//                int page = 0;
//                if (page == 0 || workid == 0)
//                {
//                    var studys = DBcontext.RG_Work.AsEnumerable();
//                    if (studys.Count() == 0) emb.WithDescription("Учебных заведений нет!");
//                    else
//                    {
//                        if (page > Math.Ceiling(Convert.ToDouble(studys.Count()) / 5)) emb.WithDescription("404 Not Found. Вы зашли в темный район, тут никого нет.").WithFooter($"Страница { (page == 0 ? 1 : page)}/{ Math.Ceiling(Convert.ToDouble(studys.Count()) / 5)}");
//                        else
//                        {
//                            await ListBuilder(page, studys, emb, "RGwork");
//                        }
//                    }
//                }
//                else
//                {

//                    var thiswork = DBcontext.RG_Work.FirstOrDefault(x => x.workid == workid);
//                    if (thiswork != null)
//                    {
//                        var usr = await CreateUser(Context.User);
//                        var workstudy = DBcontext.RG_Study.FirstOrDefault(x => x.studyid == thiswork.studyid);
//                        bool es = false;
//                        if (thiswork.studyid == 0)
//                            es = true;
//                        else
//                        {
//                            if (usr.StudyNowid != workstudy.studyid)
//                            {
//                                if (usr.Studys.FirstOrDefault(x => x.studyid == thiswork.studyid) != null)
//                                    es = true;
//                                else emb.WithDescription($"Для того чтобы устроиться на работу, нужно окончить {workstudy.studyName}");
//                            }
//                            else emb.WithDescription("Для того чтобы устроиться на эту работу, вы должны закончить обучение в действующем учебном заведении.");
//                        }

//                        if (es)
//                        {
//                            if (usr.money >= (long)thiswork.money)
//                            {
//                                usr.workid = thiswork.workid;
//                                usr.workStreak = 0;
//                                DBcontext.RG_Profile.Update(usr);
//                                await DBcontext.SaveChangesAsync();
//                                emb.WithDescription($"Вы успешно поступили в {thiswork.workName}");
//                            }
//                            else emb.WithDescription($"Вам не хватает {(long)thiswork.money - usr.money} для устройства на работу!");
//                        }
//                    }
//                    else emb.WithDescription("Такой работы нет.");
//                }
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//                _cache.Remove(_cache);
//            }
//        }

//        private static readonly List<Checking> slide = new List<Checking>();

//        private async Task ListBuilder(int page, IEnumerable<object> item, EmbedBuilder emb, string CommandName)
//        {
//            int countslots = 10;
//            var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
//            _cache.Removes(Context);
//            emb = Lists(page, item.Skip(page * countslots).Take(countslots), GuildPrefix, CommandName, item.Count());
//            var mes = await Context.Channel.SendMessageAsync("", false, emb.Build());
//            if (item.Count() > countslots)
//            {
//                var check = new Checking() { userid = Context.User.Id, messid = mes.Id };
//                IEmote emjto = new Emoji("▶️");
//                IEmote emjot = new Emoji("◀️");
//                var time = DateTime.Now.AddSeconds(30);
//                slide.Add(check);
//                await mes.AddReactionAsync(emjto);
//                while (time > DateTime.Now)
//                {
//                    var res = slide.FirstOrDefault(x => x == check);
//                    if (res.clicked != 0)
//                    {
//                        if (res.clicked == 2) page--;
//                        else page++;

//                        emb = Lists(page, item.Skip(page * countslots).Take(countslots), GuildPrefix, CommandName, item.Count());
//                        await mes.ModifyAsync(x => x.Embed = emb.Build());
//                        slide.FirstOrDefault(x => x == check).clicked = 0;

//                        if (page > 1)
//                            await mes.RemoveReactionAsync(emjot, Context.User);
//                        if (page < Math.Ceiling(Convert.ToDouble(item.Count()) / 10))
//                            await mes.RemoveReactionAsync(emjto, Context.User);

//                        time = DateTime.Now.AddSeconds(30);
//                    }
//                }
//                slide.Remove(check);
//                await mes.RemoveAllReactionsAsync();
//            }
//        }
//        private EmbedBuilder Lists(int page, IEnumerable<object> items, string GuildPrefix, string CommandName, int CountItems)
//        {
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94);
//                int countslot = 10;

//                emb.WithFooter($"Страница {(page == 0 ? 1 : page)}/{Math.Ceiling(Convert.ToDouble(CountItems) / countslot)} - ");

//                if (CommandName == "RGwork")
//                {
//                    emb.Footer.Text += $"Купить - {GuildPrefix}rgw [itemid]";

//                    foreach (var us in items.OfType<RussiaGame_Work>().Skip(Convert.ToInt32(page > 0 ? --page : page) * countslot).Take(countslot))
//                    {
//                        var obr = DBcontext.RG_Study.FirstOrDefault(x => x.studyid == us.studyid);
//                        emb.AddField($"{us.workid}.{us.workName}", $"Зарплата: {us.money} {(obr == null ? "" : $"Образование: {obr.studyName}")}", true);
//                    }

//                }
//                else if(CommandName == "RGbazar")
//                {
//                    emb.Footer.Text += $"Работать - {GuildPrefix}rgb [workid]";
//                    foreach (var us in items.OfType<RussiaGame_Item>().Skip(Convert.ToInt32(page > 0 ? --page : page) * countslot).Take(countslot))
//                    {
//                        emb.AddField($"{us.itemid}.{us.ItemName}", $"Цена: {us.NowPrice} Престиж: {us.NowPrestije}", true);
//                    }
//                }
//                else if (CommandName == "RGshop")
//                {
//                    emb.Footer.Text += $"Работать - {GuildPrefix}rgs [workid]";
//                    foreach (var us in items.OfType<RussiaGame_Item>().Skip(Convert.ToInt32(page > 0 ? --page : page) * countslot).Take(countslot))
//                    {
//                        emb.AddField($"{us.itemid}.{us.ItemName}", $"Цена: {us.NowPrice} Престиж: {us.NowPrestije}", true);
//                    }
//                }
//                return emb;
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [RequireOwner]
//        public async Task RGstudyadd(string studyname, ushort studymoney, ushort studyday)
//        {
//            _cache.Remove(_cache);
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - RG StudyAdd");
//                if (DBcontext.RG_Study.FirstOrDefault(x => x.studyName == studyname) == null)
//                {
//                    DBcontext.RG_Study.Add(new RussiaGame_Study() { studyName = studyname, StudyMoney = studymoney, DayStudying = studyday });
//                    emb.WithDescription($"Образование успешно добавлено!\nИмя: {studyname}\nСтоимость: {studymoney}\nВремя обучения: {studyday} дней");
//                    await DBcontext.SaveChangesAsync();
//                }
//                else emb.WithDescription("Образование с таким именем уже присутствует в слотах!");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [RequireOwner]
//        public async Task RGstudyinvsee(ulong studyid)
//        {
//            _cache.Remove(_cache);
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - RG studyinvsee");
//                var studys = DBcontext.RG_Study.FirstOrDefault(x => x.studyid == studyid);
//                if (studys != null)
//                {
//                    emb.WithDescription($"Образование успешно {(studys.Invise ? "скрыта" : "открыта")}!!\nИмя: {studys.studyName}\nСтоимость: {studys.StudyMoney}\nВремя обучения: {studys.DayStudying} дней");
//                    studys.Invise = !studys.Invise;
//                    DBcontext.RG_Study.Update(studys);
//                    await DBcontext.SaveChangesAsync();
//                }
//                else emb.WithDescription("Образование с таким id отсутствует в слотах!");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [RequireOwner]
//        public async Task RGstudydel(ulong studyid)
//        {
//            _cache.Remove(_cache);
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - RG StudyDel");
//                var item = DBcontext.RG_Study.FirstOrDefault(x => x.studyid == studyid);
//                if (item != null)
//                {
//                    var usrs = DBcontext.RG_Profile.AsQueryable().Where(x => x.StudyNowid == studyid);
//                    foreach (var usr in usrs)
//                    {
//                        usr.StudyNowid = 0;
//                        usr.DaysStudy = 0;
//                        usr.LastStudy.AddYears(-DateTime.Now.Year + 1);
//                    }
//                    DBcontext.RG_Profile.UpdateRange(usrs);
//                    DBcontext.RG_Study.Remove(item);
//                    emb.WithDescription($"Работа успешно удалена!\nИмя: {item.studyName}\nЦена: {item.StudyMoney}");
//                    await DBcontext.SaveChangesAsync();
//                }
//                else emb.WithDescription("Образования с таким id не существует!");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [RequireOwner]
//        public async Task RGworkadd(string workname, ushort workmoney, ushort studyid = 0)
//        {
//            _cache.Remove(_cache);
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - RG WorkAdd");
//                if (DBcontext.RG_Work.FirstOrDefault(x => x.workName == workname) == null)
//                {
//                    var studys = DBcontext.RG_Study.FirstOrDefault(x => x.studyid == studyid);
//                    if (studys != null || studyid == 0)
//                    {
//                        DBcontext.RG_Work.Add(new RussiaGame_Work() { workName = workname, money = workmoney, studyid = studyid });
//                        emb.WithDescription($"Работа успешно добавлена!\nИмя: {workname}\nЗарплата: {workmoney}\nОбразование: {(studyid == 0 ? "Отсутствует" : $"{studys.studyid}.{studys.studyName}")}");
//                        await DBcontext.SaveChangesAsync();
//                    }
//                    else emb.WithDescription($"Работа с id {studyid} не найдено!");
//                }
//                else emb.WithDescription("Работа с таким именем уже присутствует в слотах!");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [RequireOwner]
//        public async Task RGworkinvsee(ulong workid)
//        {
//            _cache.Remove(_cache);
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - RG work invsee");
//                var works = DBcontext.RG_Work.FirstOrDefault(x => x.workid == workid);
//                if (works != null)
//                {
//                    emb.WithDescription($"Работа успешно {(works.Invise ? "скрыта" : "открыта")}!\nИмя: {works.workName}\nЗарплата: {works.money}\nВремя обучения: {works.money}");
//                    works.Invise = !works.Invise;
//                    DBcontext.RG_Work.Update(works);
//                    await DBcontext.SaveChangesAsync();
//                }
//                else emb.WithDescription("Работа с таким id отсутствует в слотах!");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [RequireOwner]
//        public async Task RGworkdel(ulong workid)
//        {
//            _cache.Remove(_cache);
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - RG WorkDel");
//                var item = DBcontext.RG_Work.FirstOrDefault(x => x.workid == workid);
//                if (item != null)
//                {
//                    var usrs = DBcontext.RG_Profile.AsQueryable().Where(x => x.workid == item.workid);
//                    foreach (var usr in usrs)
//                    {
//                        usr.workid = 0;
//                        usr.workStreak = 0;
//                        usr.LastWork.AddYears(-DateTime.Now.Year + 1);
//                    }
//                    DBcontext.RG_Profile.UpdateRange(usrs);
//                    DBcontext.RG_Work.Remove(item);
//                    emb.WithDescription($"Работа успешно удалена!\nИмя: {item.workName}\nЗарплата: {item.money}");
//                    await DBcontext.SaveChangesAsync();
//                }
//                else emb.WithDescription("Работы с таким id не существует!");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [RequireOwner]
//        public async Task RGitemadd(string itemname, ulong startprice, ulong startprestije)
//        {
//            _cache.Remove(_cache);
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - RG ItemAdd");
//                if (DBcontext.RG_Item.FirstOrDefault(x => x.ItemName == itemname) == null)
//                {
//                    DBcontext.RG_Item.Add(new RussiaGame_Item() { guildid = 0, ItemName = itemname, userid = Context.Client.CurrentUser.Id, startprice = startprice, startprestije = startprestije });
//                    emb.WithDescription($"Вещь успешно создана!\nИмя: {itemname}\nЦена: {startprice}\nПрестиж: {startprestije}");
//                    await DBcontext.SaveChangesAsync();
//                }
//                else emb.WithDescription("Вещь с таким именем уже существует!");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [RequireOwner]
//        public async Task RGitemdel(ulong itemid)
//        {
//            _cache.Remove(_cache);
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - RG ItemDel");
//                var item = DBcontext.RG_Item.FirstOrDefault(x => x.itemid == itemid);
//                if (item != null)
//                {
//                    var usrs = DBcontext.RG_Item.AsQueryable().Where(x => x.ItemName == item.ItemName);
//                    DBcontext.RG_Item.RemoveRange(usrs);
//                    DBcontext.RG_Item.Remove(item);
//                    emb.WithDescription($"Вещь успешно удалена!\nИмя: {item.ItemName}\nЦена: {item.startprice}\nПрестиж: {item.startprestije}");
//                    await DBcontext.SaveChangesAsync();
//                }
//                else emb.WithDescription("Вещь с таким именем не существует!");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        public async Task RGprofile(SocketUser user = null)
//        {
//            _cache.Remove(_cache);
//            if (user == null) user = Context.User;
//            using (var DBcontext = new DBcontext())
//            {
//                var usr = await CreateUser(user);
//                var work = DBcontext.RG_Work.FirstOrDefault(x => x.workid == usr.workid);
//                var obr = DBcontext.RG_Study.FirstOrDefault(x => x.studyid == usr.StudyNowid);
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - RG профиль {user}", user.GetAvatarUrl());
//                emb.WithDescription($"Престиж: {usr.Prestije}\n" +
//                                    $"Деньги: {usr.money}\n" +
//                                    $"Образование: {(obr == null ? "Отсутствует" : $"{obr.studyName}")}\n" +
//                                    $"Работа: {(work == null ? "Отсутствует" : $"{work.workName}")}\n" +
//                                    $"Кол-во образований: {usr.Studys.Count()}");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }

//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        [PermissionRussiaGame]
//        public async Task RGstudys()
//        {
//            _cache.Remove(_cache);
//            using (var DBcontext = new DBcontext())
//            {
//                var usr = await CreateUser(Context.User);
//                var obr = DBcontext.RG_Study.FirstOrDefault(x => x.studyid == usr.StudyNowid);
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - RG учеба {Context.User}", Context.User.GetAvatarUrl());
//                if (DateTime.Now > usr.LastStudy)
//                {
//                    if (usr.LastStudy.Year == 1) usr.LastStudy = DateTime.Now.AddDays(-1);

//                    if (Math.Abs(DateTime.Now.Day - usr.LastStudy.Day) > 5)
//                    {
//                        usr.StudyNowid = 0;
//                        usr.DaysStudy = 0;
//                        usr.LastStudy.AddYears(-DateTime.Now.Year + 1);
//                        emb.WithDescription($"Вы не ходили на учебу {DateTime.Now.Day - usr.LastStudy.Day} дней. Вы были отчислены за прогулы!").WithColor(255, 0, 94);
//                    }
//                    else
//                    {
//                        usr.LastStudy = DateTime.Now.AddDays(1);
//                        usr.DaysStudy--;
//                        if (usr.DaysStudy == 0)
//                        {
//                            var rs = new RussiaGame_Studys()
//                            {
//                                guildid = Context.Guild.Id,
//                                userid = Context.User.Id,
//                                studyid = usr.StudyNowid
//                            };

//                            usr.StudyNowid = 0;
//                            usr.DaysStudy = 0;
//                            usr.LastStudy.AddYears(-DateTime.Now.Year + 1);
//                            DBcontext.RG_Studys.Add(rs);
//                            emb.WithDescription("Вы закончили учебу, поздравляем с выпуском!");
//                        }
//                        else
//                            emb.WithDescription($"Вы сходили на учебу, до окончания учебы осталось {usr.DaysStudy} дней!");

//                        int rnd = new PcgRandom(1488).Next(0, 1000);
//                        if (rnd <= 100)
//                        {
//                            rnd = new PcgRandom(1488).Next(300, 3000);
//                            usr.money += rnd;
//                            emb.Description += $"\nВозвращаясь домой, на дороге вы нашли {rnd} coin's";
//                        }
//                    }
//                    DBcontext.RG_Profile.Update(usr);
//                    await DBcontext.SaveChangesAsync();
//                }
//                else
//                {
//                    if ((usr.LastStudy - DateTime.Now).TotalSeconds >= 3600)
//                        emb.WithDescription($"Дождитесь {(usr.LastStudy - DateTime.Now).Hours} часов и {(usr.LastStudy - DateTime.Now).Minutes} минут чтобы снова сходить на учебу!");

//                    else if ((usr.LastStudy - DateTime.Now).TotalSeconds <= 3600)
//                        emb.WithDescription($"Дождитесь {((usr.LastStudy - DateTime.Now).TotalSeconds > 60 ? $"{(usr.LastStudy - DateTime.Now).Minutes} минут и " : "")} {(usr.LastStudy - DateTime.Now).Seconds} секунд чтобы снова сходить на учебу!");
//                }
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand, NotDBCommand]
//        [PermissionRussiaGame]
//        public async Task RGworking()
//        {
//            _cache.Remove(_cache);
//            using (var DBcontext = new DBcontext())
//            {
//                var usr = await CreateUser(Context.User);
//                var work = DBcontext.RG_Work.FirstOrDefault(x => x.workid == usr.workid);
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - RG работа {Context.User}", Context.User.GetAvatarUrl());
//                if (DateTime.Now > usr.LastWork)
//                {
//                    if (usr.LastWork.Year == 1) usr.LastWork = DateTime.Now.AddDays(-1);

//                    if (Math.Abs(DateTime.Now.Day - usr.LastWork.Day) > 1)
//                    {
//                        if (Math.Abs(DateTime.Now.Day - usr.LastWork.Day) > 5)
//                        {
//                            emb.WithDescription("Вы были уволены за неоднократные прогулы!");
//                            usr.workid = 0;
//                            usr.workStreak = 0;
//                            usr.LastWork.AddYears(-DateTime.Now.Year + 1);
//                        }
//                        else
//                        {
//                            usr.workStreak = 1;
//                            usr.money += (long)(500 + ((500 / 35) * usr.workStreak));
//                            emb.WithDescription($"Вы не ходили на работу {DateTime.Now.Day - usr.LastWork.Day} дней. Ваши прогулы вычтены из зарплаты в размере {(ulong)usr.LastWork.Day * work.money} coin's").WithColor(255, 0, 94);
//                            usr.money -= (long)(Convert.ToUInt64(usr.LastWork.Day) * work.money);
//                            usr.LastWork = DateTime.Now.AddDays(1);
//                        }
//                    }
//                    else
//                    {
//                        usr.workStreak++;
//                        ulong amt = 500 + ((500 / 35) * usr.workStreak);
//                        usr.money += (long)amt;
//                        if (Math.Ceiling(Convert.ToDouble(usr.workStreak / 31)) > 0)
//                        {

//                            emb.WithDescription($"Вы ходили на работу {usr.workStreak % 31} месяца подрят, поэтому начальник выдал вам премию!\nПремия: 20% от зарплаты!");
//                            usr.money += (long)amt / 20;
//                        }
//                        else
//                        {
//                            emb.WithDescription($"Вы сходили на работу и получили {amt} Coin's!");
//                            int rnd = new PcgRandom(1488).Next(0, 1000);
//                            if (rnd <= 100)
//                            {
//                                int moneyrnd = new PcgRandom(1488).Next(300, 3000);
//                                usr.money += moneyrnd;
//                                emb.Description += $"\nВозвращаясь домой, на дороге вы нашли {moneyrnd} coin's";
//                            }
//                        }
//                        usr.LastWork = DateTime.Now.AddDays(1);
//                    }
//                    DBcontext.RG_Profile.Update(usr);
//                    await DBcontext.SaveChangesAsync();
//                }
//                else
//                {
//                    if ((usr.LastWork - DateTime.Now).TotalSeconds >= 3600)
//                        emb.WithDescription($"Дождитесь {(usr.LastWork - DateTime.Now).Hours} часов и {(usr.LastWork - DateTime.Now).Minutes} минут чтобы снова сходить на работу!");

//                    if ((usr.LastWork - DateTime.Now).TotalSeconds <= 3600)
//                        emb.WithDescription($"Дождитесь {((usr.LastWork - DateTime.Now).TotalSeconds > 60 ? $"{(usr.LastWork - DateTime.Now).Minutes} минут и " : "")} {(usr.LastWork - DateTime.Now).Seconds} секунд чтобы снова сходить на работу!");
//                }
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        public async Task RGshop(ulong itemid = 0)
//        {
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Магазин вещей");
//                int page = 0;
//                if (page == 0 || itemid == 0)
//                {
//                    var items = DBcontext.RG_Item.AsQueryable().Where(x => x.userid == Context.Client.CurrentUser.Id);
//                    if (items.Count() == 0) emb.WithDescription("В магазине пока нет вещей!");
//                    else
//                    {
//                        if (page > Math.Ceiling(Convert.ToDouble(items.Count()) / 5)) emb.WithDescription("404 Not Found. Вы зашли в темный район, тут никого нет.").WithFooter($"Страница { (page == 0 ? 1 : page)}/{ Math.Ceiling(Convert.ToDouble(items.Count()) / 5)}");
//                        else
//                        {
//                            await ListBuilder(page, items, emb, "RGshop");
//                        }
//                        emb.Footer.Text += $"Страница { (page == 0 ? 1 : page)}/{ Math.Ceiling(Convert.ToDouble(items.Count()) / 5)}";
//                    }
//                }
//                else
//                {
//                    var thisitem = DBcontext.RG_Item.FirstOrDefault(x => x.userid == Context.Client.CurrentUser.Id && x.itemid == itemid);
//                    if (thisitem != null)
//                    {
//                        var usr = await CreateUser(Context.User);
//                        if (usr.money >= (long)thisitem.NowPrice)
//                        {
//                            DBcontext.RG_Item.Add(new RussiaGame_Item() { guildid = Context.Guild.Id, userid = Context.User.Id, ItemName = thisitem.ItemName });
//                            await DBcontext.SaveChangesAsync();
//                            emb.WithDescription($"Вы успешно купили {thisitem.ItemName}");
//                        }
//                        else emb.WithDescription($"Вам не хватает {(long)thisitem.NowPrice - usr.money} для покупки {thisitem.ItemName}!");
//                    }
//                    else emb.WithDescription("Такой вещи нет в магазине.");
//                }
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//                _cache.Remove(_cache);
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        public async Task RGbazar(ulong itemid = 0)
//        {
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Барахолка");
//                int page = 0;
//                if (page == 0 || itemid == 0)
//                {
//                    var items = DBcontext.RG_Item.AsQueryable().Where(x => x.guildid == Context.Guild.Id && x.traded);
//                    if (items.Count() == 0) emb.WithDescription("В барахолке нет вещей!");
//                    else
//                    {
//                        if (page > Math.Ceiling(Convert.ToDouble(items.Count()) / 5)) emb.WithDescription("404 Not Found. Вы зашли в темный район, тут никого нет.").WithFooter($"Страница { (page == 0 ? 1 : page)}/{ Math.Ceiling(Convert.ToDouble(items.Count()) / 5)}");
//                        else
//                        {
//                            await ListBuilder(page, items, emb, "RGbazar");
//                        }
//                    }
//                }
//                else
//                {
//                    var thisitem = DBcontext.RG_Item.FirstOrDefault(x => x.guildid == Context.Guild.Id && x.traded && x.itemid == itemid);
//                    if (thisitem != null)
//                    {
//                        var usr = await CreateUser(Context.User);
//                        if (usr.money >= (long)thisitem.NowPrice)
//                        {
//                            thisitem.userid = Context.User.Id;
//                            thisitem.traded = false;
//                            thisitem.countTrade++;
//                            DBcontext.RG_Item.Update(thisitem);
//                            await DBcontext.SaveChangesAsync();
//                            emb.WithDescription($"Вы успешно купили {thisitem.ItemName}");
//                        }
//                        else emb.WithDescription($"Вам не хватает {(long)thisitem.NowPrice - usr.money} для покупки {thisitem.ItemName}!");
//                    }
//                    else emb.WithDescription("Такой вещи нет в барахолке.");
//                }
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//                _cache.Remove(_cache);
//            }
//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand, NotDBCommand]
//        public async Task RGbazaradd(ulong itemid)
//        {
//            _cache.Remove(_cache);
//            using (var DBcontext = new DBcontext())
//            {
//                var usr = await CreateUser(Context.User);
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - RGbazar add {Context.User}", Context.User.GetAvatarUrl());
//                var item = usr.UserItems.FirstOrDefault(x => x.itemid == itemid);
//                if (item != null)
//                {
//                    item.traded = true;
//                    DBcontext.RG_Item.Update(item);
//                    await DBcontext.SaveChangesAsync();
//                    emb.WithDescription($"Вы успешно выставили {item.ItemName} на продажу в барахолке!");
//                }
//                else
//                {
//                    var itemtrade = usr.UserItemsTraded.FirstOrDefault(x => x.itemid == itemid);
//                    if (itemtrade != null)
//                        emb.WithDescription("Ваша вещь уже находится в продаже");
//                    else
//                        emb.WithDescription("Данная вещь отсутствует у вас в инвентаре!");
//                }
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }

//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand, NotDBCommand]
//        public async Task RGbazardel(ulong itemid)
//        {
//            _cache.Remove(_cache);
//            using (var DBcontext = new DBcontext())
//            {
//                var usr = await CreateUser(Context.User);
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - RGbazar del {Context.User}", Context.User.GetAvatarUrl());
//                var itemtrade = usr.UserItemsTraded.FirstOrDefault(x => x.itemid == itemid);
//                if (itemtrade != null)
//                {
//                    itemtrade.traded = false;
//                    DBcontext.RG_Item.Update(itemtrade);
//                    await DBcontext.SaveChangesAsync();
//                    emb.WithDescription($"Вы успешно убрали {itemtrade.ItemName} с продажи в барахолке!");
//                }
//                else
//                {
//                    var item = usr.UserItems.FirstOrDefault(x => x.itemid == itemid);
//                    if (item != null)
//                        emb.WithDescription("Предмет уже находится в вашем инвентаре!");
//                    else emb.WithDescription("Предмет который вы хотите убрать с барахолки не является вашим, или вовсе не существует!");
//                }
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }

//        }

//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        public async Task RGtransfer(SocketUser user, uint money)
//        {
//            _cache.Remove(_cache);
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($" - RGtransfer {Context.User} 💵 {user}", Context.User.GetAvatarUrl());
//                if (user != Context.User)
//                {
//                    var you = await CreateUser(Context.User);
//                    var trades = await CreateUser(user);
//                    if (you.money >= money)
//                    {
//                        you.money -= money;
//                        trades.money += money;
//                        DBcontext.RG_Profile.Update(you);
//                        DBcontext.RG_Profile.Update(trades);
//                        emb.WithDescription($"Вы успешно перевели пользователю {user.Mention}, {money} coin's.");
//                        await DBcontext.SaveChangesAsync();
//                    }
//                    else emb.WithDescription($"У вас недостаточно средств для перевода. У вас {you.money} coin's");
//                }
//                else emb.WithDescription("Не, у нас так не принято, давай лучше другим, чем самому себе:)");
//                await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }

//        }
//    }
//}
