using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.Services;
using DarlingBotNet.Services.Sys;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DarlingBotNet.Modules
{
    public class Settings : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        private readonly DbService _db;

        public Settings(DiscordSocketClient discord, CommandService commands, DbService db, IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _db = db;
        } // Подключение компонентов

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task violationsystem()
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var Guild = DBcontext.Guilds.Get(Context.Guild);
                var embed = new EmbedBuilder().WithColor(255, 0, 94)
                                              .WithFooter($"Выбор системы - **{Guild.Prefix}ViolationSystemSet [WarnSystem/OFF]**\n" +
                                                          $"💤**OFF** - отключить систему");

                if (Guild.ViolationSystem == 0)

                    embed.WithAuthor("ViolationSystem - ⚠️ У вас не настроена репорт система!")
                        .WithDescription($"Для настройки выберите тип системы:\n" +
                        $"📕**WarnSystem** - выдача наказаний по количеству выданных предупреждений.\n" +
                        $"Не нуждается в точной настройке, варн дается за любое нарушенное правило\n\n" +
                        $"📒**ReportSystem** - находится в разработке...");

                else if (Guild.ViolationSystem == 1)

                    embed.WithAuthor("ViolationSystem - ReportSystem ✅")
                        .WithDescription("В разработке...");
                //.WithDescription($"Для настройки системы используйте:\n\n" +
                //$"**Добавить нарушение по правилу** - {Guild.Prefix}addrules [номер правила] [подправило] [нарушение - (ban,kick,mute,tmute)] [причина]\n" +
                //$"**Удалить нарушение по правилу** - {Guild.Prefix}delrules [номер правила] [подправило]\n" +
                //$"**Открыть правила** - {Guild.Prefix}rules\n\n" +
                //$"**Выдать нарушение** - {Guild.Prefix}report [user] [номер правила]\n" +
                //$"**Подправило** - система рассчитана на выдачу нарушений по 1 правилу несколько раз\n" +
                //$"Это так же как и Warn только несколько нарушений можно выставить на каждое правило.\n" +
                //$"`tmute - мут в минутах пример: tmute20`\n");

                else if (Guild.ViolationSystem == 2)

                    embed.WithAuthor("ViolationSystem - WarnSystem ✅")
                        .WithDescription($"Для настройки системы используйте:\n\n" +
                        $"**Выставить варны** - {Guild.Prefix}addwarn [Номер варна] [Варн] [причина]\n" +
                        $"**Удалить варн** - {Guild.Prefix}delwarn [номер]\n" +
                        $"**Выдать варн** - {Guild.Prefix}warn [USER]\n" +
                        $"**Снять варн с пользователя** - {Guild.Prefix}unwarn [USER] [Колво-варнов]\n" +
                        $"**Посмотреть варны** - {Guild.Prefix}warns\n\n" +
                        $"**Более подробно тут - [Инструкция](https://docs.darlingbot.ru/commands/settings-server/vybor-sistemy-narushenii)**");

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task violationsystemset(string System = "off")
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var glds = DBcontext.Guilds.Get(Context.Guild);
                var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨ReportSystemSet").WithFooter($"Для настройки напишите {glds.Prefix}violationsystem"); ;
                if (System.ToLower() == "off")
                {
                    glds.ViolationSystem = 0;
                    embed.WithDescription("✅ Репорт система выключена");
                }
                //else if (System.ToLower() == "reportsystem")
                //{
                //    glds.ViolationSystem = 1;
                //    embed.WithDescription("✅ Репорт система **reportsystem** включена");
                //}
                else if (System.ToLower() == "warnsystem")
                {
                    glds.ViolationSystem = 2;
                    embed.WithDescription("✅ Репорт система **warnsystem** включена");
                }
                else
                    embed.WithDescription("⚠️ репорт система не найдена!").WithFooter("Все возможные режимы - warnsystem,OFF");

                DBcontext.Guilds.Update(glds);
                await DBcontext.SaveChangesAsync();
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner, PermissionViolation]
        public async Task addwarn(ulong CountWarn, string report)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var Guild = DBcontext.Guilds.Get(Context.Guild);
                var warn = DBcontext.Warns.Get(Context.Guild.Id,CountWarn);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("AddWarn");

                if (CountWarn <= 15)
                {
                    var Text = SystemLoading.CheckText(report);
                    if (Text.Result.Item1)
                    {
                        if (warn != null)
                        {
                            emb.WithDescription($"Варн {CountWarn} был перезаписан с `{warn.ReportWarn}` на `{report}`.");
                            warn.ReportWarn = report;
                            DBcontext.Warns.Update(warn);
                        }
                        else
                        {
                            emb.WithDescription($"Варн {CountWarn} был успешно добавлен.");
                            var newwarn = new Warns() { guildId = Context.Guild.Id, CountWarn = CountWarn, ReportWarn = report };
                            DBcontext.Warns.Add(newwarn);
                            
                        }
                        emb.WithFooter($"Посмотреть все варны {Guild.Prefix}ws");
                        await DBcontext.SaveChangesAsync();
                    }
                    else
                    {
                        emb.Description += Text.Result.Item2.Description;
                        if (Text.Result.Item2.Footer != null)
                            emb.Footer = Text.Result.Item2.Footer;
                    }
                }
                else emb.WithFooter($"Количество варнов не может быть больше 15");

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner, PermissionViolation]
        public async Task delwarn(ulong CountWarn)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var Guild = DBcontext.Guilds.Get(Context.Guild);
                var warn = DBcontext.Warns.Get(Context.Guild.Id,CountWarn);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("AddWarn");
                if (warn != null)
                {
                    DBcontext.Warns.Remove(warn);
                    await DBcontext.SaveChangesAsync();
                    emb.WithDescription($"Варн с номером {CountWarn} успешно удален.");
                }
                else emb.WithDescription($"Варн с номером {CountWarn} отсутствует.");
                emb.WithFooter($"Посмотреть все варны {Guild.Prefix}ws");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task prefix(string prefix = null)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var Guild = DBcontext.Guilds.Get(Context.Guild);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Prefix");
                if (prefix == null) emb.WithDescription($"Префикс сервера - {Guild.Prefix}");
                else
                {
                    if (prefix.Length > 4) emb.WithDescription($"Префикс не может быть длиньше 4 символов");
                    else
                    {
                        emb.WithDescription($"Префикс сервера изменен с `{Guild.Prefix}` на `{prefix}`");
                        Guild.Prefix = prefix;
                        await DBcontext.SaveChangesAsync();
                    }
                }

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task commandinvise(string commandname = null)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var Guild = DBcontext.Guilds.Get(Context.Guild);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("CommandInvise - Отключенные команды");

                if (commandname == null)
                {
                    var CommandNameList = _commands.Commands;
                    bool tr = false;
                    if (Guild.CommandInviseList == null) tr = true;
                    else
                    {
                        string info = "";
                        foreach (var cmd in Guild.CommandInviseList)
                        {
                            var x = CommandNameList.Where(x => x.Aliases.First().ToLower() == cmd || x.Aliases.Skip(0).First().ToLower() == cmd).FirstOrDefault();
                            if (x != null) info += $"**{cmd}/{x.Name}** - {x.Summary.Split("||")[0]}";
                        }
                        if (info == "") tr = true;
                        else emb.WithDescription(info);

                    }
                    if (tr) emb.WithDescription("Команды еще не добавлены.").WithFooter($"Включить команду - {Guild.Prefix}CommandInvise [commandname]");
                }
                else
                {
                    var CommandName = _commands.Commands.Where(x => x.Aliases.First().ToLower() == commandname.ToLower() || x.Aliases.Skip(0).First().ToLower() == commandname.ToLower()).First();

                    if (CommandName == null)
                        emb.WithDescription("Такая команда не существует");
                    else if (BotSettings.CommandNotInvise.Where(x => x == CommandName.Name.ToLower()).Count() != 0)
                        emb.WithDescription("Эту команду нельзя отключить!");
                    else
                    {
                        List<string> liststring = Guild.CommandInviseList;

                        var Command = Guild.CommandInviseList.Where(x => x == commandname.ToLower()).FirstOrDefault();

                        emb.WithDescription($"Команда `{commandname}` {(Command == null ? "отключена" : "включена")}").WithFooter($"{(Command == null ? "Включить" : "Отключить")} команду - {Guild.Prefix}CommandInvise [commandname]");

                        if (Command == null) liststring.Add(commandname.ToLower());
                        else liststring.Remove(commandname.ToLower());

                        Guild.CommandInviseList = liststring;
                        DBcontext.Guilds.Update(Guild);
                        await DBcontext.SaveChangesAsync();
                    }
                }


                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task levelrole()
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var glds = DBcontext.Guilds.Get(Context.Guild);
                var embed = new EmbedBuilder().WithAuthor("🔨LevelRole - уровневые роли отсутствуют ⚠️").WithColor(255, 0, 94);
                string info = "";

                var lvl = DBcontext.LVLROLES.Get(Context.Guild);
                if (lvl.Count() != 0)
                {
                    embed.WithAuthor($"🔨LevelRole - уровневые роли");
                    lvl = lvl.OrderBy(u => u.countlvl);
                    foreach (var LVL in lvl)
                        info += $"{LVL.countlvl} уровень - {Context.Guild.GetRole(LVL.roleid).Mention}\n";
                }
                embed.AddField("Добавить роль", $"{glds.Prefix}lr.Add [ROLE] [LEVEL]");
                embed.AddField("Удалить роль", $"{glds.Prefix}lr.Del [ROLE]");

                await Context.Channel.SendMessageAsync("", false, embed.WithDescription($"{info}").Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task levelroleadd(SocketRole role, uint level)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var glds = DBcontext.Guilds.Get(Context.Guild);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨lr.Add");
                var lvlrole = DBcontext.LVLROLES.GetId(role.Id,Context.Guild.Id);
                if (lvlrole == null)
                {
                    var rolepos = Context.Guild.GetUser(Context.Client.CurrentUser.Id).Roles.FirstOrDefault(x => x.Position > role.Position);
                    if (rolepos != null)
                    {
                        emb.WithDescription($"Роль {role.Mention} выставлена за {level} уровень").WithFooter($"Посмотреть ваши уровневые роли {glds.Prefix}lr");
                        DBcontext.LVLROLES.GetOrCreate(role,level);
                        await DBcontext.SaveChangesAsync();
                    }
                    else emb.WithDescription("Роль бота ниже этой роли, из-за чего бот не сможет выдавать ее.").WithFooter("Поднимите роль бота выше выдаваемой роли.");
                }
                else if (lvlrole.roleid == role.Id) emb.WithDescription($"Роль {role.Mention} уже выдается за {lvlrole.countlvl} уровень");


                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task levelroledel(SocketRole role)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨lr.Del");
                var lvlrole = DBcontext.LVLROLES.Get(role);
                emb.WithDescription($"Уровневая роль {role.Mention} {(lvlrole != null ? "удалена" : "не является уровневой")}.");
                if (lvlrole != null)
                {
                    DBcontext.LVLROLES.Remove(lvlrole);
                    await DBcontext.SaveChangesAsync();
                }
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task channelsettings(SocketTextChannel channel = null, float number = 0)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94);
                var glds = DBcontext.Guilds.Get(Context.Guild);
                var chnl = new Channels();
                //if (channelz as SocketTextChannel == null)
                //{
                //    var channel = channelz as SocketVoiceChannel;
                //    emb.WithAuthor($"🔨ChannelSettings {(channel == null ? " " : $"- {channel.Name}")}");

                //    if (channel != null) chnl = new EEF<Channels>(new DBcontext()).GetF(x => x.guildid == Context.Guild.Id && x.channelid == channel.Id);

                //    if (channel != null && number == 0)
                //    {
                //        if (chnl != null)
                //            emb.AddField("1.Получение опыта", chnl.GiveXP.ToString().ToLower().Replace("true", "Вкл").Replace("false", "Выкл"), true);
                //        else 
                //            emb.WithDescription("Данный канал не найден!");
                //    }
                //    else if (channel != null && number != 0)
                //    {
                //        if (number >= 1 && number <= 1)
                //        {
                //            emb.WithDescription($"Получение уровней в {channel.Id} {(chnl.GiveXP == true ? "выключено" : "включены")}");
                //            chnl.GiveXP = !chnl.GiveXP;
                //            new EEF<Channels>(new DBcontext()).Update(chnl);
                //        }
                //        else emb.WithDescription("Номер может быть только 1.").WithFooter($"Подробнее - {glds.Prefix}cs [channel]");
                //    }
                //    else emb.WithDescription($"Введите нужный вам канал, пример - {glds.Prefix}cs [channel]");
                //}
                //else
                //{
                //var channel = channelz as SocketTextChannel;
                emb.WithAuthor($"🔨ChannelSettings {(channel == null ? " " : $"- {channel.Name}")}");
                if (channel != null) chnl = DBcontext.Channels.Get(channel);
                if (channel != null && number == 0)
                {
                    if (chnl != null)
                    {
                        emb.AddField("1 Получение опыта", chnl.GiveXP ? "Вкл" : "Выкл", true);
                        emb.AddField("2 Удалять ссылки", chnl.SendUrl ? "Вкл" : "Выкл", true);
                        if (chnl.SendUrl) emb.AddField("2,1 Удалять ссылки-изображения?", chnl.SendUrlImage ? "Вкл" : "Выкл", true);
                        if (chnl.SendUrl) emb.AddField("Url White List:", chnl.csUrlWhiteListList.Count == 0 ? "-" : string.Join(",", chnl.csUrlWhiteListList), true);
                        emb.AddField("3 Удалять КАПС сообщения", chnl.SendCaps ? "Вкл" : "Выкл", true);
                        emb.AddField("4 Плохие слова", chnl.SendBadWord ? "Вкл" : "Выкл", true);
                        if (chnl.SendBadWord) emb.AddField("Плохие слова:", chnl.BadWordList.Count == 0 ? "-" : string.Join(",", chnl.BadWordList), true);
                        emb.AddField("5 Использование команд", chnl.UseCommand ? "Вкл" : "Выкл", true);
                        if (!chnl.UseCommand) emb.AddField("5,1 Использование RP команд?", chnl.UseRPcommand ? "Вкл" : "Выкл", true);
                        emb.AddField("6 Спам [BETA]", chnl.Spaming ? "Вкл" : "Выкл", true);
                        emb.AddField("7 АнтиМат [Soon]", chnl.antiMat ? "Вкл" : "Выкл", true);
                        emb.AddField("8 Удалять приглашения(кроме тех что сюда)", chnl.InviteMessage ? "Вкл" : "Выкл", true);
                        emb.AddField("Номер Чата", chnl.channelid, true);
                        emb.WithFooter($"Вкл/Выкл опции канала - {glds.Prefix}ChannelSettings [channel] [number]");
                    }
                    else emb.WithDescription("Данный канал не найден!");
                }
                else if (channel != null && number != 0)
                {
                    if (number >= 1 && number <= 8)
                    {
                        switch (number)
                        {
                            case 1:
                                emb.WithDescription($"Получение уровней в {channel.Mention} {(chnl.GiveXP ? "выключено" : "включены")}");
                                chnl.GiveXP = !chnl.GiveXP;
                                break;
                            case 2:
                                emb.WithDescription($"Ссылки в {channel.Mention} {(chnl.SendUrl ? "не удаляются" : "удаляются")}");
                                chnl.SendUrl = !chnl.SendUrl;
                                if (chnl.SendUrl)
                                {
                                    emb.WithFooter("Вы можете дополнительно настроить ссылки! Откройте команду еще раз!");
                                    chnl.SendUrlImage = true;
                                }
                                else chnl.SendUrlImage = false;
                                break;
                            case 2.1f:
                                emb.WithDescription($"Ссылки-картинки в {channel.Mention} {(chnl.SendUrlImage ? "не удаляются" : "удаляются")}");
                                chnl.SendUrlImage = !chnl.SendUrlImage;
                                break;
                            case 3:
                                emb.WithDescription($"КАПС-сообщения {channel.Mention} теперь {(chnl.SendCaps ? "не удаляются" : "удаляются")}");
                                chnl.SendCaps = !chnl.SendCaps;
                                break;
                            case 4:
                                emb.WithDescription($"Плохие слова в {channel.Mention} теперь {(chnl.SendBadWord ? "не удаляются" : "удаляются")}");
                                chnl.SendBadWord = !chnl.SendBadWord;
                                emb.WithFooter($"Добавить/удалить плохие слова - {glds.Prefix}cs.bw [channel] [word]");
                                break;
                            case 5:
                                emb.WithDescription($"Команды в {channel.Mention} теперь {(chnl.UseCommand ? "выключены" : "включены")}");
                                chnl.UseCommand = !chnl.UseCommand;
                                if (chnl.UseCommand == false)
                                {
                                    emb.WithFooter("Вы можете дополнительно настроить команды! Откройте команду еще раз!");
                                    chnl.UseRPcommand = false;
                                }
                                else chnl.UseRPcommand = true;
                                break;
                            case 5.1f:
                                emb.WithDescription($"RP-команды в {channel.Mention} теперь {(chnl.UseRPcommand ? "выключены" : "включены")}");
                                chnl.UseRPcommand = !chnl.UseRPcommand;
                                break;
                            case 6:
                                emb.WithDescription($"Проверка на спам в {channel.Mention} {(chnl.Spaming ? "выключена" : "включена")}");
                                chnl.Spaming = !chnl.Spaming;
                                emb.WithFooter("Спам Больше 4 похожих сообщений в диапазоне 5 секунд");
                                break;
                            case 7:
                                emb.WithDescription($"Мат в {channel.Mention} теперь {(chnl.antiMat ? "не удаляется" : "удаляется")}");
                                chnl.antiMat = !chnl.antiMat;
                                break;
                            case 8:
                                emb.WithDescription($"Приглашения на другие сервера в {channel.Mention} теперь {(chnl.InviteMessage == true ? "не удаляется" : "удаляется")}");
                                chnl.InviteMessage = !chnl.InviteMessage;
                                break;
                            default:
                                emb.WithDescription($"Команда с таким номером не найдена!");
                                break;
                        }
                        DBcontext.Channels.Update(chnl);
                        await DBcontext.SaveChangesAsync();
                    }
                    else emb.WithDescription("Номер может быть от 1 до 8.").WithFooter($"Подробнее - {glds.Prefix}cs [channel]");
                }
                else emb.WithDescription($"Введите нужный вам канал, пример - {glds.Prefix}cs [channel]");
                //}

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task channelsettingsbadword(SocketGuildChannel channel, string word)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                Guilds glds = DBcontext.Guilds.Get(Context.Guild);
                var emb = new EmbedBuilder();
                Channels chnl = DBcontext.Channels.Get(channel);
                
                if (chnl.SendBadWord)
                {
                    List<string> badlist = chnl.BadWordList;
                    bool es = false;
                    if (chnl.BadWordList.Count != 0)
                    {
                        if (chnl.BadWordList.FirstOrDefault(x => x == word) != null)
                        {
                            emb.WithDescription($"Слово {word} удалено из списока");
                            badlist.Remove(word);
                        }
                        else es = true;
                    }
                    else es = true;

                    if (es)
                    {
                        emb.WithDescription($"Слово {word} включено в список");
                        badlist.Add(word);
                    }

                    chnl.BadWordList = badlist;
                    DBcontext.Channels.Update(chnl);
                    await DBcontext.SaveChangesAsync();
                }
                else emb.WithDescription($"Вы не включили проверку Плохих слов\n{glds.Prefix}cs {channel.Name} [4]");
                await Context.Channel.SendMessageAsync("", false, emb.WithColor(255, 0, 94).WithAuthor("🔨cs.badword")
                                                                     .WithFooter($"Добавить/Удалить - {glds.Prefix}cs.bw {channel.Name} [слово]").Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task channelsettingsgivexp()
        {
            using (var DBcontext = _db.GetDbContext())
            {
                Guilds glds = DBcontext.Guilds.Get(Context.Guild);
                glds.GiveXPnextChannel = !glds.GiveXPnextChannel;
                DBcontext.Guilds.Update(glds);
                await DBcontext.SaveChangesAsync();
                string info = $"В дальнейшем в созданных каналах, опыт {(glds.GiveXPnextChannel ? "" : "не ")}будет получаться";
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨cs.givexp").WithDescription($"{info}").Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task channelsettingsurlwhitelist(SocketGuildChannel channel, string url)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                Guilds glds = DBcontext.Guilds.Get(Context.Guild);
                var emb = new EmbedBuilder();
                Channels chnl = DBcontext.Channels.Get(channel);

                if (chnl.SendUrl)
                {
                    List<string> UrlWhiteList = chnl.csUrlWhiteListList;
                    bool es = false;
                    if (chnl.csUrlWhiteListList.Count != 0)
                    {
                        if (chnl.csUrlWhiteListList.FirstOrDefault(x => x == url) != null)
                        {
                            emb.WithDescription($"Url {url} удалено из White list");
                            UrlWhiteList.Remove(url);
                        }
                        else es = true;
                    }
                    else es = true;

                    if (es)
                    {
                        emb.WithDescription($"Url {url} включено в White list");
                        UrlWhiteList.Add(url);
                    }

                    chnl.csUrlWhiteListList = UrlWhiteList;
                    DBcontext.Channels.Update(chnl);
                    await DBcontext.SaveChangesAsync();
                }
                else emb.WithDescription($"Вы не включили Белый лист ссылок!\n{glds.Prefix}cs {channel.Name} [4]");
                await Context.Channel.SendMessageAsync("", false, emb.WithColor(255, 0, 94).WithAuthor("🔨cs.UrlWhiteList")
                                                                     .WithFooter($"Добавить/Удалить - {glds.Prefix}cs.uwl #{channel.Name} [url]").Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task logsettings(uint selection = 0, SocketTextChannel channel = null)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                Guilds Guild = DBcontext.Guilds.Get(Context.Guild);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - LogsServer", Context.Guild.IconUrl);
                if (selection == 0 && channel == null)
                {
                    emb.AddField("1.Бан пользователя", (Context.Guild.GetChannel(Guild.banchannel) != null ? (Context.Guild.GetChannel(Guild.banchannel) as SocketTextChannel).Mention : "Канал не указан"), true);
                    emb.AddField("2.Разбан пользователя", (Context.Guild.GetChannel(Guild.unbanchannel) != null ? (Context.Guild.GetChannel(Guild.unbanchannel) as SocketTextChannel).Mention : "Канал не указан"), true);
                    emb.AddField("3.Кик пользователя", (Context.Guild.GetChannel(Guild.kickchannel) != null ? (Context.Guild.GetChannel(Guild.kickchannel) as SocketTextChannel).Mention : "Канал не указан"), true);
                    emb.AddField("4.Вход пользователя", (Context.Guild.GetChannel(Guild.joinchannel) != null ? (Context.Guild.GetChannel(Guild.joinchannel) as SocketTextChannel).Mention : "Канал не указан"), true);
                    emb.AddField("5.Выход пользователя", (Context.Guild.GetChannel(Guild.leftchannel) != null ? (Context.Guild.GetChannel(Guild.leftchannel) as SocketTextChannel).Mention : "Канал не указан"), true);
                    emb.AddField("6.Удаленные сообщения", (Context.Guild.GetChannel(Guild.mesdelchannel) != null ? (Context.Guild.GetChannel(Guild.mesdelchannel) as SocketTextChannel).Mention : "Канал не указан"), true);
                    emb.AddField("7.Измененные сообщения", (Context.Guild.GetChannel(Guild.meseditchannel) != null ? (Context.Guild.GetChannel(Guild.meseditchannel) as SocketTextChannel).Mention : "Канал не указан"), true);
                    emb.AddField("8.Действия пользователей в голосовых чатах", (Context.Guild.GetChannel(Guild.voiceUserActions) != null ? (Context.Guild.GetChannel(Guild.voiceUserActions) as SocketTextChannel).Mention : "Канал не указан"), true);
                    emb.WithFooter($"Включить - {Guild.Prefix}LogSettings [цифра] [канал для сообщений]\nОтключить - {Guild.Prefix}LogSettings [цифра]");
                }
                else
                {
                    if (!(selection >= 1 && selection <= 8)) emb.WithDescription($"Выбор может быть только от 1 до8").WithFooter($"Подробнее - {Guild.Prefix}LogSettings");
                    else
                    {
                        string whatz = "";
                        ulong func = 0;
                        switch (selection)
                        {
                            case 1:
                                whatz = "банах";
                                func = Guild.banchannel;
                                if (func == 0 && channel != null) Guild.banchannel = channel.Id;
                                else if (func != 0) Guild.banchannel = 0;
                                break;
                            case 2:
                                whatz = "разбанах";
                                func = Guild.unbanchannel;
                                if (func == 0 && channel != null) Guild.unbanchannel = channel.Id;
                                else if (func != 0) Guild.unbanchannel = 0;
                                break;
                            case 3:
                                whatz = "киках";
                                func = Guild.kickchannel;
                                if (func == 0 && channel != null) Guild.kickchannel = channel.Id;
                                else if (func != 0) Guild.kickchannel = 0;
                                break;
                            case 4:
                                whatz = "входах пользователей";
                                func = Guild.joinchannel;
                                if (func == 0 && channel != null) Guild.joinchannel = channel.Id;
                                else if (func != 0) Guild.joinchannel = 0;
                                break;
                            case 5:
                                whatz = "Выходах пользователей";
                                func = Guild.leftchannel;
                                if (func == 0 && channel != null) Guild.leftchannel = channel.Id;
                                else if (func != 0) Guild.leftchannel = 0;
                                break;
                            case 6:
                                whatz = "Измененных сообщениях";
                                func = Guild.meseditchannel;
                                if (func == 0 && channel != null) Guild.meseditchannel = channel.Id;
                                else if (func != 0) Guild.meseditchannel = 0;
                                break;
                            case 7:
                                whatz = "Удаленных сообщениях";
                                func = Guild.mesdelchannel;
                                if (func == 0 && channel != null) Guild.mesdelchannel = channel.Id;
                                else if (func != 0) Guild.mesdelchannel = 0;
                                break;
                            case 8:
                                whatz = "действиях пользователей";
                                func = Guild.voiceUserActions;
                                if (func == 0 && channel != null) Guild.voiceUserActions = channel.Id;
                                else if (func != 0) Guild.voiceUserActions = 0;
                                break;
                        }
                        DBcontext.Guilds.Update(Guild);
                        await DBcontext.SaveChangesAsync();
                        if (func == 0)
                        {
                            if (channel == null) emb.WithDescription($"Функция и так отключена.").WithFooter($"Хотите включить уведомления? Тогда напишите и ознакомьтесь с {Guild.Prefix}LogSettings");
                            else emb.WithDescription($"В канал {channel.Mention} будут приходить сообщения о {whatz}");
                        }
                        else
                        {
                            if (Context.Guild.GetChannel(func) == null) emb.WithDescription($"В удаленный канал сообщения о {whatz} приходить не будут");
                            else emb.WithDescription($"В {(Context.Guild.GetChannel(func) as SocketTextChannel).Mention} не будут приходить сообщения о {whatz}");
                        }
                    }
                }
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task messagesettings(uint selection = 0, [Remainder] string text = null)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                Guilds Guild = DBcontext.Guilds.Get(Context.Guild);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - MessageSettings", Context.Guild.IconUrl);
                ulong point = 1;
                if (selection == 0 && text == null)
                {
                    emb.AddField($"{point}.Сообщение при входе [text]", Guild.WelcomeMessage != null ? $"Сообщение составлено" : "Отсутствует", true);
                    point++;
                    emb.AddField($"{point}.Личное сообщение при входе [text]", Guild.WelcomeDMmessage != null ? $"Сообщение составлено" : "Отсутствует", true);
                    point++;
                    if (Guild.WelcomeMessage != null)
                    {
                        emb.AddField($"{point}.Канал для Сообщений при входе [channel]", Guild.WelcomeChannel != 0 ? Context.Guild.GetTextChannel(Guild.WelcomeChannel).Mention : "Отсутствует", true);
                        point++;
                        emb.AddField($"{point}.Роль при входе [role]", Guild.WelcomeRole != 0 ? Context.Guild.GetRole(Guild.WelcomeRole).Mention : "Отсутствует", true);
                        point++;
                    }
                    emb.AddField($"{point}.Сообщение при выходе [text]", Guild.LeaveMessage != null ? $"Сообщение составлено" : "Отсутствует", true);
                    point++;
                    if (Guild.LeaveMessage != null)
                        emb.AddField($"{point}.Канал для Сообщений при выходе [channel]", Guild.LeaveChannel != 0 ? Context.Guild.GetTextChannel(Guild.LeaveChannel).Mention : "Отсутствует", true);

                    emb.WithFooter($"Включить - {Guild.Prefix}ms [цифра] [канал/text/role]\nВыключить - {Guild.Prefix}ms [цифра]\ntext - скопируйте json код с сайта embed.discord-bot.net\n%user% - используйте чтобы упомянуть пользователя");
                }
                else
                {
                    point = 3;
                    if (Guild.LeaveMessage != null) point++;
                    if (Guild.WelcomeMessage != null) point += 2;
                    if (!(selection >= 1 && selection <= point)) emb.WithDescription($"Выбор может быть только от 1 до {point}").WithFooter($"Подробнее - {Guild.Prefix}ms");
                    else
                    {
                        switch (selection)
                        {
                            case 1:
                                if (text != null)
                                {
                                    var embed = MessageBuilder.EmbedUserBuilder(text).Item1;
                                    if (embed.Description == null || embed.Color == null)
                                        emb.WithDescription($"Сообщение составлено не верно.").WithFooter("скопируйте json код с сайта embed.discord-bot.net");
                                    else
                                    {
                                        emb.WithDescription($"EmbedVisualizer составил ваше сообщение.").WithFooter("Для отображение этого сообщения укажите канал - h.ms 3 [channel]");
                                        Guild.WelcomeMessage = text;
                                    }
                                }
                                else
                                {
                                    if (Guild.WelcomeMessage == null) emb.WithDescription($"Введите текст для включения сообщения при входе").WithFooter($"Подробнее - {Guild.Prefix}ms");
                                    else
                                    {
                                        emb.WithDescription($"Сообщение при входе выключено");
                                        Guild.WelcomeMessage = null;
                                    }
                                }
                                break;
                            case 2:
                                if (text != null)
                                {
                                    var embed = MessageBuilder.EmbedUserBuilder(text).Item1;
                                    if (embed == null) emb.WithDescription($"Сообщение составлено не верно.").WithFooter("скопируйте json код с сайта embed.discord-bot.net");
                                    else
                                    {
                                        emb.WithDescription($"EmbedVisualizer-cообщение будет отправляться пользователю при входе на сервер.").WithFooter("Если он не отключил возможность отправлять ему сообщения");
                                        Guild.WelcomeDMmessage = text;
                                    }
                                }
                                else
                                {
                                    if (Guild.WelcomeDMmessage == null) emb.WithDescription($"Введите текст для Включения Личного сообщения при входе").WithFooter($"Подробнее - {Guild.Prefix}ms");
                                    else
                                    {
                                        emb.WithDescription($"Личное сообщение при входе выключено");
                                        Guild.WelcomeDMmessage = null;
                                    }
                                }
                                break;
                            case 3:
                                if (Guild.WelcomeMessage != null)
                                {
                                    if (text != null)
                                    {
                                        try
                                        {
                                            var x = Context.Guild.GetTextChannel(Convert.ToUInt64(text.Split("<#")[1].Split(">")[0]));
                                            emb.WithDescription($"В канал {x.Mention} буду приходить сообщения о входе пользователей.");
                                            Guild.WelcomeChannel = x.Id;
                                        }
                                        catch (Exception)
                                        {
                                            emb.WithDescription($"Введенный канал не найден. Укажите канал в таком формате: {Context.Guild.SystemChannel.Mention}");
                                        }
                                    }
                                    else
                                    {
                                        if (Guild.WelcomeChannel == 0) emb.WithDescription($"Введите канал для отправки сообщений о входе пользователей").WithFooter($"Подробнее - {Guild.Prefix}ms");
                                        else
                                        {
                                            emb.WithDescription($"В канал {Context.Guild.GetTextChannel(Guild.WelcomeChannel).Mention} не будут приходить сообщения о входе");
                                            Guild.WelcomeChannel = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    if (text != null)
                                    {
                                        emb.WithDescription($"Сообщение при выходе включено.\nText: `{text}`").WithFooter($"Для отображение этого сообщения укажите канал - h.ms {(Guild.WelcomeMessage != null ? "6" : "4")} [channel]");
                                        Guild.LeaveMessage = text;
                                    }
                                    else
                                    {
                                        if (Guild.LeaveMessage == null) emb.WithDescription($"Введите текст для включения сообщения при выходе").WithFooter($"Подробнее - {Guild.Prefix}ms");
                                        else
                                        {
                                            emb.WithDescription($"Сообщение при выходе выключено");
                                            Guild.LeaveMessage = null;
                                        }
                                    }
                                }
                                break;
                            case 4:
                                if (Guild.WelcomeMessage != null)
                                {
                                    if (text != null)
                                    {
                                        try
                                        {
                                            var x = Context.Guild.GetRole(Convert.ToUInt64(text.Split("<@&")[1].Split(">")[0]));
                                            emb.WithDescription($"Новый пользователям будет выдаваться роль {x.Mention}.");
                                            Guild.WelcomeRole = x.Id;
                                        }
                                        catch (Exception)
                                        {
                                            emb.WithDescription($"Введенная роль не найдена. Укажите роль в таком формате: {Context.Guild.EveryoneRole.Mention}").WithFooter("Не бойтесь, ни один ваш любимчик не был потревожен.");
                                        }
                                    }
                                    else
                                    {
                                        if (Guild.WelcomeRole == 0) emb.WithDescription($"Введите роль для выдачи ее пользователям").WithFooter($"Подробнее - {Guild.Prefix}ms");
                                        else
                                        {
                                            emb.WithDescription($"Роль {Context.Guild.GetRole(Guild.WelcomeRole).Mention} не будет выдаваться пользователям");
                                            Guild.WelcomeRole = 0;
                                        }
                                    }
                                }
                                else if (Guild.LeaveMessage != null)
                                {
                                    if (text != null)
                                    {
                                        try
                                        {
                                            var x = Context.Guild.GetTextChannel(Convert.ToUInt64(text.Split("<#")[1].Split(">")[0]));
                                            emb.WithDescription($"В канал {x.Mention} буду приходить сообщения о выходе пользователей.");
                                            Guild.LeaveChannel = x.Id;
                                        }
                                        catch (Exception)
                                        {
                                            emb.WithDescription($"Введенный канал не найден. Укажите канал в таком формате: {Context.Guild.SystemChannel.Mention}");
                                        }
                                    }
                                    else
                                    {
                                        if (Guild.LeaveChannel == 0) emb.WithDescription($"Введите канал для получения сообщений о выходе пользователей").WithFooter($"Подробнее - {Guild.Prefix}ms");
                                        else
                                        {
                                            emb.WithDescription($"В канал {Context.Guild.GetTextChannel(Guild.WelcomeChannel).Mention} не будут приходить сообщения о выходе");
                                            Guild.LeaveChannel = 0;
                                        }
                                    }
                                }
                                break;
                            case 5:
                                if (Guild.WelcomeMessage != null)
                                {
                                    if (text == null)
                                    {
                                        if (Guild.LeaveMessage == null) emb.WithDescription($"Введите текст для включения сообщения при выходе").WithFooter($"Подробнее - {Guild.Prefix}ms");
                                        else
                                        {
                                            emb.WithDescription($"Сообщение при выходе выключено");
                                            Guild.LeaveMessage = null;
                                        }
                                    }
                                    else
                                    {
                                        var embed = MessageBuilder.EmbedUserBuilder(text).Item1;
                                        if (embed == null) emb.WithDescription($"Сообщение составлено не верно.").WithFooter("скопируйте json код с сайта embed.discord-bot.net");
                                        else
                                        {
                                            emb.WithDescription($"EmbedVisualizer сообщение при выходе включено.").WithFooter($"Для отображение этого сообщения укажите канал - h.ms [{(Guild.WelcomeMessage != null ? "6" : "4")}] [channel]");
                                            Guild.LeaveMessage = text;
                                        }
                                    }
                                }
                                break;
                            case 6:
                                if (Guild.LeaveMessage != null && Guild.WelcomeMessage != null)
                                {
                                    if (text == null)
                                    {
                                        if (Guild.WelcomeChannel == 0) emb.WithDescription($"Введите канал для получения сообщений о выходе пользователей").WithFooter($"Подробнее - {Guild.Prefix}ms");
                                        else
                                        {
                                            emb.WithDescription($"В канал {Context.Guild.GetTextChannel(Guild.WelcomeChannel).Mention} не будут приходить сообщения о выходе");
                                            Guild.WelcomeChannel = 0;
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            var x = Context.Guild.GetTextChannel(Convert.ToUInt64(text.Split("<#")[1].Split(">")[0]));
                                            emb.WithDescription($"В канал {x.Mention} буду приходить сообщения о выходе пользователей.");
                                            Guild.LeaveChannel = x.Id;
                                        }
                                        catch (Exception)
                                        {
                                            emb.WithDescription($"Введенный канал не найден. Укажите канал в таком формате: {Context.Guild.SystemChannel.Mention}");
                                        }
                                    }
                                }
                                break;
                        }
                        DBcontext.Guilds.Update(Guild);
                        await DBcontext.SaveChangesAsync();
                    }
                }
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task privatechannelcreate()
        {
            using (var DBcontext = _db.GetDbContext())
            {
                Guilds Guild = DBcontext.Guilds.Get(Context.Guild);
                var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨PrivateCreate");
                bool da = false;
                if (Guild.PrivateChannelID == 0)
                {
                    embed.WithDescription("Канал для создания приваток создан!");
                    da = !da;
                }
                else
                {
                    if (Context.Guild.VoiceChannels.FirstOrDefault(x => x.Id == Guild.PrivateChannelID) == null)
                    {
                        embed.WithDescription("Канал для создания приваток пересоздан!");
                        da = !da;
                    }
                    else embed.WithDescription($"У вас уже создан канал для приваток с именем {Context.Guild.GetVoiceChannel(Guild.PrivateChannelID).Name}");

                }
                if (da)
                {
                    var category = await Context.Guild.CreateCategoryChannelAsync("DARLING PRIVATE");
                    var pr = await Context.Guild.CreateVoiceChannelAsync("СОЗДАТЬ 🎉");
                    await pr.ModifyAsync(x => x.CategoryId = category.Id);
                    Guild.PrivateChannelID = pr.Id;
                    DBcontext.Guilds.Update(Guild);
                    await DBcontext.SaveChangesAsync();
                }
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task setmuterole()
        {
            using (var DBcontext = _db.GetDbContext())
            {
                Guilds glds = DBcontext.Guilds.Get(Context.Guild);
                var emb = new EmbedBuilder().WithAuthor("🔨MuteRole").WithColor(255, 0, 94);
                var mes = await Context.Channel.SendMessageAsync("", false, emb.Build());
                if (glds.voicemuterole == 0 || glds.chatmuterole == 0)
                {
                    emb.WithDescription("Роли для нарушений создаются...");
                    await new SystemLoading(_discord,_db).CreateMuteRole(Context.Guild);
                    await mes.ModifyAsync(x => x.Embed = emb.WithDescription("Роли созданы и уже привязаны к каналам!").Build());
                }
                else emb.WithDescription("Роли для нарушений уже созданы");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task raidsettings(uint select = 0, uint point = 0)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                Guilds glds = DBcontext.Guilds.Get(Context.Guild);
                var emb = new EmbedBuilder().WithAuthor("🔨RaidSettings").WithColor(255, 0, 94);
                string raidus = "Для подробной настройки Anti-Raid системы используйте -> [инструкцию](https://docs.darlingbot.ru/commands/settings-server/anti-raid-sistema)";
                if (select == 0 && point == 0)
                {
                    emb.WithDescription(raidus);
                    emb.AddField("1.Анти-Raid система", $"{(glds.RaidStop == false ? "Выкл" : "Вкл")}", true);
                    if (glds.RaidStop)
                    {
                        emb.AddField("2.Время срабатывания", $"{glds.RaidTime}", true);
                        emb.AddField("3.Кол-во пользователей", $"{glds.RaidUserCount}", true);
                    }
                    else emb.WithFooter($"Включить систему - {glds.Prefix}rs 1");
                }
                else
                {
                    if (select == 1)
                    {
                        emb.WithDescription($"Anti-Raid система {(glds.RaidStop == true ? "выключена" : "включена")}");
                        if (!glds.RaidStop)
                        {
                            emb.Description += "\nСамые оптимальные настройки уже были выставлены!";
                            emb.WithFooter("Напишите команду без параметров если хотите настроить систему.");
                            glds.RaidUserCount = 5;
                            glds.RaidTime = 3;
                        }
                        glds.RaidStop = !glds.RaidStop;
                    }
                    else if ((select == 2 || select == 3) && glds.RaidStop)
                    {
                        if (select == 2)
                        {
                            if (point < 1 && point > 20)
                                emb.WithDescription("Ради безопасности ваших пользователей, нельзя выставить время меньше 1 и больше 20 секунд.");
                            else
                            {
                                emb.WithDescription($"Если в течении {point} секунд, зайдет {glds.RaidUserCount} человек, их и последующих зашедших пользователей в течении 30 секунд, замутит.\nВнимание, я выдам просто роль, вы самостоятельно ее можете снять, проверив пользователя.");
                                glds.RaidTime = point;
                            }
                        }
                        else
                        {
                            emb.WithDescription($"Если в течении {glds.RaidTime} секунд, зайдет {point} человек, их и последующих зашедших пользователей в течении 30 секунд, замутит.\nВнимание, я выдам просто роль, вы самостоятельно ее можете снять, проверив пользователя.");
                            glds.RaidUserCount = point;
                        }
                    }
                    else
                    {
                        if (glds.RaidStop) emb.WithDescription($"Первый параметр не может быть больше 3.\n{raidus}");
                        else emb.WithDescription($"Для того чтобы пользовать Raid системой, нужно ее включить\n{raidus}").WithFooter($"Включить - {glds.Prefix}rs 1");
                    }

                    DBcontext.Guilds.Update(glds);
                    await DBcontext.SaveChangesAsync();
                }
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionServerOwner]
        public async Task getroletoemote(ulong messageid = 0, string emote = null, SocketRole role = null, bool GetOrRemove = false)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder().WithAuthor("🔨GetRoleToEmote").WithColor(255, 0, 94);
                if (messageid == 0 && emote == null && role == null && GetOrRemove == false)
                {
                    Guilds glds = DBcontext.Guilds.Get(Context.Guild);
                    emb.WithDescription("Команда способствует выдачи/удалению роли участника по нажатию на эмодзи в сообщении!\n\n" +
                    "**messageid** - ваше сообщение в котором будут отслеживаться эмодзи\n" +
                    "**emote** - id эмодзи по нажатию на которое будет выдаваться/удаляться роль[использовать только серверные эмодзи]\n" +
                    "**role** - роль которая будет выдаваться/удаляться\n" +
                    "**GetOrRemove** - Имеет значения False/true, при false роль выдается, при true удаляется.\n\n" +
                    $"Последнее значение можно оставить пустым, оно автоматически будет False\n" +
                    $"Пример: {glds.Prefix}grte [message id] [emote] [role] [true/false/null]");
                }
                else
                {
                    var mes = Context.Guild.TextChannels.Where(x => x.GetMessageAsync(messageid).Result != null).FirstOrDefault();
                    if (mes.Id == 0) emb.WithDescription($"Сообщение с номером {messageid} не найдено");
                    else
                    {
                        if (Context.Guild.TextChannels.Where(x => x.GetMessageAsync(messageid) != null) != null)
                        {
                            if (Context.Guild.Emotes.Where(x => x.Name == emote) != null)
                            {
                                DBcontext.EmoteClick.GetOrCreate(Context.Guild.Id, mes.Id, emote, GetOrRemove, role.Id, messageid);
                                await DBcontext.SaveChangesAsync();
                                emb.WithDescription($"Вы успешно создали {(GetOrRemove == false ? "выдачу" : "удаление")} {role.Mention} за нажатие {Emote.Parse(emote)}");
                                await mes.GetMessageAsync(messageid).Result.AddReactionAsync(Emote.Parse(emote));
                            }
                            else emb.WithDescription("Эмодзи не найдено. Возможно вы используете не серверные эмодзи?\n");
                        }
                        else emb.WithDescription("Сообщение не найдено");
                    }
                }
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        //[Aliases, Commands, Usage, Descriptions]
        //[PermissionBlockCommand, PermissionServerOwner]
        //public async Task getrole()
        //{
        //    var embed = new EmbedBuilder().WithAuthor("🔨LevelRole - уровневые роли отсутствуют ⚠️").WithColor(255, 0, 94);
        //    string info = "";

        //    Guilds glds = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id);
        //    var lvl = new EEF<LVLROLES>(new DBcontext()).Get(x => x.guildid == Context.Guild.Id);
        //    if (lvl.Count() != 0)
        //    {
        //        embed.WithAuthor($"🔨LevelRole - уровневые роли");
        //        lvl = lvl.OrderBy(u => u.countlvl);
        //        foreach (var LVL in lvl)
        //            info += $"{Context.Guild.GetRole(LVL.roleid).Mention} - {LVL.countlvl} уровень\n";
        //    }
        //    embed.AddField("Добавить роль", $"{glds.Prefix}levelroleAdd [ROLE] [LEVEL]");
        //    embed.AddField("Удалить роль", $"{glds.Prefix}levelroleDel [ROLE]");

        //    await Context.Channel.SendMessageAsync("", false, embed.WithDescription($"{info}").Build());
        //}

        //[Aliases, Commands, Usage, Descriptions]
        //[PermissionBlockCommand, PermissionServerOwner]
        //public async Task getroleadd(SocketRole role, uint level)
        //{
        //    var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨LevelRoleAdd");
        //    var glds = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id);
        //    var lvlrole = new EEF<LVLROLES>(new DBcontext()).GetF(x => x.roleid == role.Id && x.guildid == Context.Guild.Id);
        //    if (lvlrole == null)
        //    {
        //        emb.WithDescription($"Роль {role.Mention} выставлена за {level} уровень").WithFooter($"Посмотреть ваши уровневые роли {glds.Prefix}lr");
        //        new EEF<LVLROLES>(new DBcontext()).Create(new LVLROLES() { guildid = Context.Guild.Id, roleid = role.Id, countlvl = level });
        //    }
        //    else if (lvlrole.roleid == role.Id) emb.WithDescription($"Роль {role.Mention} уже выдается за {lvlrole.countlvl} уровень");


        //    await Context.Channel.SendMessageAsync("", false, emb.Build());
        //}

        //[Aliases, Commands, Usage, Descriptions]
        //[PermissionBlockCommand, PermissionServerOwner]
        //public async Task getroledel(SocketRole role)
        //{
        //    var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨GetRoleDel");
        //    var lvlrole = new EEF<LVLROLES>(new DBcontext()).GetF(x => x.roleid == role.Id && x.guildid == Context.Guild.Id);
        //    emb.WithDescription($"Роль {role.Mention} {(lvlrole != null ? "удалена из списка" : "не выдавалась")}.");
        //    if (lvlrole != null) new EEF<LVLROLES>(new DBcontext()).Remove(lvlrole);
        //    await Context.Channel.SendMessageAsync("", false, emb.Build());

        //}
    }
}
