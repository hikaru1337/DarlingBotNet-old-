using DarlingBotNet.DataBase;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DarlingBotNet.DataBase
{
    public interface IEmoteClickRepository : IRepository<EmoteClick>
    {
        EmoteClick GetOrCreate(ulong guildId, ulong channelid, string emote, bool GetOrRemove, ulong roleid, ulong messageid);
        IEnumerable<EmoteClick> GetC(ulong guildId, ulong channelid);
        IEnumerable<EmoteClick> GetCS(ulong guildId, IEnumerable<Channels> channel);
        IEnumerable<EmoteClick> GetM(ulong guildId, ulong messageid);
        IEnumerable<EmoteClick> Where(Expression<Func<EmoteClick, bool>> predicate);
        void RemoveRange(ulong GuildId);
        //void RemoveRange(IEnumerable<EmoteClick> Emotes);
        IEnumerable<EmoteClick> GetR(SocketRole Role);
    }
}
