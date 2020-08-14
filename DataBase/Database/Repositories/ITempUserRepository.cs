using DarlingBotNet.DataBase;
using Discord;
using System;
using System.Collections.Generic;

namespace DarlingBotNet.DataBase
{
    public interface ITempUserRepository : IRepository<TempUser>
    {
        TempUser GetOrCreate(ulong guildId, ulong userId, DateTime times, string Reason);
        IEnumerable<TempUser> Get(ulong guildId);
        TempUser Get(ulong guildId, ulong userId, DateTime times, string Reason);
        void RemoveRange(ulong GuildId);

        TempUser Get(TempUser tempuser);
    }
}
