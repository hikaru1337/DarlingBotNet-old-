using DarlingBotNet.DataBase;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System.Collections.Generic;

namespace DarlingBotNet.DataBase
{
    public interface IPrivateChannelsRepository : IRepository<PrivateChannels>
    {
        PrivateChannels GetOrCreate(ulong UserId,ulong ChannelId,ulong GuildId);
        PrivateChannels GetOrCreate(ulong UserId, RestVoiceChannel Channel);
        PrivateChannels Get(ulong UserId, RestVoiceChannel Channel);
        PrivateChannels Get(ulong UserId, SocketVoiceChannel Channel);
        IEnumerable<PrivateChannels> Get(ulong guildId);
        PrivateChannels Get(ulong guildId, ulong ChannelId);
        void RemoveRange(ulong GuildId);
    }
}
