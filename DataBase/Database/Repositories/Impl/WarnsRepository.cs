using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DarlingBotNet.DataBase
{
    public class WarnsRepository : Repository<Warns>, IWarnsRepository
    {
        public WarnsRepository(DbContext context) : base(context)
        {
        }

        public void EnsureCreated(ulong userId, ulong guildId)
        {
            _context.Database.ExecuteSqlRaw($@"INSERT OR IGNORE INTO Users (userid, guildId, ZeroCoin) VALUES ({userId}, {guildId},1000);");
        }

        public Warns GetOrCreate(ulong CountWarn, ulong guildId,string report)
        {
            //EnsureCreated(userId, guildId);
            var warn = new Warns() { guildId = guildId, CountWarn = CountWarn, ReportWarn = report };
            _set.Add(warn);
            return warn;
        }
        public IEnumerable<Warns> Get(ulong guildId)
        {
            return _set.AsNoTracking().Where(u => u.guildId == guildId);
        }

        public Warns Get(ulong guildId, ulong CountWarn)
        {
            return _set.FirstOrDefault(u => u.guildId == guildId && u.CountWarn == CountWarn);
        }

        public void RemoveRange(ulong GuildId)
        {
            var lists = _set.AsNoTracking().Where(x => x.guildId == GuildId);
            _set.RemoveRange(lists);
        }
    }
}
