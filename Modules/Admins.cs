using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Image = SixLabors.ImageSharp.Image;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DarlingBotNet.Services;
using System.IO;
using SixLabors.ImageSharp.Formats.Png;
using DarlingBotNet.DataBase;
using System.Numerics;

namespace DarlingBotNet.Modules
{
    public class Admins : ModuleBase<SocketCommandContext>
    {
        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionViolation]
        public async Task warns()
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Warns");
            var Guild = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id);
            if (Guild.ViolationSystem == 0) emb.WithDescription("На сервере не включена система нарушений.");
            else if (Guild.ViolationSystem == 1) emb.WithDescription("На сервере выбрана другая система нарушений");
            else
            {
                var warns = new EEF<Warns>(new DBcontext()).Get(x => x.guildId == Context.Guild.Id);
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
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionViolation]
        public async Task warn(SocketGuildUser user)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Warn");
            var wrn = WarnSystem.WarnUser(user).Result;
            var usr = wrn.Item1;
            emb.Description += wrn.Item2.Description;
            await Context.Channel.SendMessageAsync("", false, emb.Build());
            var glds = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == user.Guild.Id);
            var warn = new EEF<Warns>(new DBcontext()).GetF(x => x.guildId == user.Guild.Id && x.CountWarn == usr.countwarns);
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
                        glds = await SystemLoading.CreateMuteRole(Context.Guild);
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
                        var x = new EEF<TempUser>(new DBcontext()).Create(new TempUser() { guildId = Context.Guild.Id, userId = user.Id, ToTime = time, Reason = warn.ReportWarn });

                        if (warn.ReportWarn.Substring(0, 4) == "tban")
                        {
                            await user.BanAsync();
                            await Task.Delay(minute * 60000);
                            new EEF<TempUser>(new DBcontext()).Remove(x);
                            emb.WithDescription($"Пользователь {user.Mention} получил разбан");
                        }
                        else if (warn.ReportWarn.Substring(0, 5) == "tmute")
                        {
                            await Task.Delay(minute * 60000);
                            new EEF<TempUser>(new DBcontext()).Remove(x);
                            emb.WithDescription($"Пользователь {user.Mention} получил размут");
                            glds = await SystemLoading.CreateMuteRole(Context.Guild);
                            await user.RemoveRoleAsync(user.Guild.GetRole(glds.chatmuterole));
                            await user.RemoveRoleAsync(user.Guild.GetRole(glds.voicemuterole));
                        }
                        await Context.Channel.SendMessageAsync("", false, emb.Build());
                    }
                    
                }
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionViolation]
        public async Task unwarn(SocketGuildUser user)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("UnWarn");
            var wrn = WarnSystem.UnWarnUser(user).Result;
            var Warn = new EEF<Warns>(new DBcontext()).GetF(x => x.guildId == user.Guild.Id && x.CountWarn == ++wrn.Item1.countwarns);
            if (Warn != null)
            {
                var glds = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == user.Guild.Id);
                if (Warn.ReportWarn == "ban")
                    await Context.Guild.RemoveBanAsync(user);
                else if (Warn.ReportWarn == "mute")
                {
                    glds = await SystemLoading.CreateMuteRole(Context.Guild);
                    await user.RemoveRoleAsync(user.Guild.GetRole(glds.chatmuterole));
                    await user.RemoveRoleAsync(user.Guild.GetRole(glds.voicemuterole));
                }
                else if (Warn.ReportWarn.Substring(0, 4) == "tban" || Warn.ReportWarn.Substring(0, 5) == "tmute")
                {
                    var time = new EEF<TempUser>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id && x.userId == user.Id && x.ToTime < DateTime.Now);
                    if (time != null)
                        new EEF<TempUser>(new DBcontext()).Remove(time);
                }
            }
            emb.Description = wrn.Item2.Description;
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task clear(uint MessageDelete = 0)
        {
            var emb = new EmbedBuilder().WithColor(255,0,94);
            if (MessageDelete == 0)
            {
                emb.WithTitle("Ошибка").WithDescription("Введите количество сообщений которое требуется удалить.");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
            else
            {
                await Context.Message.DeleteAsync();
                await (Context.Message.Channel as SocketTextChannel).DeleteMessagesAsync(await Context.Message.Channel.GetMessagesAsync((int)MessageDelete).FlattenAsync());
                emb.WithTitle("Успех").WithDescription($"Удалено {MessageDelete + 1} сообщений").WithColor(255, 0, 94);
                var x = await Context.Channel.SendMessageAsync("", false, emb.Build());
                await Task.Delay(3000);
                await x.DeleteAsync();
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
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
                emb.WithTitle("Успех").WithDescription($"Удалено {MessageDelete + 1} сообщений").WithColor(255, 0, 94);
                await (Context.Message.Channel as SocketTextChannel).DeleteMessagesAsync(result);
                var x = await Context.Channel.SendMessageAsync("", false, emb.Build());
                await x.DeleteAsync();
            }
            
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task Profile(SocketGuildUser user = null)
        {
            if (user == null) user = Context.User as SocketGuildUser;
            var usr = SystemLoading.CreateUser(user).Result;
            using (var img = new Image<Rgba32>(600, 1000))
            {
                //float x3 = 398, x4 = 398;
                //int x1 = 59, x2 = 59;
                //int y1 = 306, y2 = 344, y3 = 344, y4 = 306;
                //x3 = (acc.LevelNumber + 1) * 80 * (acc.LevelNumber + 1);
                //x3 = (float)((((acc.XP - (acc.LevelNumber * 80 * acc.LevelNumber)) / x3) * 100) * 3.39) + 58;
                //x4 = x3;
                //var avatar = new WebClient().DownloadData(user.GetAvatarUrl(ImageFormat.Jpeg, 640));

                var userimg = Image.Load(new WebClient().DownloadData(user.GetAvatarUrl(ImageFormat.Jpeg,640)));

                if(userimg.Height != 640)
                    userimg.Mutate(x => x.Resize(600,600));

                var Akrobat = new FontCollection().Install("UserProfile/Akrobat-Regular.ttf");
                var text = Akrobat.CreateFont(48, FontStyle.Regular);
                var textpoints = Akrobat.CreateFont(68, FontStyle.Regular);
                var point = new Vector2(0, 22.5f);
                if(user.Username.Length < 18)
                {
                    text = Akrobat.CreateFont(55, FontStyle.Regular);
                    point = new Vector2(0, 20);
                }
                else if(user.Username.Length > 19 && user.Nickname.Length < 26)
                {
                    text = Akrobat.CreateFont(40, FontStyle.Regular);
                    point = new Vector2(0, 28);
                }
                else if (user.Username.Length > 27)
                {
                    text = Akrobat.CreateFont(35, FontStyle.Regular);
                    point = new Vector2(0, 32);
                }

                    img.Mutate(x => x.DrawImage(SixLabors.ImageSharp.Image.Load("UserProfile/Back.png"), 1)
                                 .DrawImage(userimg, 0.6f, new Point(0, 0))
                                 .DrawImage(SixLabors.ImageSharp.Image.Load("UserProfile/Third.png"), 1)
                                 .DrawText(new TextGraphicsOptions(true)
                                 {
                                     HorizontalAlignment = HorizontalAlignment.Center
                                 }, "1", textpoints, new Rgba32(255, 255, 255), new PointF(500,556))
                                 .DrawText(new TextGraphicsOptions(true)
                                 {
                                     HorizontalAlignment = HorizontalAlignment.Center,
                                     WrapTextWidth = img.Width
                                 }, Regex.Replace("ПРОФИЛЬ " + user.Username.ToUpper(), @"\p{Cs}", ""), text, new Rgba32(255, 255, 255), point
                                 ));

                
                await Context.Channel.SendFileAsync(img.ToStream(), "gg.png");
            }
        }

        
    }
}
