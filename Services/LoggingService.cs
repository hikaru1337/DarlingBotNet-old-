using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DarlingBotNet.Services
{
    class LoggingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;

        private string _logDirectory { get; }
        private string _logFile => Path.Combine(_logDirectory, $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt");

        public LoggingService(DiscordSocketClient discord, CommandService Commands)
        {
            _logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            _discord = discord;
            _commands = Commands;

            _discord.Log += OnLogAsync;
            _commands.Log += OnLogAsync;
        }

        private Task OnLogAsync(LogMessage msg)
        {
            if (!Directory.Exists(_logDirectory)) 
                Directory.CreateDirectory(_logDirectory);
            if (!File.Exists(_logFile)) 
                File.Create(_logFile).Dispose();
            ConsoleColor color = ConsoleColor.White;
            switch (msg.Severity.ToString())
            {
                case "Critical":
                case "Warning":
                case "Error":
                    color = ConsoleColor.Red;
                    break;
            }
            Console.ForegroundColor = color;
            string logText = $"{DateTime.UtcNow} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
            Console.WriteLine(logText);
            Console.ForegroundColor = ConsoleColor.White;
             File.AppendAllText(_logFile, logText + "\n");
            return Task.CompletedTask;
        }
    }
}
