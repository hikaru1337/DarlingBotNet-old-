using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SixLabors.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;
using DarlingBotNet.Services;
using System.Numerics;
using DarlingBotNet.Services.Sys;
using DarlingBotNet.DataBase;
using Microsoft.Extensions.Caching.Memory;
using DarlingBotNet.DataBase.Database;

namespace DarlingBotNet.Modules
{
    [PrivateChannelBlock]
    public class PrivateChannels : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _provider;
        private readonly IMemoryCache _cache;
        public PrivateChannels(DiscordSocketClient discord, IServiceProvider provider, IMemoryCache cache)
        {
            _provider = provider;
            _discord = discord;
            _cache = cache;
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task PCuserblock(SocketGuildUser user)
        {
            using (var DBcontext = new DBcontext())
            { 
                _cache.Removes(Context);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"{user.Mention} PrivateChannel blocked").WithDescription("Пользователю заблокирован доступ к каналу");
                var prc = DBcontext.PrivateChannels.FirstOrDefault(x=>x.userid == user.Id && x.guildid == user.Guild.Id);
                var chnl = Context.Guild.GetVoiceChannel(prc.channelid);
                await chnl.AddPermissionOverwriteAsync(user, new OverwritePermissions(connect: PermValue.Deny));
                if (chnl.Id == user.VoiceChannel.Id)
                    await user.VoiceChannel.DisconnectAsync();
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [PermissionBlockCommand]
        public async Task PCuserget(SocketGuildUser user)
        {
            using (var DBcontext = new DBcontext())
            {
                _cache.Removes(Context);
                var emb = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"{user.Mention} PrivateChannel Gived").WithDescription("Пользователю выдан доступ к каналу");
                var prc = DBcontext.PrivateChannels.FirstOrDefault(x => x.userid == user.Id && x.guildid == user.Guild.Id);
                var chnl = Context.Guild.GetVoiceChannel(prc.channelid);
                await chnl.AddPermissionOverwriteAsync(user, new OverwritePermissions(connect: PermValue.Allow));
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }
    }
}
