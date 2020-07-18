using System;
using System.Threading.Tasks;
using DarlingBotNet.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DarlingBotNet
{
    internal class SystemSingleTone
    {
        public SystemSingleTone(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile(BotSettings.config_file);
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public static async Task RunAsync(string[] args)
        {
            await new SystemSingleTone(args).RunAsync();
        }

        public async Task RunAsync()
        {
            var config = new DiscordSocketConfig
            {
                TotalShards = BotSettings.TOTAL_SHARDS
            };

            //var services = new ServiceCollection();             // Create a new instance of a service collection
            //ConfigureServices(services);
            using (var services = ConfigureServices(config))
            {
                //var provider = services.BuildServiceProvider();     // Build the service provider
                services.GetRequiredService<LoggingService>(); // Start the logging service
                services.GetRequiredService<CommandHandler>(); // Start the command handler service

                await services.GetRequiredService<StartUpService>().StartAsync(); // Start the startup service
                var client = services.GetRequiredService<DiscordShardedClient>();
                client.ShardReady += ReadyAsync;
                client.Log += LogAsync;
                client.ShardConnected += Client_ShardConnected;
                await Task.Delay(-1); // Keep the program alive
            }
        }

        private Task Client_ShardConnected(DiscordSocketClient socketClient)
        {
            Console.WriteLine(socketClient.ShardId);
            return Task.CompletedTask;
        }

        private Task ReadyAsync(DiscordSocketClient shard)
        {
            Console.WriteLine($"Shard Number {shard.ShardId} is connected and ready!");
            return Task.CompletedTask;
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices(DiscordSocketConfig config)
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(config))
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    // Add discord to the collection
                    LogLevel = LogSeverity.Verbose, // Tell the logger to give Verbose amount of info
                    MessageCacheSize = 1000, // Cache 1,000 messages per channel
                    DefaultRetryMode = RetryMode.Retry502,
                    ExclusiveBulkDelete = true
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    // Add the command service to the collection
                    LogLevel = LogSeverity.Verbose, // Tell the logger to give Verbose amount of info
                    DefaultRunMode = RunMode.Async // Force all commands to run async by default
                }))
                .AddSingleton<CommandHandler>() // Add the command handler to the collection
                .AddSingleton<StartUpService>() // Add startupservice to the collection
                .AddSingleton<LoggingService>() // Add loggingservice to the collection

                //.AddSingleton<Random>()                 // Add random to the collection
                .AddSingleton(Configuration) // Add the configuration to the collection
                .BuildServiceProvider();
        }
    }
}