using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DarlingBotNet.Services;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DarlingBotNet.DataBase.Database.Repositories.Impl
{
    public class GuildsRepository : Repository<Guilds>, IGuildsRepository
    {
        public GuildsRepository(DbContext context) : base(context)
        {
        }

        public void EnsureCreated(ulong guildId)
        {
            _context.Database.ExecuteSqlRaw($@"INSERT OR IGNORE INTO Guilds (guildId,GiveXPnextChannel,Prefix) VALUES ({guildId},true,h.);");
        }

        public Guilds GetOrCreate(SocketGuild Guild)
        {
            //EnsureCreated(guildId);
            var gg = _set.FirstOrDefault(x => x.guildId == Guild.Id);
            if (gg == null)
            {
                gg = new Guilds() { guildId = Guild.Id, GiveXPnextChannel = true, Prefix = BotSettings.Prefix };
                _set.Add(gg);
            }    
            return gg;
        }

        public Guilds GetOrCreate(ulong GuildId)
        {
            var gg = _set.FirstOrDefault(x => x.guildId == GuildId);
            if (gg == null)
            {
                gg = new Guilds() { guildId = GuildId, GiveXPnextChannel = true, Prefix = BotSettings.Prefix };
                _set.Add(gg);
            }
            return gg;
        }
        public Guilds Get(SocketGuild Guild)
        {
            return _set.FirstOrDefault(u => u.guildId == Guild.Id);
        }

        public Guilds Get(ulong GuildId)
        {
            return _set.FirstOrDefault(u => u.guildId == GuildId);
        }

        public IEnumerable<Guilds> Where(Expression<Func<Guilds, bool>> predicate)
        {
            return _set.AsNoTracking().Where(predicate);
        }
    }
}
