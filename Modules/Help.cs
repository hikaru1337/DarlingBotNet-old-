using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.Services;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarlingBotNet.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IServiceProvider _provider;
        private readonly DbService _db;

        public Help(CommandService service, IServiceProvider provider, DbService db)
        {
            _service = service;
            _provider = provider;
            _db = db;
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task modules()
        {
            using (var DBcontext = _db.GetDbContext())
            {
                string prefix = DBcontext.Guilds.Get(Context.Guild).Prefix;
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("📚Все Модули")
                                            .WithFooter($"Выбрать модуль - {prefix}c [Имя модуля]");
                string description = null;
                foreach (var module in _service.Modules)
                {
                    foreach (var command in module.Commands) // Check if there are any commands
                    {
                        var result = await command.CheckPreconditionsAsync(Context, _provider);
                        if (result.IsSuccess)
                        {
                            description += $"{module.Name}\n";
                            break;
                        }
                    }
                }
                await Context.Channel.SendMessageAsync("", false, emb.WithDescription(description).Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task commands(string modules)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var Guild = DBcontext.Guilds.Get(Context.Guild);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"📜{modules} - Команды");
                string description = "";

                var mdls = _service.Modules.Where(x => x.Name.ToLower() == modules.ToLower());
                bool es = false;
                if (mdls.Count() == 0) es = true;
                else
                {
                    var res = mdls.First().GetExecutableCommandsAsync(Context, _provider).Result;
                    if (res.Count == 0) es = true;
                    else
                    {
                        foreach (var cmd in mdls.First().Commands)
                        {
                            var result = await cmd.CheckPreconditionsAsync(Context, _provider);
                            if (!result.IsSuccess)
                                es = true;

                            foreach (var cmdOff in Guild.CommandInviseList)
                            {
                                if (cmd.Aliases.First() == cmdOff)
                                    es = true;
                            }
                            if (!es)
                                description += $"{Guild.Prefix}{cmd.Aliases.First()} {(cmd.Aliases.Last() != null ? $"({cmd.Aliases.Last()})" : "-")}\n";
                            else
                                es = false;

                        }
                        emb.WithDescription(description).WithFooter($"Информация о команде - {Guild.Prefix}i [Имя команды]");
                        es = false;
                    }
                }

                if (es) emb.WithDescription($"Модуль {modules} не найден!").WithAuthor($"📜{modules} - ошибка");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task info(string command)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var Guild = DBcontext.Guilds.Get(Context.Guild);
                var Command = _service.Commands.Where(x => x.Aliases.ElementAt(0).ToLower() == command.ToLower() || (x.Aliases.Count > 1 ? x.Aliases.ElementAt(1) : x.Aliases.ElementAt(0)).ToLower() == command.ToLower()).FirstOrDefault();
                var emb = new EmbedBuilder().WithAuthor($"📋Информация о {command}");

                if (Command != null)
                {
                    if (Guild.CommandInviseList.Where(x => x == Command.Aliases.First()).Count() == 0)
                    {
                        string scr1 = Command.Summary.Split("||").Count() > 0 ? "" : Command.Summary.Split("||")[1];

                        emb.AddField($"Сокращение: {Command.Remarks.Replace('"', ' ')}",
                                     $"Описание: {Command.Summary.Split("||")[0]}\n" +
                                     $"Пример: {Guild.Prefix}{command} {scr1}");
                    }
                    else emb.WithDescription($"Команда `{command}` отключена создаталем сервера!");

                }
                else emb.WithDescription($"Команда `{command}` не найдена!");

                await Context.Channel.SendMessageAsync("", false, emb.WithColor(255, 0, 94).Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task use()
        {
            using (var DBcontext = _db.GetDbContext())
            {
                string prefix = DBcontext.Guilds.Get(Context.Guild).Prefix;
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder().WithColor(255, 0, 94)
                                                                                    .WithAuthor($"Информация о боте {Context.Client.CurrentUser.Username}🌏", Context.Client.CurrentUser.GetAvatarUrl())
                                                                                    .WithDescription(string.Format(SystemLoading.WelcomeText, prefix))
                                                                                    .WithImageUrl(BotSettings.bannerBoturl)
                                                                                    .Build());
            }
        }
    }
}
