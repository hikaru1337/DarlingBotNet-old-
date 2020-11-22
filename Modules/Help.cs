using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.Services;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ServiceStack;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DarlingBotNet.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IServiceProvider _provider;
        private readonly IMemoryCache _cache;


        public Help(CommandService service, IServiceProvider provider, IMemoryCache cache)
        {
            _service = service;
            _provider = provider;
            _cache = cache;
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task modules()
        {

            string prefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("📚Модули бота")
                                        .WithFooter($"Команды модуля - {prefix}c [Имя модуля]");
            var mdls = _service.Modules;
            foreach (var mdl in mdls)
            {
                if (mdl.GetExecutableCommandsAsync(Context, _provider).Result.Count > 0)
                    emb.Description += $"{mdl.Name}\n";
            }
            if (string.IsNullOrWhiteSpace(emb.Description)) emb.WithDescription("Модули бота отсутствуют!");
            _cache.Removes(Context);
            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task commands(string modules)
        {

            var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id);
            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"📜{modules} - Команды");

            var mdls = _service.Modules.FirstOrDefault(x => x.Name.ToLower() == modules.ToLower());
            if (mdls != null && mdls.GetExecutableCommandsAsync(Context, _provider).Result.Count > 0)
            {
                foreach (var cmd in mdls.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context, _provider);
                    var rez = Guild.CommandInviseList.FirstOrDefault(x => x == cmd.Aliases.First());
                    if (result.IsSuccess && rez == null)
                        emb.Description += $"{Guild.Prefix}{cmd.Aliases.First()} {(cmd.Aliases.Last() != null ? $"({cmd.Aliases.Last()})" : "-")}\n";
                }
                emb.WithFooter($"Информация о команде - {Guild.Prefix}i [Имя команды]");
            }
            else emb.WithDescription($"Модуль {modules} не найден!").WithAuthor($"📜{modules} - ошибка");
            _cache.Removes(Context);
            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task info(string command)
        {
            var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id);
            var Command = _service.Commands.Where(x => x.Aliases.ElementAt(0).ToLower() == command.ToLower() || (x.Aliases.Count > 1 ? x.Aliases.ElementAt(1) : x.Aliases.ElementAt(0)).ToLower() == command.ToLower()).FirstOrDefault();
            var emb = new EmbedBuilder().WithAuthor($"📋Информация о {command}");

            if (Command != null)
            {
                if (Guild.CommandInviseList.FirstOrDefault(x => x == Command.Aliases.First()) == null)
                {
                    string text = null;
                    foreach (var Parameter in Command.Parameters)
                    {
                        if (Parameter.IsOptional)
                            text += $"[{Parameter}/null] ";
                        else
                            text += $"[{Parameter}] ";

                    }
                    emb.AddField($"Сокращение: {Command.Remarks.Replace('"', ' ')}",
                                 $"Описание: {Command.Summary}\n" +
                                 $"Пример: {Guild.Prefix}{Command.Name} {text}");
                }
                else emb.WithDescription($"Команда `{command}` отключена создаталем сервера!");
            }
            else emb.WithDescription($"Команда `{command}` не найдена!");

            _cache.Removes(Context);
            await Context.Channel.SendMessageAsync("", false, emb.WithColor(255, 0, 94).Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task use()
        {
            var GuildPrefix = _cache.GetOrCreateGuldsCache(Context.Guild.Id).Prefix;
            _cache.Removes(Context);
            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder().WithColor(255, 0, 94)
                                                                                .WithAuthor($"Информация о боте {Context.Client.CurrentUser.Username}🌏", Context.Client.CurrentUser.GetAvatarUrl())
                                                                                .WithDescription(string.Format(OtherSettings.WelcomeText, GuildPrefix))
                                                                                .WithImageUrl(BotSettings.bannerBoturl)
                                                                                .Build());

        }
    }
}
