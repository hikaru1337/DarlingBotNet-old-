using DarlingBotNet.DataBase;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DarlingBotNet.DataBase
{
    public interface IClansRepository : IRepository<Clans>
    {
        Clans GetOrCreate(string clanname,string url,SocketGuildUser Owner);
        Clans GetOwnerClan(ulong guildId,ulong userid);
        Clans GetUserClan(ulong guildId, uint clanid);
        IEnumerable<Clans> GetClans(ulong guildId);
        IEnumerable<Clans> GetClans();

        IEnumerable<Clans> Where(Expression<Func<Clans, bool>> predicate);
    }
}
