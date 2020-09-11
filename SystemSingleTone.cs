using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DarlingBotNet.DataBase;
using DarlingBotNet.Services;
using DarlingBotNet.Services.Sys;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace DarlingBotNet
{
    class SystemSingleTone
    {

        public IConfigurationRoot Configuration { get; }

        public SystemSingleTone(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddYamlFile(BotSettings.config_file);
            Configuration = builder.Build();
        }

        public static async Task RunAsync(string[] args)
        {
            
            await new SystemSingleTone(args).RunAsync();
        }

        public async Task RunAsync()
        {
            //Assembly.LoadFile(@"C:\Users\kisa2\source\repos\DarlingBotNet\DarlingBotNet\bin\Debug\netcoreapp3.1\System.Linq.Async.dll");
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
                    return new ServiceCollection()
                    .AddMemoryCache()
                    .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                    {                                       // Add discord to the collection
                        LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                        MessageCacheSize = 128,             // Cache 1,000 messages per channel
                        DefaultRetryMode = RetryMode.Retry502,
                        ExclusiveBulkDelete = true
                    }))
                    //.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                    //.AddSingleton(s =>
                    //{
                    //    var provider = s.GetRequiredService<ObjectPoolProvider>();
                    //    return provider.Create(new DbContext());
                    //})
                    .AddSingleton(new CommandService(new CommandServiceConfig
                    {                                       // Add the command service to the collection
                        LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                        DefaultRunMode = RunMode.Async,     // Force all commands to run async by default
                    }))
                    .AddSingleton<CommandHandler>()         // Add the command handler to the collection
                    .AddSingleton<StartUpService>()         // Add startupservice to the collection
                    .AddSingleton<LoggingService>()         // Add loggingservice to the collection
                    .AddSingleton<DBcontext>()
                    .AddSingleton<DbContext>()
                    //.AddSingleton<Random>()                 // Add random to the collection
                    .AddSingleton(Configuration)           // Add the configuration to the collection
                    .BuildServiceProvider();
            
        }


    }
}
