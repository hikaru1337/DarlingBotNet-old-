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
using DarlingBotNet.DataBase.Database;
using ServiceStack;

namespace DarlingBotNet.Modules
{
    public class Admins : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _discord;
        private readonly DbService _db;

        public Admins(DiscordSocketClient discord, DbService db, CommandService commands, IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _db = db;
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ban(IGuildUser user, uint DeleteMessageDays = 0, [Remainder] string reason = null)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94);
            if (DeleteMessageDays <= 7)
            {
                emb.WithDescription($"Пользователь {user.Mention} получил Ban{(reason != null ? $"\nПричина: {reason}" : "")}").WithAuthor($"{user.Mention} Banned");
                var chat = user.GetOrCreateDMChannelAsync();
                if (chat != null)
                {
                    emb.WithDescription($"Вы были забанены на сервере {Context.Guild.Name}{(reason != null ? $"\nПричина: {reason}" : "")}").WithAuthor($"Banned");
                    await user.SendMessageAsync("", false, emb.Build());
                }
            }
            else emb.WithDescription("Вы не можете удалить сообщения больше чем за 7 дней").WithAuthor("Ban Error");
            await Context.Channel.SendMessageAsync("", false, emb.Build());
            await user.Guild.AddBanAsync(user, (int)DeleteMessageDays, reason);
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
                emb.WithDescription($"Пользователь {usr.User.Username} был разбанен!");
                await Context.Guild.RemoveBanAsync(userid);
            }
            else emb.WithDescription("Пользователь не найден. Вы его точно банили?");
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task kick(IGuildUser user, [Remainder] string reason = null)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Kick");
            emb.WithDescription($"Пользователь {user.Mention} был кикнут{(reason != null ? $"\nПричина: {reason}" : "")}").WithAuthor($"{user.Mention} Kicked");
            var chat = user.GetOrCreateDMChannelAsync();
            if (chat != null)
            {
                emb.WithDescription($"Вы были кикнуты с сервера {Context.Guild.Name}{(reason != null ? $"\nПричина: {reason}" : "")}").WithAuthor($"Kicked");
                await user.SendMessageAsync("", false, emb.Build());
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
            await user.KickAsync(reason);
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task mute(IGuildUser user)
        {
            var role = await new SystemLoading(_discord, _db).CreateMuteRole(Context.Guild);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Mute");
            if (user.RoleIds.Where(x => x == role.voicemuterole && x == role.chatmuterole) != null)
                emb.WithDescription($"Пользователь {user.Mention} уже имеет Mute-Роли").WithAuthor($"{user.Mention} Muted?");
            else
            {
                emb.WithDescription($"Пользователь {user.Mention} получил мут").WithAuthor($"{user.Mention} Muted");
                var chat = user.GetOrCreateDMChannelAsync();
                if (chat != null)
                {
                    emb.WithDescription($"Вы были замучены на сервере {Context.Guild.Name}").WithAuthor($"Muted");
                    await user.SendMessageAsync("", false, emb.Build());
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
            var role = await new SystemLoading(_discord, _db).CreateMuteRole(Context.Guild);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("UnMute");
            if (user.RoleIds.Where(x => x == role.voicemuterole && x == role.chatmuterole) == null)
                emb.WithDescription($"Пользователь {user.Mention} не имеет Mute-Ролей").WithAuthor($"{user.Mention} UnMuted?");
            else
            {
                emb.WithDescription($"Пользователь {user.Mention} получил размут").WithAuthor($"{user.Mention} UnMuted");
                var chat = user.GetOrCreateDMChannelAsync();
                if (chat != null)
                {
                    emb.WithDescription($"Вы были размучены на сервере {Context.Guild.Name}").WithAuthor($"UnMuted");
                    await user.SendMessageAsync("", false, emb.Build());
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
            using (var DBcontext = _db.GetDbContext())
            {
                var role = await new SystemLoading(_discord, _db).CreateMuteRole(Context.Guild);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("TempMute");
                if (time < 720)
                {
                    var times = DateTime.Now.AddMinutes(time);
                    var xz = DBcontext.TempUser.GetOrCreate(Context.Guild.Id, user.Id, times, "TempMute");
                    await DBcontext.SaveChangesAsync();
                    emb.WithDescription($"Пользователь {user.Mention} получил мут");
                    bool DMclose = false;
                    try
                    {
                        emb.WithDescription($"Вы были замучены на сервере {Context.Guild.Name} на {time} минут").WithAuthor($"TempMuted");
                        await user.SendMessageAsync("", false, emb.Build());
                    }
                    catch (Exception)
                    {
                        DMclose = true;
                    }

                    await Context.Channel.SendMessageAsync("", false, emb.Build());
                    await Task.Delay(time * 60000);

                    if (DBcontext.TempUser.Get(xz.guildId, xz.userId, xz.ToTime, xz.Reason) != null)
                    {
                        DBcontext.TempUser.Remove(xz);
                        await DBcontext.SaveChangesAsync();
                        role = await new SystemLoading(_discord, _db).CreateMuteRole(Context.Guild);
                        if (user.RoleIds.Where(x => x == role.chatmuterole || x == role.voicemuterole) != null)
                        {
                            emb.WithDescription($"Пользователь {user.Mention} получил размут");
                            await user.RemoveRoleAsync(user.Guild.GetRole(role.chatmuterole));
                            await user.RemoveRoleAsync(user.Guild.GetRole(role.voicemuterole));
                            if(!DMclose)
                            { 
                                emb.WithDescription($"Вы были размучены на сервере {Context.Guild.Name}").WithAuthor($"TempUnMuted");
                                await user.SendMessageAsync("", false, emb.Build());
                            }
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
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Warn");
                var wrn = new WarnSystem(_db).WarnUser(user).Result;
                var usr = wrn.Item1;
                emb.Description += wrn.Item2.Description;
                await Context.Channel.SendMessageAsync("", false, emb.Build());
                var glds = DBcontext.Guilds.Get(user.Guild);
                var warn = DBcontext.Warns.Get(user.Guild.Id, usr.countwarns);
                if (warn != null)
                {
                    if (warn.ReportWarn == "kick")
                        await user.KickAsync();
                    else if (warn.ReportWarn == "ban")
                        await user.BanAsync();
                    else if (warn.ReportWarn == "mute" || warn.ReportWarn.Substring(0, 4) == "tban" || warn.ReportWarn.Contains("tmute"))
                    {

                        if (warn.ReportWarn == "mute" || warn.ReportWarn.Contains("tmute"))
                        {
                            glds = await new SystemLoading(_discord, _db).CreateMuteRole(Context.Guild);
                            await user.AddRoleAsync(user.Guild.GetRole(glds.chatmuterole));
                            await user.AddRoleAsync(user.Guild.GetRole(glds.voicemuterole));
                        }

                        if (warn.ReportWarn != "mute")
                        {
                            int minute = 0;
                            if (warn.ReportWarn.Contains("tban"))
                                minute = Convert.ToInt32(warn.ReportWarn.ToLower().Substring(4, warn.ReportWarn.ToLower().Length - 4));
                            else if (warn.ReportWarn.Contains("tmute"))
                                minute = Convert.ToInt32(warn.ReportWarn.ToLower().Substring(5, warn.ReportWarn.ToLower().Length - 5));

                            var time = DateTime.Now.AddMinutes(minute);
                            var xz = DBcontext.TempUser.GetOrCreate(Context.Guild.Id, user.Id, time, warn.ReportWarn);
                            await DBcontext.SaveChangesAsync();
                            if (warn.ReportWarn.Substring(0, 4) == "tban")
                            {
                                await user.BanAsync();
                                await Task.Delay(minute * 60000);
                                if (DBcontext.TempUser.Get(xz.guildId, xz.userId, xz.ToTime, xz.Reason) != null)
                                {
                                    DBcontext.TempUser.Remove(xz);
                                    await DBcontext.SaveChangesAsync();
                                    emb.WithDescription($"Пользователь {user.Mention} получил разбан");
                                }
                            }
                            else if (warn.ReportWarn.Substring(0, 5) == "tmute")
                            {
                                await Task.Delay(minute * 60000);
                                if (DBcontext.TempUser.Get(xz.guildId, xz.userId, xz.ToTime, xz.Reason) != null)
                                {
                                    DBcontext.TempUser.Remove(xz);
                                    await DBcontext.SaveChangesAsync();
                                    glds = await new SystemLoading(_discord, _db).CreateMuteRole(Context.Guild);
                                    if (user.Roles.Where(x => x.Id == glds.chatmuterole || x.Id == glds.voicemuterole) != null)
                                    {
                                        emb.WithDescription($"Пользователь {user.Mention} получил размут");
                                        await user.RemoveRoleAsync(user.Guild.GetRole(glds.chatmuterole));
                                        await user.RemoveRoleAsync(user.Guild.GetRole(glds.voicemuterole));
                                    }
                                }
                            }
                            await Context.Channel.SendMessageAsync("", false, emb.Build());
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
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("UnWarn");
                var wrn = new WarnSystem(_db).UnWarnUser(user).Result;
                uint i = wrn.Item1.countwarns++;
                var Warn = DBcontext.Warns.Get(user.Guild.Id, i);
                if (Warn != null)
                {
                    var glds = DBcontext.Guilds.Get(user.Guild);
                    if (Warn.ReportWarn == "ban")
                        await Context.Guild.RemoveBanAsync(user);
                    else if (Warn.ReportWarn == "mute")
                    {
                        glds = await new SystemLoading(_discord, _db).CreateMuteRole(Context.Guild);
                        await user.RemoveRoleAsync(user.Guild.GetRole(glds.chatmuterole));
                        await user.RemoveRoleAsync(user.Guild.GetRole(glds.voicemuterole));
                    }
                    else if (Warn.ReportWarn.Substring(0, 4) == "tban" || Warn.ReportWarn.Substring(0, 5) == "tmute")
                    {
                        var time = DBcontext.TempUser.Get(Context.Guild.Id).First(x => x.userId == user.Id && x.ToTime < DateTime.Now);
                        if (time != null)
                        {
                            DBcontext.TempUser.Remove(time);
                            await DBcontext.SaveChangesAsync();
                        }

                    }
                }
                emb.Description = wrn.Item2.Description;
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
            var emb = new EmbedBuilder().WithColor(255, 0, 94);
            var mes = MessageBuilder.EmbedUserBuilder(text);
            await channel.SendMessageAsync(mes.Item2, false, mes.Item1.Build());
        }

    }
}
