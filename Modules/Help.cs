using System;
using System.Linq;
using System.Threading.Tasks;
using DarlingBotNet.DataBase;
using DarlingBotNet.Services;
using Discord;
using Discord.Commands;

namespace DarlingBotNet.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider _provider;
        private readonly CommandService _service;

        public Help(CommandService service, IServiceProvider provider)
        {
            _service = service;
            _provider = provider;
        }

        [Aliases]
        [Commands]
        [Usage]
        [Descriptions]
        [PermissionBlockCommand]
        public async Task modules()
        {
            var glds = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("📚Все Модули")
                .WithFooter($"Выбрать модуль - {glds.Prefix}c [Имя модуля]");
            string description = null;
            var succ = false;
            foreach (var module in _service.Modules)
            {
                foreach (var command in module.Commands) // Check if there are any commands
                {
                    var result = await command.CheckPreconditionsAsync(Context, _provider);
                    if (result.IsSuccess)
                        succ = true;
                }

                if (succ)
                {
                    description += $"{module.Name}\n";
                    succ = false;
                }
            }

            await Context.Channel.SendMessageAsync("", false, emb.WithDescription(description).Build());
        }

        [Aliases]
        [Commands]
        [Usage]
        [Descriptions]
        [PermissionBlockCommand]
        public async Task commands(string modules)
        {
            var Guild = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"📜{modules} - Команды");
            var description = "";

            var mdls = _service.Modules.Where(x => x.Name.ToLower() == modules.ToLower());
            var es = false;
            if (mdls.Count() == 0)
            {
                es = true;
            }
            else
            {
                var res = mdls.First().GetExecutableCommandsAsync(Context, _provider).Result;
                if (res.Count == 0)
                {
                    es = true;
                }
                else
                {
                    foreach (var cmd in mdls.First().Commands)
                    {
                        var result = await cmd.CheckPreconditionsAsync(Context, _provider);
                        if (!result.IsSuccess)
                            es = true;

                        foreach (var cmdOff in Guild.CommandInviseList)
                            if (cmd.Aliases.First() == cmdOff)
                                es = true;
                        if (!es)
                            description +=
                                $"{Guild.Prefix}{cmd.Aliases.First()} {(cmd.Aliases.Last() != null ? $"({cmd.Aliases.Last()})" : "-")}\n";
                        else
                            es = false;
                    }

                    emb.WithDescription(description)
                        .WithFooter($"Информация о команде - {Guild.Prefix}i [Имя команды]");
                    es = false;
                }
            }

            if (es) emb.WithDescription($"Модуль {modules} не найден!").WithAuthor($"📜{modules} - ошибка");
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases]
        [Commands]
        [Usage]
        [Descriptions]
        [PermissionBlockCommand]
        public async Task info(string command)
        {
            var Guild = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id);
            var Command = _service.Commands.Where(x =>
                    x.Aliases.ElementAt(0).ToLower() == command.ToLower() ||
                    (x.Aliases.Count > 1 ? x.Aliases.ElementAt(1) : x.Aliases.ElementAt(0)).ToLower() ==
                    command.ToLower())
                .FirstOrDefault();
            var emb = new EmbedBuilder().WithAuthor($"📋Информация о {command}");

            if (Command != null)
            {
                if (Guild.CommandInviseList.Where(x => x == Command.Aliases.First()).Count() == 0)
                {
                    var scr1 = Command.Summary.Split("||")[1] == null ? "Отсутствуют" : Command.Summary.Split("||")[1];

                    emb.AddField($"Сокращение: {Command.Remarks.Replace('"', ' ')}",
                        $"Описание: {Command.Summary.Split("||")[0]}\n" +
                        $"Пример: {Guild.Prefix}{command} {scr1}");
                }
                else
                {
                    emb.WithDescription($"Команда `{command}` отключена создаталем сервера!");
                }
            }
            else
            {
                emb.WithDescription($"Команда `{command}` не найдена!");
            }

            await Context.Channel.SendMessageAsync("", false, emb.WithColor(255, 0, 94).Build());
        }

        [Aliases]
        [Commands]
        [Usage]
        [Descriptions]
        [PermissionBlockCommand]
        public async Task use()
        {
            var prefix = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == Context.Guild.Id).Prefix;
            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder().WithColor(255, 0, 94)
                .WithAuthor($"Информация о боте {Context.Client.CurrentUser.Username}🌏",
                    Context.Client.CurrentUser.GetAvatarUrl())
                .WithDescription(string.Format(SystemLoading.WelcomeText, prefix))
                .WithImageUrl(BotSettings.bannerBoturl)
                .Build());
        }
    }
}