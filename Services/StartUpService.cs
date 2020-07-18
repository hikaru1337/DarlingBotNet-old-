using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DarlingBotNet.Services
{
    internal class StartUpService
    {
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _provider;

        public StartUpService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands,
            IConfigurationRoot config)
        {
            _provider = provider;
            _config = config;
            _discord = discord;
            _commands = commands;
        }

        public async Task StartAsync()
        {
            string discordToken = _config["tokens:discord"];
            if (string.IsNullOrWhiteSpace(discordToken)) throw new Exception("нет токена!");
            await _discord.LoginAsync(TokenType.Bot, discordToken);
            await _discord.StartAsync();
            await _discord.SetGameAsync("h.u");
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }
    }
}