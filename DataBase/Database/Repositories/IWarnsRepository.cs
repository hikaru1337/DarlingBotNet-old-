using DarlingBotNet.DataBase;
using Discord;
using System.Collections.Generic;

namespace DarlingBotNet.DataBase
{
    public interface IWarnsRepository : IRepository<Warns>
    {
        Warns GetOrCreate(ulong CountWarn, ulong guildId, string report);
        IEnumerable<Warns> Get(ulong guildId);
        Warns Get(ulong guildId, ulong CountWarn);

        void RemoveRange(ulong GuildId);
    }
}
