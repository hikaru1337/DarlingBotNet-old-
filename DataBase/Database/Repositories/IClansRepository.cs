using DarlingBotNet.DataBase;
using Discord;
using System.Collections.Generic;

namespace DarlingBotNet.DataBase
{
    public interface IClansRepository : IRepository<Clans>
    {
        //Clans GetOrCreate(ulong CountWarn, ulong guildId, string report);
        //IEnumerable<Clans> Get(ulong guildId);
        //Clans Get(ulong guildId, ulong CountWarn);

        //void RemoveRange(ulong GuildId);
    }
}
