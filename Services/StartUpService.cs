using DarlingBotNet.DataBase;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DarlingBotNet.Services
{
    class StartUpService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        public StartUpService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands, IConfigurationRoot config)
        {
            _provider = provider;
            _config = config;
            _discord = discord;
            _commands = commands;
        }

        public async Task StartAsync()
        {
            using (var mContext = new DBcontext())
            {
                mContext.Database.Migrate();
                mContext.SaveChanges();
                mContext.Dispose();
            }
            string discordToken = _config["tokens:discord"];
            if (string.IsNullOrWhiteSpace(discordToken)) throw new Exception("нет токена!");
            await _discord.LoginAsync(TokenType.Bot, discordToken);
            await _discord.StartAsync();
            await _discord.SetGameAsync("docs.darlingbot.ru");
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
            
        }
    }
}
