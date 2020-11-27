using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using DarlingBotNet.Services;
using DarlingBotNet.Services.Sys;
using DarlingBotNet.DataBase;
using Microsoft.Extensions.Caching.Memory;
using DarlingBotNet.DataBase.Database;

namespace DarlingBotNet.Modules
{
    public class Admins : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _provider;
        private readonly IMemoryCache _cache;
        public Admins(DiscordSocketClient discord, IServiceProvider provider, IMemoryCache cache)
        {
            _provider = provider;
            _discord = discord;
            _cache = cache;
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [PermissionHierarchy]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task ban(SocketGuildUser User, uint DeleteMessageDays = 0, [Remainder] string Reason = null)
        {
            _cache.Removes(Context);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"{User} ban");
            if (DeleteMessageDays <= 7)
            {
                    emb.WithDescription($"Пользователь {User.Mention} получил Ban{(Reason != null ? $"\nПричина: {Reason}" : "")}");
                    try
                    {
                        var embb = new EmbedBuilder().WithDescription($"Вы были забанены на сервере {Context.Guild.Name}{(Reason != null ? $"\nПричина: {Reason}" : "")}");
                        await User.SendMessageAsync("", false, embb.Build());
                        emb.Description += "\nСообщение было доставлено пользователю!";
                    }
                    catch (Exception)
                    {
                        emb.Description += "\nСообщение не было доставлено пользователю!";
                    }
                    await User.Guild.AddBanAsync(User, (int)DeleteMessageDays, Reason);
            }
            else emb.WithDescription("Вы не можете удалить сообщения больше чем за 7 дней").WithAuthor("Ban Error");
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task unban(ulong UserId)
        {
            _cache.Removes(Context);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"UnBan");
            try
            {
                var usr = await Context.Guild.GetBanAsync(UserId);
                emb.WithDescription($"Пользователь {usr.User.Username} был разбанен!").WithAuthor($"{usr} UnBan"); ;
                await Context.Guild.RemoveBanAsync(UserId);
            }
            catch (Exception)
            {
                emb.WithDescription("Пользователь не найден. Вы его точно банили?");
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [PermissionHierarchy]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task kick(SocketGuildUser User, [Remainder] string Reason = null)
        {
            _cache.Removes(Context);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"{User} Kicked", User.GetAvatarUrl());
                emb.WithDescription($"Пользователь {User.Mention} был кикнут{(Reason != null ? $"\nПричина: {Reason}" : "")}");
                try
                {
                    var embb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Вас кикнули",Context.Guild.IconUrl)
                                                 .WithDescription($"Вы были кикнуты с сервера {Context.Guild.Name}{(Reason != null ? $"\nПричина: {Reason}" : "")}");
                    await User.SendMessageAsync("", false, embb.Build());
                    emb.Description += "\nСообщение было доставлено пользователю!";
                }
                catch (Exception)
                {
                    emb.Description += "\nСообщение не было доставлено пользователю!";
                }
                await User.KickAsync(Reason);
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task mute(SocketGuildUser User)
        {
            var role = await OtherSettings.CreateMuteRole(Context.Guild);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"{User} Muted", User.GetAvatarUrl());
            var cmute = Context.Guild.GetRole(role.chatmuterole);
            var vmute = Context.Guild.GetRole(role.voicemuterole);
            if (User.Roles.Contains(cmute) || User.Roles.Contains(vmute))
                emb.WithDescription($"Пользователь {User.Mention} уже имеет Mute-Роли");
            else
            {
                var checkcmute = await OtherSettings.CheckRoleValid(User, cmute.Id, false);
                var checkvmute = await OtherSettings.CheckRoleValid(User, vmute.Id, false);
                if (checkcmute != null || checkvmute != null)
                {
                    emb.WithDescription(checkcmute == null ? checkvmute : checkcmute);
                    await Context.Channel.SendMessageAsync("", false, emb.Build());
                    return;
                }
                emb.WithDescription($"Пользователь {User.Mention} получил мут");
                try
                {
                    var embb = new EmbedBuilder().WithDescription($"Вы были замучены на сервере {Context.Guild.Name}");
                    await User.SendMessageAsync("", false, embb.Build());
                    emb.Description += "\nСообщение было доставлено пользователю!";
                }
                catch (Exception)
                {
                    emb.Description += "\nСообщение не было доставлено пользователю!";
                }
                
            }
            _cache.Removes(Context);
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task unmute(SocketGuildUser User)
        {
            var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("UnMute");
            var cmute = Context.Guild.GetRole(Guild.chatmuterole);
            var vmute = Context.Guild.GetRole(Guild.voicemuterole);
            if (User.Roles.Contains(cmute) || User.Roles.Contains(vmute))
                emb.WithDescription($"Пользователь {User.Mention} не имеет Mute-Ролей").WithAuthor($"{User} UnMuted?");
            else
            {
                emb.WithDescription($"Пользователь {User.Mention} получил размут").WithAuthor($"{User} UnMuted");
                try
                {
                    var embb = new EmbedBuilder().WithDescription($"Вы были размучены на сервере {Context.Guild.Name}").WithAuthor($"UnMuted");
                    await User.SendMessageAsync("", false, embb.Build());
                    emb.Description += "\nСообщение было доставлено пользователю!";
                }
                catch (Exception)
                {
                    emb.Description += "\nСообщение не было доставлено пользователю!";
                }
                await OtherSettings.CheckRoleValid(User, cmute.Id, true);
                await OtherSettings.CheckRoleValid(User, vmute.Id, true);
            }
            _cache.Removes(Context);
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task tempmute(SocketGuildUser User, ushort Minutes)
        {
            using (var DBcontext = new DBcontext())
            {
                _cache.Removes(Context);
                var role = await OtherSettings.CreateMuteRole(Context.Guild);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("TempMute");
                if (Minutes < 720)
                {
                    var times = DateTime.Now.AddMinutes(Minutes);
                    var xz = DBcontext.TempUser.Add(new TempUser() { GuildId = Context.Guild.Id, UserId = User.Id, ToTime = times, Reason = Warns.ReportType.tmute }).Entity;
                    await DBcontext.SaveChangesAsync();
                    var vrole = await OtherSettings.CheckRoleValid(User, role.voicemuterole,false);
                    var crole = await OtherSettings.CheckRoleValid(User, role.chatmuterole, false);
                    if (vrole != null || crole != null)
                    {
                        emb.WithDescription(vrole == null ? crole : vrole);
                        await Context.Channel.SendMessageAsync("", false, emb.Build());
                        return;
                    }
                    else
                        emb.WithDescription($"Пользователь {User.Mention} получил мут на {Minutes} минут");
                    bool DMclose = false;
                    var embb = new EmbedBuilder().WithDescription($"Вы были замучены на сервере {Context.Guild.Name} на {Minutes} минут").WithAuthor($"TempMuted");
                    try
                    {
                        await User.SendMessageAsync("", false, embb.Build());
                    }
                    catch (Exception)
                    {
                        DMclose = true;
                    }

                    await Context.Channel.SendMessageAsync("", false, emb.Build());
                    await Task.Delay(Minutes * 60000);
                    xz = DBcontext.TempUser.FirstOrDefault(x => x.GuildId == xz.GuildId && x.UserId == xz.UserId && x.ToTime == xz.ToTime && x.Reason == xz.Reason);
                    if (xz != null)
                    {
                        DBcontext.TempUser.Remove(xz);
                        await DBcontext.SaveChangesAsync();
                        if (User.Roles.Where(x => x.Id == role.chatmuterole || x.Id == role.voicemuterole) != null)
                        {
                            emb.WithDescription($"Пользователь {User.Mention} получил размут");
                            await OtherSettings.CheckRoleValid(User, role.chatmuterole, true);
                            await OtherSettings.CheckRoleValid(User, role.voicemuterole, true);

                            if (!DMclose)
                                await User.SendMessageAsync("", false, embb.Build());
                        }
                    }
                    else
                        return;

                }
                else emb.WithDescription("Время мута должно быть не больше 720 минут");

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionViolation]
        [PermissionHierarchy]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task warn(SocketGuildUser User)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Warn");

                var GuildWarnCount = DBcontext.Warns.AsQueryable().Count(x => x.GuildId == User.Guild.Id);
                var usr = _cache.GetOrCreateUserCache(User.Id, User.Guild.Id);
                _cache.Removes(Context);
                if (usr.countwarns >= GuildWarnCount)
                    usr.countwarns = 1;
                else
                    usr.countwarns++;

                DBcontext.Users.Update(usr);
                await DBcontext.SaveChangesAsync();

                emb.WithDescription($"Пользователь {User.Mention} получил {usr.countwarns} нарушение");


                var warn = DBcontext.Warns.FirstOrDefault(x => x.GuildId == User.Guild.Id && x.CountWarn == usr.countwarns);
                if (warn != null)
                {
                    var DateTimes = DateTime.Now.AddMinutes(warn.MinutesWarn);
                    TempUser xz = null;
                    Guilds glds = null;
                    bool success = false;
                    if (warn.ReportTypes == Warns.ReportType.kick)
                    {
                        await User.KickAsync();
                    }
                    else if (warn.ReportTypes == Warns.ReportType.ban || warn.ReportTypes == Warns.ReportType.tban)
                    {
                            if (warn.ReportTypes == Warns.ReportType.tban)
                            {
                                var usrtban = DBcontext.TempUser.AsQueryable().Where(x => x.UserId == Context.User.Id && x.GuildId == Context.Guild.Id && x.Reason == warn.ReportTypes);
                                DBcontext.TempUser.RemoveRange(usrtban);
                                xz = DBcontext.TempUser.Add(new TempUser() { GuildId = Context.Guild.Id, UserId = User.Id, ToTime = DateTimes, Reason = warn.ReportTypes }).Entity;
                                await DBcontext.SaveChangesAsync();
                                success = true;
                            }
                            await User.BanAsync();
                    }
                    else if (warn.ReportTypes == Warns.ReportType.mute || warn.ReportTypes == Warns.ReportType.tmute)
                    {
                        glds = await OtherSettings.CreateMuteRole(Context.Guild);
                        var cmute = await OtherSettings.CheckRoleValid(User, glds.chatmuterole, false);
                        var vmute = await OtherSettings.CheckRoleValid(User, glds.voicemuterole, false);

                        if (cmute != null || vmute != null)
                            emb.Description += $"\n{(cmute == null ? vmute : cmute)}";
                        else
                        {
                            var usrtban = DBcontext.TempUser.AsQueryable().Where(x => x.UserId == Context.User.Id && x.GuildId == Context.Guild.Id && x.Reason == warn.ReportTypes);
                            DBcontext.TempUser.RemoveRange(usrtban);
                            xz = DBcontext.TempUser.Add(new TempUser() { GuildId = Context.Guild.Id, UserId = User.Id, ToTime = DateTimes, Reason = warn.ReportTypes }).Entity;
                            await DBcontext.SaveChangesAsync();
                            success = true;
                        }
                    }

                    await Context.Channel.SendMessageAsync("", false, emb.Build());
                    if ((warn.ReportTypes == Warns.ReportType.tban || warn.ReportTypes == Warns.ReportType.tmute) && success)
                    {
                        await Task.Delay((int)warn.MinutesWarn * 60000);
                        xz = DBcontext.TempUser.FirstOrDefault(x => x.GuildId == xz.GuildId && x.ToTime == xz.ToTime && x.UserId == xz.UserId);
                        if (xz != null)
                        {
                            DBcontext.TempUser.Remove(xz);
                            await DBcontext.SaveChangesAsync();
                        }

                        success = false;
                        if (warn.ReportTypes == Warns.ReportType.tban)
                        {
                            var ban = await User.Guild.GetBanAsync(User);
                            if (ban != null)
                            {
                                await User.Guild.RemoveBanAsync(User);
                                success = true;
                                emb.WithDescription($"Пользователь {User.Mention} получил разбан");
                            }
                        }
                        else
                        {
                            glds = await OtherSettings.CreateMuteRole(Context.Guild);
                            if (User.Roles.Count(x => x.Id == glds.chatmuterole || x.Id == glds.voicemuterole) > 1)
                            {
                                emb.WithDescription($"Пользователь {User.Mention} получил размут");
                                success = true;

                                await OtherSettings.CheckRoleValid(User, glds.chatmuterole, true);
                                await OtherSettings.CheckRoleValid(User, glds.voicemuterole, true);

                            }
                        }

                        if (success)
                            await Context.Channel.SendMessageAsync("", false, emb.Build());
                    }

                }
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionViolation]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task unwarn(SocketGuildUser User)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("UnWarn");
                var usr = _cache.GetOrCreateUserCache(User.Id, User.Guild.Id);

                var warn = DBcontext.Warns.FirstOrDefault(x => x.GuildId == User.Guild.Id && x.CountWarn == usr.countwarns);
                if (warn != null)
                {
                    if (warn.ReportTypes == Warns.ReportType.ban || warn.ReportTypes == Warns.ReportType.tban)
                        await Context.Guild.RemoveBanAsync(User);
                    else if (warn.ReportTypes == Warns.ReportType.tmute || warn.ReportTypes == Warns.ReportType.mute)
                    {
                        var Guild = _cache.GetOrCreateGuldsCache(User.Guild.Id);
                        await OtherSettings.CheckRoleValid(User, Guild.chatmuterole, true);
                        await OtherSettings.CheckRoleValid(User, Guild.voicemuterole, true);
                    }


                    if (warn.ReportTypes == Warns.ReportType.tban || warn.ReportTypes == Warns.ReportType.tmute)
                    {
                        var time = DBcontext.TempUser.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.UserId == User.Id && x.ToTime < DateTime.Now);
                        if (time != null)
                        {
                            DBcontext.TempUser.Remove(time);
                            await DBcontext.SaveChangesAsync();
                        }
                    }
                }
                if (usr.countwarns != 0)
                {
                    emb.WithDescription($"У пользователя {User.Mention} снято {usr.countwarns} нарушение.");
                    usr.countwarns--;
                    DBcontext.Users.Update(usr);
                    await DBcontext.SaveChangesAsync();
                }
                else
                    emb.WithDescription($"У пользователя {User.Mention} нету нарушений.");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task clear(uint CountMessage)
        {
            _cache.Removes(Context);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Message Clear");

            if (CountMessage > 100)
            {
                emb.WithDescription("Удалить больше 100 сообщений нельзя!");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
            else
            {
                await Context.Message.DeleteAsync();
                var messages = await Context.Message.Channel.GetMessagesAsync((int)CountMessage).FlattenAsync();
                var result = messages.Where(x => x.CreatedAt >= DateTimeOffset.Now.AddHours(3).Subtract(TimeSpan.FromDays(14)));
                await ((ITextChannel)Context.Channel).DeleteMessagesAsync(result);
                emb.WithDescription($"Удалено {result.Count() + 1} сообщений").WithColor(255, 0, 94);
                var x = await Context.Channel.SendMessageAsync("", false, emb.Build());
                await Task.Delay(3000);
                await x.DeleteAsync();
            }

        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task userclear(SocketGuildUser User, uint CountMessage)
        {
            _cache.Removes(Context);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"{User} Message Clear");
            if (CountMessage > 100)
            {
                emb.WithDescription("Удалить больше 100 сообщений нельзя!");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
            else
            {
                await Context.Message.DeleteAsync();
                if (User == Context.User)
                    CountMessage++;
                var messages = await Context.Message.Channel.GetMessagesAsync((int)CountMessage).FlattenAsync();
                var result = messages.Where(x => x.Author.Id == User.Id && x.CreatedAt >= DateTimeOffset.Now.AddHours(3).Subtract(TimeSpan.FromDays(14)));
                emb.WithDescription($"Удалено {result.Count() + 1} сообщений от {User.Mention}").WithFooter("Сообщения больше 14 дней не удаляются!").WithColor(255, 0, 94);
                await (Context.Message.Channel as SocketTextChannel).DeleteMessagesAsync(result);
                var x = await Context.Channel.SendMessageAsync("", false, emb.Build());
                await Task.Delay(3000);
                await x.DeleteAsync();
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.SendMessages)]
        public async Task embedsay(SocketTextChannel TextChannel, [Remainder] string JsonText)
        {
            _cache.Removes(Context);
            var mes = MessageBuilder.EmbedUserBuilder(JsonText);
            if (mes.Item2 == "ERROR")
            {
                mes.Item2 = null;
                mes.Item1.Color = new Color(255, 0, 94);
                mes.Item1.WithAuthor("Ошибка!");
                mes.Item1.Description = "Неправильная конвертация в Json.\nПрочтите инструкцию! - [инструкция](https://docs.darlingbot.ru/commands/komandy-adminov/embedsay)";
                TextChannel = Context.Channel as SocketTextChannel;
            }
            await TextChannel.SendMessageAsync(mes.Item2, false, mes.Item1.Build());
        }

        //[Aliases, Commands, Usage, Descriptions]
        //[PermissionBlockCommand]
        //public async Task profile(SocketGuildUser user = null)
        //{
        //    if (user == null) user = Context.User as SocketGuildUser;
        //    var usr = DBcontext.Users.GetOrCreate(user.Id,Context.Guild.Id);


        //using (var img = new Image<Rgba32>(600, 1000))
        //{

        //    float xs = usr.XP - usr.Level * 80 * usr.Level;
        //    xs = xs / ((usr.Level + 1) * 80 * (usr.Level + 1) - (usr.Level * 80 * usr.Level)) * 100 * 6 - 600;

        //    var usertop = new EEF<Users>(new DBcontext()).Get(x => x.guildId == user.Guild.Id && x.XP >= usr.XP).Count();

        //    var userimg = Image.Load(new WebClient().DownloadData(user.GetAvatarUrl(ImageFormat.Jpeg, 640)));

        //    if (userimg.Height != 640)
        //        userimg.Mutate(x => x.Resize(600, 600));

        //    var Akrobat = new FontCollection().Install("UserProfile/Akrobat-Regular.ttf");
        //    var text = Akrobat.CreateFont(48, FontStyle.Regular);
        //    var textpoints = Akrobat.CreateFont(68, FontStyle.Regular);
        //    var point = new Vector2(0, 22.5f);
        //    if (user.Username.Length < 18)
        //    {
        //        text = Akrobat.CreateFont(55, FontStyle.Regular);
        //        point = new Vector2(0, 20);
        //    }
        //    else if (user.Username.Length > 19 && user.Nickname.Length < 26)
        //    {
        //        text = Akrobat.CreateFont(40, FontStyle.Regular);
        //        point = new Vector2(0, 28);
        //    }
        //    else if (user.Username.Length > 27)
        //    {
        //        text = Akrobat.CreateFont(35, FontStyle.Regular);
        //        point = new Vector2(0, 32);
        //    }

        //    img.Mutate(x => x.DrawImage(SixLabors.ImageSharp.Image.Load("UserProfile/Back.png"), 1)
        //                 .DrawImage(userimg, 0.6f, new Point(0, 0))
        //                 .DrawImage(SixLabors.ImageSharp.Image.Load("UserProfile/Third.png"), 1)
        //                 .DrawImage(SixLabors.ImageSharp.Image.Load("UserProfile/ThirdLevel.png"), 1, new Point((int)xs, 0))
        //                 .DrawText(new TextGraphicsOptions(true)
        //                 {
        //                     HorizontalAlignment = HorizontalAlignment.Center
        //                 }, usr.Level.ToString(), textpoints, new Rgba32(255, 255, 255), pointz(usr.Level.ToString(), 556))
        //                 .DrawText(new TextGraphicsOptions(true)
        //                 {
        //                     HorizontalAlignment = HorizontalAlignment.Center
        //                 }, usr.ZeroCoin.ToString(), textpoints, new Rgba32(255, 255, 255), pointz(usr.ZeroCoin.ToString(), 633))

        //                 .DrawText(new TextGraphicsOptions(true)
        //                 {
        //                     HorizontalAlignment = HorizontalAlignment.Center
        //                 }, usertop.ToString(), textpoints, new Rgba32(255, 255, 255), pointz(usertop.ToString(), 711))
        //                 .DrawText(new TextGraphicsOptions(true)
        //                 {
        //                     HorizontalAlignment = HorizontalAlignment.Center
        //                 }, Context.Guild.GetUser(usr.userid).JoinedAt.Value.UtcDateTime.ToString("dd.MM.yy"), textpoints, new Rgba32(255, 255, 255), pointz(Context.Guild.GetUser(usr.userid).JoinedAt.Value.UtcDateTime.ToString("dd.MM.yy"), 790))
        //                 .DrawText(new TextGraphicsOptions(true)
        //                 {
        //                     HorizontalAlignment = HorizontalAlignment.Center,
        //                     WrapTextWidth = img.Width
        //                 }, Regex.Replace("ПРОФИЛЬ " + user.Username.ToUpper(), @"\p{Cs}", ""), text, new Rgba32(255, 255, 255), point
        //                 ));


        //    await Context.Channel.SendFileAsync(img.ToStream(), "gg.png");
        //}
        //}

        //private static Vector2 pointz(string text, uint y)
        //{
        //    var point = new Vector2(550, y);
        //    if (text.Length >= 4 && text.Length <= 7)
        //        point = new PointF(510, y);
        //    else if (text.Length >= 8 && text.Length <= 12)
        //        point = new PointF(470, y);
        //    else if (text.Length >= 13 && text.Length <= 16)
        //        point = new PointF(440, y);
        //    return point;
        //}


    }
}
