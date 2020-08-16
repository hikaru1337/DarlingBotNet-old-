using DarlingBotNet.DataBase;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DarlingBotNet.DataBase
{
    public interface IChannelsRepository : IRepository<Channels>
    {
        Channels GetOrCreate(SocketTextChannel Channel, bool givexp);
        Channels GetId(ulong channelId, ulong guildId);
        Channels Get(SocketTextChannel Channel);
        IEnumerable<Channels> Get(Guilds Guilds);
        void CreateRange(IEnumerable<SocketTextChannel> Channels);
        void CreateRange(SocketGuild Guild);
        void RemoveRange(IEnumerable<SocketTextChannel> Channels);
        //void RemoveRange(IEnumerable<Channels> Channels);
        void RemoveRange(SocketGuild Guild);
        void RemoveRange(ulong GuildId);
        IEnumerable<Channels> Where(Expression<Func<Channels, bool>> predicate);
    }
}
