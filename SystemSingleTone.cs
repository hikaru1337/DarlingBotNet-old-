using System;
using System.IO;
using System.Threading.Tasks;
using DarlingBotNet.DataBase.Database;
using DarlingBotNet.Services;
using DarlingBotNet.Services.Sys;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace DarlingBotNet
{
    class SystemSingleTone
    {
        private readonly DbService _db;
        public IConfigurationRoot Configuration { get; }

        public SystemSingleTone(string[] args)
        {
            _db = new DbService();
            var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddYamlFile(BotSettings.config_file);
            Configuration = builder.Build();
        }

        public static async Task RunAsync(string[] args)
        {
            
            await new SystemSingleTone(args).RunAsync();
        }

        public async Task RunAsync()
        {
            var services = ConfigureServices();             // Create a new instance of a service collection
            //ConfigureServices(services);

            services.GetRequiredService<LoggingService>();      // Start the logging service
            services.GetRequiredService<CommandHandler>(); 		// Start the command handler service

            await services.GetRequiredService<StartUpService>().StartAsync();       // Start the startup service
            var client = services.GetRequiredService<DiscordSocketClient>();
            //client.Log += LogAsync;
                await Task.Delay(-1);      
        }


        //private Task LogAsync(LogMessage log)
        //{
        //    Console.WriteLine(log);
        //    return Task.CompletedTask;
        //}

        private ServiceProvider ConfigureServices()
        {
            
            using (var uow = _db.GetDbContext())
            {
                return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {                                       // Add discord to the collection
                    LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                    MessageCacheSize = 128,             // Cache 1,000 messages per channel
                    DefaultRetryMode = RetryMode.Retry502,
                    ExclusiveBulkDelete = true
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {                                       // Add the command service to the collection
                    LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                    DefaultRunMode = RunMode.Async,     // Force all commands to run async by default
                }))
                .AddSingleton<CommandHandler>()         // Add the command handler to the collection
                .AddSingleton<StartUpService>()         // Add startupservice to the collection
                .AddSingleton<LoggingService>()         // Add loggingservice to the collection
                .AddSingleton(uow)
                .AddSingleton(_db)
                //.AddSingleton<Random>()                 // Add random to the collection
                .AddSingleton(Configuration)           // Add the configuration to the collection
                .BuildServiceProvider();
            }
        }
    }
}
