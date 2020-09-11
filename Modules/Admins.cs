using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SixLabors.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;
using DarlingBotNet.Services;
using System.Numerics;
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
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ban(SocketGuildUser user, uint DeleteMessageDays = 0, [Remainder] string reason = null)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"{user.Mention} ban");
            if (DeleteMessageDays <= 7)
            {
                if (user.Hierarchy < Context.Guild.GetUser(Context.Client.CurrentUser.Id).Hierarchy)
                {
                    emb.WithDescription($"Пользователь {user.Mention} получил Ban{(reason != null ? $"\nПричина: {reason}" : "")}").WithAuthor($"{user.Mention} Banned");
                    try
                    {
                        var embb = new EmbedBuilder().WithDescription($"Вы были забанены на сервере {Context.Guild.Name}{(reason != null ? $"\nПричина: {reason}" : "")}").WithAuthor($"Banned",user.GetAvatarUrl());
                        await user.SendMessageAsync("", false, embb.Build());
                        emb.Description += "\nСообщение было доставлено пользователю!";
                    }
                    catch (Exception)
                    {
                        emb.Description += "\nСообщение не было доставлено пользователю!";
                    }
                    await user.Guild.AddBanAsync(user, (int)DeleteMessageDays, reason);
                }
                else emb.WithDescription($"Пользователь {user.Mention} имеет права выше бота!\nБот не может забанить пользователя");
            }
            else emb.WithDescription("Вы не можете удалить сообщения больше чем за 7 дней").WithAuthor("Ban Error");
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task unban(ulong userid)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("UnBan");
            var usr = await Context.Guild.GetBanAsync(userid);
            if (usr != null)
            {
                emb.WithDescription($"Пользователь {usr.User.Username} был разбанен!").WithAuthor($"{usr.User} UnBan");
                await Context.Guild.RemoveBanAsync(userid);
            }
            else emb.WithDescription("Пользователь не найден. Вы его точно банили?");
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task kick(SocketGuildUser user, [Remainder] string reason = null)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Kick");
            if (user.Hierarchy < Context.Guild.GetUser(Context.Client.CurrentUser.Id).Hierarchy)
            {
                emb.WithDescription($"Пользователь {user.Mention} был кикнут{(reason != null ? $"\nПричина: {reason}" : "")}").WithAuthor($"{user.Mention} Kicked", user.GetAvatarUrl());
                try
                {
                    var embb = new EmbedBuilder().WithDescription($"Вы были кикнуты с сервера {Context.Guild.Name}{(reason != null ? $"\nПричина: {reason}" : "")}").WithAuthor($"Kicked",user.GetAvatarUrl());
                    await user.SendMessageAsync("", false, embb.Build());
                    emb.Description += "\nСообщение было доставлено пользователю!";
                }
                catch (Exception)
                {
                    emb.Description += "\nСообщение не было доставлено пользователю!";
                }
                await user.KickAsync(reason);
            }
            else emb.WithDescription($"Пользователь {user.Mention} имеет права выше бота!\nБот не может кикнуть пользователя");
            await Context.Channel.SendMessageAsync("", false, emb.Build());
            
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task mute(IGuildUser user)
        {
            var role = await SystemLoading.CreateMuteRole(Context.Guild);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Mute");
            if (user.RoleIds.Where(x => x == role.voicemuterole && x == role.chatmuterole) != null)
                emb.WithDescription($"Пользователь {user.Mention} уже имеет Mute-Роли").WithAuthor($"{user.Mention} Muted?");
            else
            {
                emb.WithDescription($"Пользователь {user.Mention} получил мут").WithAuthor($"{user.Mention} Muted");
                try
                {
                    var embb = new EmbedBuilder().WithDescription($"Вы были замучены на сервере {Context.Guild.Name}").WithAuthor($"Muted", user.GetAvatarUrl());
                    await user.SendMessageAsync("", false, embb.Build());
                    emb.Description += "\nСообщение было доставлено пользователю!";
                }
                catch (Exception)
                {
                    emb.Description += "\nСообщение не было доставлено пользователю!";
                }
                await (user as SocketGuildUser).AddRoleAsync(Context.Guild.GetRole(role.chatmuterole));
                await (user as SocketGuildUser).AddRoleAsync(Context.Guild.GetRole(role.voicemuterole));
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task unmute(IGuildUser user)
        {
            var role = await SystemLoading.CreateMuteRole(Context.Guild);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("UnMute");
            if (user.RoleIds.Where(x => x == role.voicemuterole && x == role.chatmuterole) == null)
                emb.WithDescription($"Пользователь {user.Mention} не имеет Mute-Ролей").WithAuthor($"{user.Mention} UnMuted?");
            else
            {
                emb.WithDescription($"Пользователь {user.Mention} получил размут").WithAuthor($"{user.Mention} UnMuted");
                try
                {
                    var embb = new EmbedBuilder().WithDescription($"Вы были размучены на сервере {Context.Guild.Name}").WithAuthor($"UnMuted");
                    await user.SendMessageAsync("", false, embb.Build());
                    emb.Description += "\nСообщение было доставлено пользователю!";
                }
                catch (Exception)
                {
                    emb.Description += "\nСообщение не было доставлено пользователю!";
                }
                await (user as SocketGuildUser).RemoveRoleAsync(Context.Guild.GetRole(role.chatmuterole));
                await (user as SocketGuildUser).RemoveRoleAsync(Context.Guild.GetRole(role.voicemuterole));
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task tempmute(IGuildUser user, ushort time)
        {
            using (var DBcontext = new DBcontext())
            {
                var role = await SystemLoading.CreateMuteRole(Context.Guild);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("TempMute");
                if (time < 720)
                {
                    var times = DateTime.Now.AddMinutes(time);
                    var xz = DBcontext.TempUser.Add(new TempUser() { guildid = Context.Guild.Id,userId = user.Id, ToTime = times, Reason = "TempMute" }).Entity;
                    await DBcontext.SaveChangesAsync();
                    emb.WithDescription($"Пользователь {user.Mention} получил мут");
                    bool DMclose = false;
                    var embb = new EmbedBuilder().WithDescription($"Вы были замучены на сервере {Context.Guild.Name} на {time} минут").WithAuthor($"TempMuted");
                    try
                    {
                        await user.SendMessageAsync("", false, emb.Build());
                    }
                    catch (Exception)
                    {
                        DMclose = true;
                    }

                    await Context.Channel.SendMessageAsync("", false, emb.Build());
                    await Task.Delay(time * 60000);
                    xz = DBcontext.TempUser.FirstOrDefault(x=>x.guildid == xz.guildid && x.userId == xz.userId && x.ToTime == xz.ToTime && x.Reason == xz.Reason);
                    if (xz != null)
                    {
                        DBcontext.TempUser.Remove(xz);
                        await DBcontext.SaveChangesAsync();
                        role = await SystemLoading.CreateMuteRole(Context.Guild);
                        if (user.RoleIds.Where(x => x == role.chatmuterole || x == role.voicemuterole) != null)
                        {
                            emb.WithDescription($"Пользователь {user.Mention} получил размут");
                            await user.RemoveRoleAsync(user.Guild.GetRole(role.chatmuterole));
                            await user.RemoveRoleAsync(user.Guild.GetRole(role.voicemuterole));
                            if(!DMclose)
                                await user.SendMessageAsync("", false, embb.Build());
                        }
                    }
                    else return;

                }
                else emb.WithDescription("Время мута должно быть не больше 720 минут");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionViolation]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task warn(SocketGuildUser user)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Warn");
                //var wrn = new WarnSystem().WarnUser(user,_cache).Result;

                var glds = _cache.GetOrCreateGuldsCache(user.Guild.Id);
                var GuildWarnCount = DBcontext.Warns.AsQueryable().Count(x => x.guildid == user.Guild.Id);
                var usr = _cache.GetOrCreateUserCache(user.Id, user.Guild.Id);
                if (usr.countwarns >= 15 || usr.countwarns >= GuildWarnCount)
                    usr.countwarns = 1;
                else
                    usr.countwarns++;
                emb.WithDescription($"Пользователь {user.Mention} получил {usr.countwarns} нарушение");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
                //var usr = wrn.Item1;
                //emb.Description += wrn.Item2.Description;

                var warn = DBcontext.Warns.FirstOrDefault(x=>x.guildid == user.Guild.Id && x.CountWarn == usr.countwarns);
                if (warn != null)
                {
                    var rep = string.Concat(warn.ReportWarn.ToCharArray().Where(x => !(x >= 48 && x <= 57)));
                    var times = string.Concat(warn.ReportWarn.ToCharArray().Where(x => x >= 48 && x <= 57));
                    var time = Convert.ToInt32(times);
                    var DateTimes = DateTime.Now.AddMinutes(time);
                    TempUser xz = null;


                    if (rep == "tban" || rep == "tmute")
                    {
                        var usrtban = DBcontext.TempUser.AsQueryable().Where(x => x.userId == Context.User.Id && x.guildid == Context.Guild.Id && x.Reason.Contains(rep));
                        DBcontext.TempUser.RemoveRange(usrtban);
                        xz = DBcontext.TempUser.Add(new TempUser() { guildid = Context.Guild.Id, userId = user.Id, ToTime = DateTimes, Reason = warn.ReportWarn }).Entity;
                        await DBcontext.SaveChangesAsync();
                    }
                    if (rep == "kick")
                    {
                        if (user.Hierarchy < Context.Guild.GetUser(Context.Client.CurrentUser.Id).Hierarchy)
                            await user.KickAsync();
                        else emb.WithDescription($"Пользователь {user.Mention} имеет права выше бота!\nБот не может кикнуть пользователя");
                    }
                    else
                    {
                        if (rep == "ban" || rep == "tban")
                        {
                            if (user.Hierarchy < Context.Guild.GetUser(Context.Client.CurrentUser.Id).Hierarchy)
                                await user.BanAsync();
                            else emb.WithDescription($"Пользователь {user.Mention} имеет права выше бота!\nБот не может кикнуть пользователя");
                        }
                        else if (rep == "mute" || rep == "tmute")
                        {
                            glds = await SystemLoading.CreateMuteRole(Context.Guild);
                            await user.AddRoleAsync(user.Guild.GetRole(glds.chatmuterole));
                            await user.AddRoleAsync(user.Guild.GetRole(glds.voicemuterole));
                        }

                        if (rep == "tban" || rep == "tmute")
                        {
                            await Task.Delay(time * 60000);
                            if (DBcontext.TempUser.FirstOrDefault(x => x.guildid == xz.guildid && x.ToTime == xz.ToTime && x.userId == xz.userId) != null)
                            {
                                DBcontext.TempUser.Remove(xz);
                                await DBcontext.SaveChangesAsync();
                                bool success = false;
                                if (rep == "tban")
                                {
                                    var ban = await user.Guild.GetBanAsync(user);
                                    if (ban != null)
                                    {
                                        await user.Guild.RemoveBanAsync(user);
                                        success = true;
                                        emb.WithDescription($"Пользователь {user.Mention} получил разбан");
                                    }
                                }
                                else
                                {
                                    glds = await SystemLoading.CreateMuteRole(Context.Guild);
                                    if (user.Roles.Count(x => x.Id == glds.chatmuterole || x.Id == glds.voicemuterole) > 0)
                                    {
                                        emb.WithDescription($"Пользователь {user.Mention} получил размут");
                                        success = true;
                                        await user.RemoveRoleAsync(user.Guild.GetRole(glds.chatmuterole));
                                        await user.RemoveRoleAsync(user.Guild.GetRole(glds.voicemuterole));
                                    }
                                }
                                if (success)
                                    await Context.Channel.SendMessageAsync("", false, emb.Build());
                            }
                        }
                    }
                }
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionViolation]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task unwarn(SocketGuildUser user)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("UnWarn");
                var usr = _cache.GetOrCreateUserCache(user.Id, user.Guild.Id);
                var Warn = DBcontext.Warns.FirstOrDefault(x=>x.guildid == user.Guild.Id && x.CountWarn == usr.countwarns);
                if (Warn != null)
                {
                    var rep = string.Concat(Warn.ReportWarn.ToCharArray().Where(x => !(x >= 48 && x <= 57)));
                    if (rep == "ban")
                        await Context.Guild.RemoveBanAsync(user);
                    else if (rep == "tban" || rep == "tmute" || rep == "mute")
                    {
                        if(rep != "tban")
                        {
                            var glds = await SystemLoading.CreateMuteRole(Context.Guild);
                            await user.RemoveRoleAsync(user.Guild.GetRole(glds.chatmuterole));
                            await user.RemoveRoleAsync(user.Guild.GetRole(glds.voicemuterole));
                        }
                        else
                            await Context.Guild.RemoveBanAsync(user);
                        if (rep != "mute")
                        {
                            var time = DBcontext.TempUser.FirstOrDefault(x => x.guildid == Context.Guild.Id && x.userId == user.Id && x.ToTime < DateTime.Now);
                            if (time != null)
                            {
                                DBcontext.TempUser.Remove(time);
                                await DBcontext.SaveChangesAsync();
                            }
                        }
                    }
                }
                if (usr.countwarns != 0)
                {
                    emb.WithDescription($"У пользователя {user.Mention} снято {usr.countwarns} нарушение.");
                    usr.countwarns--;
                }
                else emb.WithDescription($"У пользователя {user.Mention} нету нарушений.");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task clear(uint MessageDelete = 0)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94);
            if (MessageDelete == 0)
            {
                emb.WithTitle("Ошибка").WithDescription("Введите количество сообщений которое требуется удалить.");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
            else
            {
                if (MessageDelete > 100) emb.WithDescription("Удалить больше 100 сообщений нельзя!");
                else
                {
                    await Context.Message.DeleteAsync();
                    var messages = await Context.Message.Channel.GetMessagesAsync((int)MessageDelete).FlattenAsync();
                    var result = messages.Where(x => x.CreatedAt >= DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(14)));
                    await ((ITextChannel)Context.Channel).DeleteMessagesAsync(result);
                    emb.WithTitle("Успех").WithDescription($"Удалено {result.Count() + 1} сообщений").WithColor(255, 0, 94);
                    var x = await Context.Channel.SendMessageAsync("", false, emb.Build());
                    await Task.Delay(3000);
                    await x.DeleteAsync();
                }
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task userclear(SocketGuildUser user, uint MessageDelete = 0)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94);
            if (MessageDelete == 0)
            {
                emb.WithTitle("Ошибка").WithDescription("Введите количество сообщений которое требуется удалить.");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
            else
            {
                await Context.Message.DeleteAsync();
                if (user == Context.User) MessageDelete++;
                var messages = await Context.Message.Channel.GetMessagesAsync((int)MessageDelete).FlattenAsync();
                var result = messages.Where(x => x.Author.Id == user.Id && x.CreatedAt >= DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(14)));
                emb.WithTitle("Успех").WithDescription($"Удалено {result.Count() + 1} сообщений от {user.Mention}").WithColor(255, 0, 94);
                await (Context.Message.Channel as SocketTextChannel).DeleteMessagesAsync(result);
                var x = await Context.Channel.SendMessageAsync("", false, emb.Build());
                await Task.Delay(3000);
                await x.DeleteAsync();
            }

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

        private static Vector2 pointz(string text, uint y)
        {
            var point = new Vector2(550, y);
            if (text.Length >= 4 && text.Length <= 7)
                point = new PointF(510, y);
            else if (text.Length >= 8 && text.Length <= 12)
                point = new PointF(470, y);
            else if (text.Length >= 13 && text.Length <= 16)
                point = new PointF(440, y);
            return point;
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task embedsay(SocketTextChannel channel, [Remainder] string text)
        {
            var mes = MessageBuilder.EmbedUserBuilder(text);
            await channel.SendMessageAsync(mes.Item2, false, mes.Item1.Build());
        }

    }
}
