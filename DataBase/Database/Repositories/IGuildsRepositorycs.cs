using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DarlingBotNet.DataBase.Database.Repositories
{
    public interface IGuildsRepository : IRepository<Guilds>
    {
        Guilds GetOrCreate(ulong guildId);
        Guilds GetOrCreate(SocketGuild guild);
        Guilds Get(ulong guildId);
        Guilds Get(SocketGuild guild);
        IEnumerable<Guilds> Where(Expression<Func<Guilds, bool>> predicate);
    }
}
