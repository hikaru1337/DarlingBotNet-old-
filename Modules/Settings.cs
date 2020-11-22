using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.DataBase.Database.Models;
using DarlingBotNet.Services;
using DarlingBotNet.Services.Sys;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DarlingBotNet.DataBase.Guilds;
using static DarlingBotNet.DataBase.Warns;

namespace DarlingBotNet.Modules
{
    [PermissionServerOwner]
    public class Settings : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        private readonly IMemoryCache _cache;

        
        public Settings(DiscordSocketClient discord, CommandService commands, IServiceProvider provider, IMemoryCache cache)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _cache = cache;

        } // Подключение компонентов

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task violationsystem(ViolationSystem VS = ViolationSystem.none)
        {
            using (var DBcontext = new DBcontext())
            {
                var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id);
                var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("ViolationSystem")
                                              .WithFooter($"Выбор системы - **{Guild.Prefix}VS [WarnSystem/off/null]**\n" +
                                                          $"💤**OFF** - отключить систему");

                if (VS == ViolationSystem.WarnSystem)
                {
                    if (Guild.VS == VS)
                        embed.WithDescription($"Система **Warn** уже выбрана!");
                    else
                    {
                        Guild.VS = VS;
                        DBcontext.Guilds.Update(Guild);
                        await DBcontext.SaveChangesAsync();
                        embed.WithDescription($"Система **Warn** выбрана!");
                    }
                }
                else if (VS == ViolationSystem.ReportSystem)
                {
                    embed.WithDescription("⚠️С репост системой технические шоколадки...");
                }
                else if (VS == ViolationSystem.off)
                {
                    if (Guild.VS == VS || Guild.VS == ViolationSystem.none)
                        embed.WithDescription($"Система уже отключена!");
                    else
                    {
                        Guild.VS = VS;
                        DBcontext.Guilds.Update(Guild);
                        await DBcontext.SaveChangesAsync();
                        embed.WithDescription($"Система отключена!");
                    }
                }
                else
                {
                    if (Guild.VS == ViolationSystem.none || Guild.VS == ViolationSystem.off)
                        embed.WithAuthor("ViolationSystem - ⚠️ У вас не настроена репорт система!")
                            .WithDescription($"Для настройки выберите тип системы:\n" +
                            $"📕**WarnSystem** - выдача наказаний по количеству выданных предупреждений.\n" +
                            $"Не нуждается в точной настройке, варн дается за любое нарушенное правило\n\n" +
                            $"📒**ReportSystem** - находится в разработке...");

                    else if (VS == ViolationSystem.ReportSystem)
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

                    else if (Guild.VS == ViolationSystem.WarnSystem)

                        embed.WithAuthor("ViolationSystem - WarnSystem ✅")
                            .WithDescription($"Для настройки системы используйте:\n\n" +
                            $"**Выставить варны** - {Guild.Prefix}addwarn [Номер варна] [Варн] [причина]\n" +
                            $"**Удалить варн** - {Guild.Prefix}delwarn [номер]\n" +
                            $"**Выдать варн** - {Guild.Prefix}warn [USER]\n" +
                            $"**Снять варн с пользователя** - {Guild.Prefix}unwarn [USER] [Колво-варнов]\n" +
                            $"**Посмотреть варны** - {Guild.Prefix}warns\n\n" +
                            $"**Более подробно тут - [Инструкция](https://docs.darlingbot.ru/commands/settings-server/vybor-sistemy-narushenii)**");

                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionViolation]
        public async Task addwarn(byte CountWarn, ReportType report, ushort MinuteTime = 0)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("AddWarn");
                if ((report == ReportType.tban || report == ReportType.tmute) && MinuteTime == 0)
                    emb.WithDescription("Варн должен содержать время от 1 до 1440 минут");
                else
                {
                    var warn = DBcontext.Warns.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.CountWarn == CountWarn);


                    if (CountWarn >= 1 && CountWarn <= 15)
                    {
                        if (MinuteTime >= 1 && MinuteTime <= 1440)
                        {
                            var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                            if (warn != null)
                            {
                                emb.WithDescription($"Варн {CountWarn} был перезаписан с `{warn.ReportTypes}` на `{report}`.");
                                warn.ReportTypes = report;
                                warn.MinutesWarn = MinuteTime;
                                DBcontext.Warns.Update(warn);
                            }
                            else
                            {
                                emb.WithDescription($"Варн {CountWarn} был успешно добавлен.");
                                var newwarn = new Warns() { GuildId = Context.Guild.Id, CountWarn = CountWarn, ReportTypes = report, MinutesWarn = MinuteTime };
                                DBcontext.Warns.Add(newwarn);
                            }
                            emb.WithFooter($"Посмотреть все варны {GuildPrefix}ws");

                            await DBcontext.SaveChangesAsync();
                        }
                        else
                            emb.WithDescription("Время должно быть больше 1 и меньше 1440 минут");
                    }
                    else emb.WithDescription($"Количество варнов может быть больше 1 и меньше 15");
                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand, PermissionViolation]
        public async Task delwarn(byte CountWarn)
        {
            using (var DBcontext = new DBcontext())
            {
                var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                var warn = DBcontext.Warns.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.CountWarn == CountWarn);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("AddWarn");
                if (warn != null)
                {
                    DBcontext.Warns.Remove(warn);
                    await DBcontext.SaveChangesAsync();
                    emb.WithDescription($"Варн с номером {CountWarn} успешно удален.");
                }
                else emb.WithDescription($"Варн с номером {CountWarn} отсутствует.");
                emb.WithFooter($"Посмотреть все варны {GuildPrefix}ws");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task prefix(string prefix = null)
        {
            using (var DBcontext = new DBcontext())
            {
                var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Prefix");
                if (prefix == null) emb.WithDescription($"Префикс сервера - {Guild.Prefix}");
                else
                {
                    if (prefix.Length > 4) emb.WithDescription($"Префикс не может быть длиньше 4 символов");
                    else
                    {
                        emb.WithDescription($"Префикс сервера изменен с `{Guild.Prefix}` на `{prefix}`");
                        Guild.Prefix = prefix;
                        DBcontext.Guilds.Update(Guild);
                        await DBcontext.SaveChangesAsync();
                    }
                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task commandinvise(string commandname = null)
        {
            using (var DBcontext = new DBcontext())
            {
                var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("CommandInvise - Отключенные команды");

                if (commandname == null)
                {
                    foreach (var cmd in Guild.CommandInviseList)
                    {
                        var x = _commands.Commands.FirstOrDefault(x => x.Aliases.FirstOrDefault().ToLower() == cmd || x.Aliases.LastOrDefault().ToLower() == cmd);
                        if (x != null)
                        {
                            emb.Description += $"**{cmd}/{x.Name}** - {x.Summary.Split("||")[0]}";
                        }
                    }
                    if (emb.Description == null)
                        emb.WithDescription("Команды еще не добавлены.").WithFooter($"Включить команду - {Guild.Prefix}CommandInvise [commandname]");
                }
                else
                {
                    commandname = commandname.ToLower();

                    var CommandName = _commands.Commands.FirstOrDefault(x => x.Aliases.FirstOrDefault().ToLower() == commandname || x.Aliases.LastOrDefault().ToLower() == commandname);

                    if (CommandName == null)
                        emb.WithDescription("Такая команда не существует");
                    else if (BotSettings.CommandNotInvise.Count(x => x == CommandName.Name.ToLower()) != 0)
                        emb.WithDescription("Эту команду нельзя отключить!");
                    else
                    {
                        List<string> liststring = Guild.CommandInviseList;
                        var Command = Guild.CommandInviseList.FirstOrDefault(x => x == commandname);

                            emb.WithDescription($"Команда `{commandname}` {(Command == null ? "отключена" : "включена")}")
                                .WithFooter($"{(Command == null ? "Включить" : "Отключить")} команду - {Guild.Prefix}ci [commandname]");

                            if (Command == null)
                                liststring.Add(commandname);
                            else
                                liststring.Remove(commandname);

                            Guild.CommandInviseList = liststring;
                            DBcontext.Guilds.Update(Guild);
                            await DBcontext.SaveChangesAsync();
                    }
                }

                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task task()
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Task");

                var UserBoost = DBcontext.DarlingBoost.FirstOrDefault(x => x.UserId == Context.User.Id);
                if (UserBoost != null && UserBoost.Ends > DateTime.Now)
                {
                    var Tasks = DBcontext.Tasks.AsQueryable().Where(x => x.GuildId == Context.Guild.Id);
                    emb.WithDescription("Инструкция к команде: \nВаши задачи: ");
                    int i = 0;
                    foreach (var Task in Tasks)
                    {
                        string text = "";
                        i++;
                        var Check = MessageBuilder.EmbedUserBuilder(Task.Message);
                        if (Check.Item2 == "ERROR")
                            text = Task.Message;
                        else
                            text = Check.Item1.Description;


                        emb.Description += $"\n\n{i}.<#{Task.ChannelId}> Повторение: {(Task.Repeat ? $"Вкл Время: {Task.Times.ToShortTimeString()}" : $"Выкл Время: {Task.Times}")}  текст: ||{text}||";
                    }
                    if (i == 0)
                        emb.Description += "отсутствуют!";
                    var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                    emb.AddField("Добавить задачу", $"{GuildPrefix}TaskAdd [Channel] [repeat] [Time] [Message]", true);
                    emb.AddField("Удалить задачу", $"{GuildPrefix}TaskDel [id]", true);
                }
                else
                    emb.WithDescription("Для использования Task системы, приобритите [DarlingBoost](https://docs.darlingbot.ru/commands/darling-boost)");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task taskadd(SocketTextChannel TextChannel, bool repeat, string Time, [Remainder] string message)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("TaskAdd");

                var UserBoost = DBcontext.DarlingBoost.FirstOrDefault(x => x.UserId == Context.User.Id);
                if (UserBoost != null && UserBoost.Ends > DateTime.Now)
                {

                    DateTime Times = new DateTime();
                    bool tried = false;

                    try
                    {
                        Time = Time.Replace('-', ' ');
                        Times = Convert.ToDateTime(Time);
                        Times = Times.AddHours(-3);
                    }
                    catch (Exception)
                    {
                        tried = true;
                        emb.WithDescription("Дата введена некорректно. Пример: 10.10.2020-15:30");
                    }

                    if (!tried)
                    {
                        if (Times >= DateTime.UtcNow)
                        {
                            if ((Times - DateTime.UtcNow).TotalMinutes >= 1 && (Times - DateTime.UtcNow).TotalDays <= 31)
                            {
                                var CountTask = DBcontext.Tasks.AsQueryable().Count(x => x.GuildId == Context.Guild.Id);
                                if (CountTask <= 15)
                                {
                                    var Tasks = new Tasks() { GuildId = Context.Guild.Id, ChannelId = TextChannel.Id, Message = message, Times = Times, Repeat = repeat };
                                    var TasksDB = DBcontext.Tasks.Add(Tasks);
                                    await DBcontext.SaveChangesAsync();
                                    if ((Times - DateTime.UtcNow).TotalHours <= 24)
                                        await TaskTimer.StartTimerNow(TasksDB.Entity);
                                    emb.WithDescription($"Сообщение отложено в канал {TextChannel.Mention} на {(repeat == false ? Times.ToString("dd/MM/yyyy HH:mm") : $"повторение в {Times.ToShortTimeString()}")}.");
                                }
                                else
                                    emb.WithDescription("Создать больше 15 отложенных сообщений нельзя!");
                            }
                            else emb.WithDescription("Время может быть не меньше 1 минуты и больше 31 дня!");
                        }
                        else emb.WithDescription("Время не может быть меньше текущего!");
                    }
                }
                else
                    emb.WithDescription("Для использования Task системы, приобритите [DarlingBoost](https://docs.darlingbot.ru/commands/darling-boost)");
                    _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task taskdel(uint id)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("TaskDel");
                var UserBoost = DBcontext.DarlingBoost.FirstOrDefault(x => x.UserId == Context.User.Id);
                if (UserBoost != null && UserBoost.Ends > DateTime.Now)
                {
                    var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                    if (id != 0)
                        id--;
                    var Tasks = DBcontext.Tasks.AsQueryable().Where(x => x.GuildId == Context.Guild.Id).AsEnumerable().ElementAt((int)id);

                    if (Tasks == null)
                        emb.WithDescription("Задача с таким id не найдена!").WithFooter($"Узнать id -> {GuildPrefix}Task");
                    else
                    {
                        emb.WithDescription($"Задача в канал <#{Tasks.ChannelId}> на {(Tasks.Repeat ? $"повторение в {Tasks.Times.ToShortTimeString()}" : Tasks.Times.ToString())} была удалена!");
                        DBcontext.Remove(Tasks);
                        await DBcontext.SaveChangesAsync();
                    }
                }
                else
                    emb.WithDescription("Для использования Task системы, приобритите [DarlingBoost](https://docs.darlingbot.ru/commands/darling-boost)");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task levelroleadd(SocketRole role, uint level)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨lr.Add");
                var lvlrole = DBcontext.LVLROLES.FirstOrDefault(x => x.RoleId == role.Id && x.GuildId == Context.Guild.Id);
                if (lvlrole == null)
                {
                    var rolepos = Context.Guild.CurrentUser.Roles.FirstOrDefault(x => x.Position > role.Position);
                    if (rolepos != null)
                    {
                        if (!role.IsManaged)
                        {
                            var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                            emb.WithDescription($"Роль {role.Mention} выставлена за {level} уровень").WithFooter($"Посмотреть ваши уровневые роли {GuildPrefix}lr");
                            DBcontext.LVLROLES.Add(new LVLROLES() { RoleId = role.Id, GuildId = role.Guild.Id, CountLvl = level });
                            await DBcontext.SaveChangesAsync();
                        }
                        else emb.WithDescription("Роль бота или Boost, нельзя сделать уровневыми!");
                    }
                    else emb.WithDescription("Роль бота ниже этой роли, из-за чего бот не сможет выдавать ее.").WithFooter("Поднимите роль бота выше выдаваемой роли.");
                }
                else emb.WithDescription($"Роль {role.Mention} уже выдается за {lvlrole.CountLvl} уровень");

                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task levelroledel(SocketRole role)
        {
            _cache.Removes(Context);
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨lr.Del");
                var lvlrole = DBcontext.LVLROLES.FirstOrDefault(x => x.RoleId == role.Id && x.GuildId == role.Guild.Id);
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
        [PermissionBlockCommand]
        public async Task channelsettings(SocketTextChannel channel = null, float number = 0)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94);
                var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                var chnl = new Channels();
                emb.WithAuthor($"🔨ChannelSettings {(channel == null ? " " : $"- {channel.Name}")}");
                if (channel != null) chnl = _cache.GetOrCreateChannelCache(channel.Id, channel.Guild.Id);
                if (channel != null && number == 0)
                {
                    if (chnl != null)
                    {
                        emb.AddField("1 Получение опыта", chnl.GiveXP ? "Вкл" : "Выкл", true);
                        emb.AddField("2 Удалять ссылки", chnl.DelUrl ? "Вкл" : "Выкл", true);
                        if (chnl.DelUrl) emb.AddField("2,1 Удалять ссылки-изображения?", chnl.DelUrlImage ? "Вкл" : "Выкл", true);
                        if (chnl.DelUrl) emb.AddField("Url White List:", chnl.csUrlWhiteListList.Count == 0 ? "-" : string.Join(",", chnl.csUrlWhiteListList), true);
                        emb.AddField("3 Удалять КАПС сообщения", chnl.DelCaps ? "Вкл" : "Выкл", true);
                        emb.AddField("4 Плохие слова", chnl.SendBadWord ? "Вкл" : "Выкл", true);
                        if (chnl.SendBadWord) emb.AddField("Плохие слова:", chnl.BadWordList.Count == 0 ? "-" : string.Join(",", chnl.BadWordList), true);
                        emb.AddField("5 Использование команд", chnl.UseCommand ? "Вкл" : "Выкл", true);
                        if (!chnl.UseCommand) emb.AddField("5,1 Использование RP команд?", chnl.UseRPcommand ? "Вкл" : "Выкл", true);
                        emb.AddField("6 Спам [BETA]", chnl.Spaming ? "Вкл" : "Выкл", true);
                        emb.AddField("7 Удалять приглашения(кроме тех что сюда)", chnl.InviteMessage ? "Вкл" : "Выкл", true);
                        emb.AddField("Номер Чата", chnl.ChannelId, true);
                        emb.WithFooter($"Вкл/Выкл опции канала - {GuildPrefix}ChannelSettings [channel] [number]");
                    }
                    else emb.WithDescription("Данный канал не найден!");
                }
                else if (channel != null && number != 0)
                {
                    if (number >= 1 && number <= 7)
                    {
                        switch (number)
                        {
                            case 1:
                                emb.WithDescription($"Получение уровней в {channel.Mention} {(chnl.GiveXP ? "выключено" : "включено")}");
                                chnl.GiveXP = !chnl.GiveXP;
                                break;
                            case 2:
                                emb.WithDescription($"Ссылки в {channel.Mention} {(chnl.DelUrl ? "не удаляются" : "удаляются")}");
                                chnl.DelUrl = !chnl.DelUrl;
                                if (chnl.DelUrl)
                                {
                                    emb.WithFooter("Вы можете дополнительно настроить ссылки! Откройте команду еще раз!");
                                    chnl.DelUrlImage = true;
                                }
                                else chnl.DelUrlImage = false;
                                break;
                            case 2.1f:
                                emb.WithDescription($"Ссылки-картинки в {channel.Mention} {(chnl.DelUrlImage ? "не удаляются" : "удаляются")}");
                                chnl.DelUrlImage = !chnl.DelUrlImage;
                                break;
                            case 3:
                                emb.WithDescription($"КАПС-сообщения {channel.Mention} теперь {(chnl.DelCaps ? "не удаляются" : "удаляются")}");
                                chnl.DelCaps = !chnl.DelCaps;
                                break;
                            case 4:
                                emb.WithDescription($"Плохие слова в {channel.Mention} теперь {(chnl.SendBadWord ? "не удаляются" : "удаляются")}");
                                chnl.SendBadWord = !chnl.SendBadWord;
                                emb.WithFooter($"Добавить/удалить плохие слова - {GuildPrefix}cs.bw [channel] [word]");
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
                    else emb.WithDescription("Номер может быть от 1 до 7.").WithFooter($"Подробнее - {GuildPrefix}cs [channel]");
                }
                else emb.WithDescription($"Введите нужный вам канал, пример - {GuildPrefix}cs [channel]");

                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task channelsettingsbadword(SocketTextChannel channel, string word)
        {
            using (var DBcontext = new DBcontext())
            {
                var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨cs.badword")
                                            .WithFooter($"Добавить/Удалить - {GuildPrefix}cs.bw {channel.Name} [слово]");
                var chnl = _cache.GetOrCreateChannelCache(channel.Id, channel.Guild.Id);

                if (chnl.SendBadWord)
                {
                    List<string> badlist = chnl.BadWordList;
                    if (chnl.BadWordList.FirstOrDefault(x => x == word) != null)
                    {
                        emb.WithDescription($"Слово {word} удалено из списока");
                        badlist.Remove(word);
                    }
                    else
                    {
                        emb.WithDescription($"Слово {word} включено в список");
                        badlist.Add(word);
                    }
                    chnl.BadWordList = badlist;
                    DBcontext.Channels.Update(chnl);
                    await DBcontext.SaveChangesAsync();
                }
                else emb.WithDescription($"Вы не включили проверку Плохих слов\n{GuildPrefix}cs {channel.Name} [4]");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task channelsettingsgivexp()
        {
            using (var DBcontext = new DBcontext())
            {
                Guilds Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨cs.givexp");
                Guild.GiveXPnextChannel = !Guild.GiveXPnextChannel;
                DBcontext.Guilds.Update(Guild);
                await DBcontext.SaveChangesAsync();
                emb.WithDescription($"В дальнейшем в созданных каналах, опыт {(Guild.GiveXPnextChannel ? "" : "не ")}будет получаться");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task channelsettingsurlwhitelist(SocketTextChannel channel, string url)
        {
            using (var DBcontext = new DBcontext())
            {
                var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨cs.UrlWhiteList")
                                            .WithFooter($"Добавить/Удалить - {GuildPrefix}cs.uwl #{channel.Name} [url]");
                var TextChannel = _cache.GetOrCreateChannelCache(channel.Id, channel.Guild.Id);

                if (TextChannel.DelUrl)
                {
                    List<string> UrlWhiteList = TextChannel.csUrlWhiteListList;
                    if (TextChannel.csUrlWhiteListList.FirstOrDefault(x => x == url) != null)
                    {
                        emb.WithDescription($"Url {url} удалено из White list");
                        UrlWhiteList.Remove(url);
                    }
                    else
                    {
                        emb.WithDescription($"Url {url} включено в White list");
                        UrlWhiteList.Add(url);
                    }

                    TextChannel.csUrlWhiteListList = UrlWhiteList;
                    DBcontext.Channels.Update(TextChannel);
                    await DBcontext.SaveChangesAsync();
                }
                else emb.WithDescription($"Вы не включили Белый лист ссылок!\n{GuildPrefix}cs {channel.Mention} [2]");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task logsettings(uint selection = 0, SocketTextChannel channel = null)
        {
            using (var DBcontext = new DBcontext())
            {
                var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - LogsServer", Context.Guild.IconUrl);
                int Count = 8;
                if (selection == 0 && channel == null)
                {
                    for (int i = 0; i < Count; i++)
                    {
                        SocketTextChannel Channel = null;
                        string text = null;
                        switch (i)
                        {
                            case 1:
                                Channel = Context.Guild.GetTextChannel(Guild.banchannel);
                                text = "Бан пользователя";
                                break;
                            case 2:
                                Channel = Context.Guild.GetTextChannel(Guild.unbanchannel);
                                text = "Разбан пользователя";
                                break;
                            case 3:
                                Channel = Context.Guild.GetTextChannel(Guild.kickchannel);
                                text = "Кик пользователя";
                                break;
                            case 4:
                                Channel = Context.Guild.GetTextChannel(Guild.joinchannel);
                                text = "Вход пользователя";
                                break;
                            case 5:
                                Channel = Context.Guild.GetTextChannel(Guild.leftchannel);
                                text = "Выход пользователя";
                                break;
                            case 6:
                                Channel = Context.Guild.GetTextChannel(Guild.mesdelchannel);
                                text = "Удаленные сообщения";
                                break;
                            case 7:
                                Channel = Context.Guild.GetTextChannel(Guild.meseditchannel);
                                text = "Измененные сообщения";
                                break;
                            case 8:
                                Channel = Context.Guild.GetTextChannel(Guild.voiceUserActions);
                                text = "Активность в голосовых чатах";
                                break;

                        }
                        emb.AddField($"{i}.{text}", Channel == null ? "Канал не указан" : Channel.Mention, true);
                    }
                    emb.WithFooter($"Включить - {Guild.Prefix}LogSettings [цифра] [канал для сообщений]\nОтключить - {Guild.Prefix}LogSettings [цифра]");
                }
                else
                {
                    if (!(selection >= 1 && selection <= Count)) 
                        emb.WithDescription($"Выбор может быть только от 1 до {Count}").WithFooter($"Подробнее - {Guild.Prefix}LogSettings");
                    else
                    {
                        string WhatMessage = null;
                        switch (selection)
                        {
                            case 1:
                                WhatMessage = "банах";
                                Guild.banchannel = channel == null ? 0 : channel.Id;
                                break;
                            case 2:
                                WhatMessage = "разбанах";
                                Guild.unbanchannel = channel == null ? 0 : channel.Id;
                                break;
                            case 3:
                                WhatMessage = "киках";
                                Guild.kickchannel = channel == null ? 0 : channel.Id;
                                break;
                            case 4:
                                WhatMessage = "входах пользователей";
                                Guild.joinchannel = channel == null ? 0 : channel.Id;
                                break;
                            case 5:
                                WhatMessage = "Выходах пользователей";
                                Guild.leftchannel = channel == null ? 0 : channel.Id;
                                break;
                            case 6:
                                WhatMessage = "Измененных сообщениях";
                                Guild.meseditchannel = channel == null ? 0 : channel.Id;
                                break;
                            case 7:
                                WhatMessage = "Удаленных сообщениях";
                                Guild.mesdelchannel = channel == null ? 0 : channel.Id;
                                break;
                            case 8:
                                WhatMessage = "действиях пользователей";
                                Guild.voiceUserActions = channel == null ? 0 : channel.Id;
                                break;
                        }
                        DBcontext.Guilds.Update(Guild);
                        await DBcontext.SaveChangesAsync();
                        if(channel == null)
                            emb.WithDescription($"Отправка сообщений о {WhatMessage} была отключена");
                        else
                            emb.WithDescription($"В канал {channel.Mention} будут приходить сообщения о {WhatMessage}");

                    }
                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task messagesettings(byte selection = 0, [Remainder] string text = null)
        {
            using (var DBcontext = new DBcontext())
            {
                var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor(" - MessageSettings", Context.Guild.IconUrl);
                byte point = 1;
                if (selection == 0 && text == null)
                {
                    emb.AddField($"{point}.Сообщение при входе [json]", Guild.WelcomeMessage != null ? $"Установлено" : "Отсутствует", true);
                    point++;
                    emb.AddField($"{point}.Личное сообщение при входе [json]", Guild.WelcomeDMmessage != null ? $"Установлено" : "Отсутствует", true);
                    point++;
                    if (Guild.WelcomeMessage != null)
                    {
                        var WelcomeChannel = Context.Guild.GetTextChannel(Guild.WelcomeChannel);
                        emb.AddField($"{point}.Канал для Сообщений при входе [channel]", WelcomeChannel != null ? WelcomeChannel.Mention : "Отсутствует", true);
                        point++;
                        var WelcomeRole = Context.Guild.GetRole(Guild.WelcomeRole);
                        emb.AddField($"{point}.Роль при входе [role]", WelcomeRole != null ? WelcomeRole.Mention : "Отсутствует", true);
                        point++;
                    }
                    emb.AddField($"{point}.Сообщение при выходе [json]", Guild.LeaveMessage != null ? $"Установлено" : "Отсутствует", true);
                    point++;
                    if (Guild.LeaveMessage != null)
                    {
                        var LeaveChannel = Context.Guild.GetTextChannel(Guild.LeaveChannel);
                        emb.AddField($"{point}.Канал для Сообщений при выходе [channel]", LeaveChannel != null ? LeaveChannel.Mention : "Отсутствует", true);
                    }

                    emb.WithFooter($"Вкл - {Guild.Prefix}ms [цифра] [channel/json/role]\nВыкл - {Guild.Prefix}ms [цифра]\n%user% - чтобы упомянуть пользователя");
                }
                else
                {
                    point = 3;
                    if (Guild.LeaveMessage != null) point++;
                    if (Guild.WelcomeMessage != null) point += 2;
                    if (!(selection >= 1 && selection <= point)) 
                        emb.WithDescription($"Выбор может быть только от 1 до {point}").WithFooter($"Подробнее - {Guild.Prefix}ms");
                    else
                    {
                        switch (selection)
                        {
                            case 1:
                                if (text != null)
                                {
                                    var embed = MessageBuilder.EmbedUserBuilder(text);
                                    if (embed.Item2 == "ERROR")
                                        emb.WithDescription($"Сообщение составлено не верно.").WithFooter("скопируйте json код с сайта embed.discord-bot.net");
                                    else
                                    {
                                        emb.WithDescription($"EmbedVisualizer составил ваше сообщение.").WithFooter("Для отображение этого сообщения укажите канал - h.ms 3 [channel]");
                                        Guild.WelcomeMessage = text;
                                    }
                                }
                                else
                                {
                                    if (Guild.WelcomeMessage == null) 
                                        emb.WithDescription($"Введите текст для включения сообщения при входе").WithFooter($"Подробнее - {Guild.Prefix}ms");
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
                                    var embed = MessageBuilder.EmbedUserBuilder(text);
                                    if (embed.Item2 == "ERROR")
                                        emb.WithDescription($"Сообщение составлено не верно.").WithFooter("скопируйте json код с сайта embed.discord-bot.net");
                                    else
                                    {
                                        emb.WithDescription($"EmbedVisualizer cообщение будет отправляться пользователю при входе на сервер.").WithFooter("Если он не отключил возможность отправлять ему сообщения");
                                        Guild.WelcomeDMmessage = text;
                                    }
                                }
                                else
                                {
                                    if (Guild.WelcomeDMmessage == null) 
                                        emb.WithDescription($"Введите текст для включения Личного сообщения при входе").WithFooter($"Подробнее - {Guild.Prefix}ms");
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
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task privatechannelcreate()
        {
            using (var DBcontext = new DBcontext())
            {
                var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id);
                var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨PrivateCreate");
                var PrivateVoiceChannel = Context.Guild.GetVoiceChannel(Guild.PrivateChannelID);

                if (PrivateVoiceChannel == null)
                {
                    embed.WithDescription($"Канал для создания приваток {(Guild.PrivateChannelID == 0 ? "создан" : "пересоздан")}!");
                    var category = await Context.Guild.CreateCategoryChannelAsync("DARLING PRIVATE");
                    var pr = await Context.Guild.CreateVoiceChannelAsync("СОЗДАТЬ 🎉", x => x.CategoryId = category.Id);
                    Guild.PrivateChannelID = pr.Id;
                    DBcontext.Guilds.Update(Guild);
                    await DBcontext.SaveChangesAsync();
                }
                else embed.WithDescription($"У вас уже создан канал для приваток с именем {PrivateVoiceChannel.Name}");

                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task setmuterole()
        {
            using (var DBcontext = new DBcontext())
            {
                var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id);
                var emb = new EmbedBuilder().WithAuthor("🔨MuteRole").WithColor(255, 0, 94).WithDescription("Роли для нарушений уже созданы!");
                var chatrole = Context.Guild.GetRole(Guild.chatmuterole);
                var voicerole = Context.Guild.GetRole(Guild.voicemuterole);
                if (chatrole == null || voicerole == null)
                    emb.WithDescription("Роли для нарушений создаются...");

                var mes = await Context.Channel.SendMessageAsync("", false, emb.Build());
                if (chatrole == null || voicerole == null)
                {
                    Guild = await OtherSettings.CreateMuteRole(Context.Guild);
                    chatrole = Context.Guild.GetRole(Guild.chatmuterole);
                    voicerole = Context.Guild.GetRole(Guild.voicemuterole);
                    await mes.ModifyAsync(x => x.Embed = emb.WithDescription($"Созданы роли {chatrole.Mention},{voicerole.Mention} и уже привязаны к каналам!").Build());
                }
                _cache.Removes(Context);
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task raidsettings(uint select = 0, uint point = 0)
        {
            using (var DBcontext = new DBcontext())
            {
                Guilds glds = _cache.GetOrCreateGuldsCache(Context.Guild.Id);

                var UserBoost = DBcontext.DarlingBoost.FirstOrDefault(x => x.UserId == Context.User.Id);
                var emb = new EmbedBuilder().WithAuthor("🔨RaidSettings").WithColor(255, 0, 94);
                if (UserBoost != null && UserBoost.Ends > DateTime.Now)
                {
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
                }
                else
                    emb.WithDescription("Для использования Anti-Raid системы, приобритите [DarlingBoost](https://docs.darlingbot.ru/commands/darling-boost)");
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task emoteclick()
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithAuthor("🔨EmoteClick").WithColor(255, 0, 94);
                var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                var Emotes = DBcontext.EmoteClick.AsQueryable().Where(x => x.GuildId == Context.Guild.Id);
                emb.WithDescription("Инструкция: [КЛИК](https://docs.darlingbot.ru/commands/settings-server/rol-po-nazhatiyu-na-emodzi)\nЗадачи:");
                int i = 0;
                foreach (var Emote in Emotes)
                {
                    i++;
                    emb.Description += $"\n{i}.<#{Emote.ChannelId}> <@&{Emote.RoleId}> {Emote.emote} {(Emote.get ? "Убирается":  "Выдается")}";
                }
                if (i == 0)
                    emb.Description += " Ролей за эмодзи нет!";
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task emoteclickadd(ulong messageid, string emote, SocketRole role, bool DelRole = true)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithAuthor("🔨EmoteClickAdd").WithColor(255, 0, 94);
                var mes = Context.Guild.TextChannels.FirstOrDefault(x => x.GetMessageAsync(messageid).Result != null);
                if (mes == null) emb.WithDescription($"Сообщение с номером {messageid} не найдено");
                else
                {
                    if (Context.Guild.TextChannels.Where(x => x.GetMessageAsync(messageid) != null) != null)
                    {
                        Emote.TryParse(emote,out Emote result);
                        if (result != null)
                        {
                            if (Context.Guild.Emotes.FirstOrDefault(x => x.Name == result.Name && x.Id == result.Id) != null)
                            {
                                var rolepos = Context.Guild.CurrentUser.Roles.FirstOrDefault(x => x.Position > role.Position);
                                if (rolepos != null)
                                {
                                    if (!role.IsManaged)
                                    {
                                        await mes.GetMessageAsync(messageid).Result.AddReactionAsync(result);
                                        DBcontext.EmoteClick.Add(new EmoteClick() { GuildId = Context.Guild.Id, MessageId = messageid, emote = emote, get = DelRole, RoleId = role.Id, ChannelId = mes.Id });
                                        await DBcontext.SaveChangesAsync();
                                        emb.WithDescription($"Вы успешно создали {(DelRole ? "удаление" : "выдачу")} {role.Mention} за нажатие {result}");
                                    }
                                    else emb.WithDescription("Роль бота или Boost, нельзя сделать для выдачи!");
                                }
                                else emb.WithDescription("Роль бота ниже этой роли, из-за чего бот не сможет выдавать ее.").WithFooter("Поднимите роль бота выше выдаваемой роли.");
                            }
                            else emb.WithDescription("Эмодзи не найдено. Возможно вы используете не серверные эмодзи?\n");
                        }
                        else emb.WithDescription("Эмодзи не найдено. Возможно вы используете не серверные эмодзи?\n");
                    }
                    else emb.WithDescription("Сообщение не найдено");
                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }
        
        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task emoteclickdel(uint id)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithAuthor("🔨EmoteClick").WithColor(255, 0, 94);
                var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
                if (id != 0)
                    id--;
                var Emote = DBcontext.EmoteClick.AsQueryable().Where(x => x.GuildId == Context.Guild.Id).AsEnumerable().ElementAt((int)id);
                if (Emote == null)
                    emb.WithDescription("Задача с таким id не найдена!");
                else
                {
                    emb.WithDescription($"Задача {(Emote.get ? "выдачи" : "удаления")} роли <@&{Emote.RoleId}> на эмодзи {Emote.emote} в канале <#{Emote.ChannelId}> была удалена!");
                    DBcontext.Remove(Emote);
                    await DBcontext.SaveChangesAsync();
                }
                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task buyroleadd(SocketRole role, ulong price)
        {
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨BuyRoleAdd");
                var glds = DBcontext.Guilds.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
                var BuyRoles = DBcontext.RoleSwaps.AsQueryable().Where(x => x.GuildId == Context.Guild.Id && x.type == RoleSwaps.RoleType.RoleBuy);
                var BuyRole = BuyRoles.FirstOrDefault(x => x.Id == role.Id);
                if (BuyRole == null)
                {
                    if (BuyRoles.Count() < 10)
                    {
                        var rolepos = Context.Guild.CurrentUser.Roles.FirstOrDefault(x => x.Position > role.Position);
                        if (rolepos != null)
                        {
                            if (!role.IsManaged)
                            {
                                emb.WithDescription($"Роль {role.Mention} успешно выставлена за {price} ZeroCoin's!");
                                var QueryDB = new RoleSwaps() { GuildId = Context.Guild.Id, Price = price, RoleId = role.Id, type = RoleSwaps.RoleType.RoleBuy };
                                DBcontext.RoleSwaps.Add(QueryDB);
                                await DBcontext.SaveChangesAsync();
                            }
                            else emb.WithDescription("Роль бота или Boost, нельзя сделать уровневыми!");
                        }
                        else emb.WithDescription("Роль бота ниже этой роли, из-за чего бот не сможет выдавать ее!").WithFooter("Поднимите роль бота выше выдаваемой роли.");
                    }
                    else emb.WithDescription("Выставить больше 10 ролей в магазин нельзя!");
                }
                else emb.WithDescription($"Роль {role.Mention} уже продается за {BuyRole.Price} ZeroCoins!");

                _cache.Removes(Context);
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task buyroledel(SocketRole role)
        {
            _cache.Removes(Context);
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("🔨BuyRoleDel");
                var buyrole = DBcontext.RoleSwaps.FirstOrDefault(x => x.RoleId == role.Id && x.GuildId == role.Guild.Id && x.type == RoleSwaps.RoleType.RoleBuy);
                emb.WithDescription($"Роль {role.Mention} {(buyrole != null ? "удалена с продажи" : "не продавалась")}.");
                if (buyrole != null)
                {
                    DBcontext.RoleSwaps.Remove(buyrole);
                    await DBcontext.SaveChangesAsync();
                }
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }
    }
}
