using DarlingBotNet.DataBase;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DarlingBotNet.DataBase
{
    public interface ILVLROLESRepository : IRepository<LVLROLES>
    {
        LVLROLES GetOrCreate(SocketRole role,ulong countlvl);
        LVLROLES GetIdOrCreate(ulong RoleId, ulong guildId, ulong countlvl);
        LVLROLES Get(SocketRole role,ulong countlvl);
        IEnumerable<LVLROLES> Get(ulong countlvl, ulong GuildId);
        LVLROLES GetId(ulong RoleId,ulong GuildId, ulong countlvl);
        LVLROLES Get(SocketRole role);
        IEnumerable<LVLROLES> Get(Guilds Guild);
        IEnumerable<LVLROLES> Get(SocketGuild Guild);
        LVLROLES GetId(ulong RoleId, ulong GuildId);
        void RemoveRange(ulong GuildId);
        //void RemoveRange(IEnumerable<LVLROLES> Emotes);
        IEnumerable<LVLROLES> Where(Expression<Func<LVLROLES, bool>> predicate);

        

    }
}
