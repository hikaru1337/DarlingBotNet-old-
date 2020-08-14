using DarlingBotNet.DataBase;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DarlingBotNet.DataBase
{
    public interface IUsersRepository : IRepository<Users>
    {
        Users GetOrCreate(ulong userid,ulong guildId);
        Users Get(ulong userid, ulong guildId);
        IEnumerable<Users> Get(ulong guildId);

        IEnumerable<Users> Where(Expression<Func<Users, bool>> predicate);
        Users GetOrCreate(SocketGuildUser user);
    }
}
