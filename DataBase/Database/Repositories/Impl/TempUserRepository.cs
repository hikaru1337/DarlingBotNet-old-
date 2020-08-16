using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DarlingBotNet.DataBase
{
    public class TempUserRepository : Repository<TempUser>, ITempUserRepository
    {
        public TempUserRepository(DbContext context) : base(context)
        {
        }

        public void EnsureCreated(ulong userId, ulong guildId)
        {
            _context.Database.ExecuteSqlRaw($@"INSERT OR IGNORE INTO Users (userid, guildId, ZeroCoin) VALUES ({userId}, {guildId},1000);");
        }

        public TempUser GetOrCreate(ulong guildId,ulong userId,DateTime times,string Reason)
        {
            var t = _set.FirstOrDefault(x => x.guildId == guildId && x.userId == userId && x.ToTime == times && x.Reason == Reason);
            if (t == null)
            {
                t = new TempUser() { guildId = guildId, userId = userId, ToTime = times, Reason = Reason };
                _set.Add(t);
            }
            return t;
        }
        public IEnumerable<TempUser> Get(ulong guildId)
        {
            return _set.AsNoTracking().Where(u => u.guildId == guildId);
        }

        public TempUser Get(TempUser tempuser)
        {
            return _set.AsNoTracking().FirstOrDefault(u => u == tempuser);
        }

        public TempUser Get(ulong guildId, ulong userId, DateTime times, string Reason)
        {
            return _set.FirstOrDefault(u => u.guildId == guildId && u.userId == userId && u.ToTime == times && u.Reason == Reason);
        }

        public void RemoveRange(ulong GuildId)
        {
            var lists = _set.AsNoTracking().Where(x => x.guildId == GuildId);
            _set.RemoveRange(lists);
        }
    }
}
