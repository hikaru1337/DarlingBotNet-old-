//using DarlingBotNet.DataBase.Database;
//using DarlingBotNet.Services;
//using Discord;
//using Discord.Commands;
//using Discord.WebSocket;
//using Microsoft.Extensions.Caching.Memory;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;

//namespace DarlingBotNet.Modules
//{
//    public class Behavior : ModuleBase<SocketCommandContext>
//    {
//        private readonly DiscordSocketClient _discord;
//        private readonly IServiceProvider _provider;
//        private readonly IMemoryCache _cache;
//        public Behavior(DiscordSocketClient discord, IServiceProvider provider, IMemoryCache cache)
//        {
//            _provider = provider;
//            _discord = discord;
//            _cache = cache;
//        }

//        [Aliases, Commands, Usage, Descriptions]
//        [PermissionBlockCommand]
//        [RequireUserPermission(GuildPermission.BanMembers)]
//        public async Task BehaviorSettings()
//        {
//            var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"🔨BehaviorSettings");
//            var Guild = _cache.GetOrCreateGuldsCache(Context.Guild.Id);
//            var Channel = Context.Guild.GetTextChannel(Guild.Behavior_SendWantedJoinToChannelId);
//            emb.AddField("Канал для предупреждений о входе пользователя с плохой репутацией",$"{(Channel == null ? "Отсутствует" : Channel.Mention )}",true);
//            emb.AddField("Выдавать Мут роли потенциальным Ботам?","",true);
//            _cache.Removes(Context);
//            await Context.Channel.SendMessageAsync("", false, emb.Build());
//        }
//    }
//}
