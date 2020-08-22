using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DarlingBotNet.DataBase
{
    public class UsersRepository : Repository<Users>, IUsersRepository
    {
        public UsersRepository(DbContext context) : base(context)
        {
        }

        public void EnsureCreated(ulong userId, ulong guildId)
        {
            _context.Database.ExecuteSqlRaw($@"INSERT OR IGNORE INTO Users (userid, guildId, ZeroCoin) VALUES ({userId}, {guildId},1000);");
        }

        public Users GetOrCreate(ulong userId, ulong guildId)
        {
            //EnsureCreated(userId, guildId);
            var gg = _set.AsNoTracking().FirstOrDefault(u => u.userid == userId && u.guildId == guildId);
            if (gg == null)
            {
                gg = new Users() { userid = userId, guildId = guildId, ZeroCoin = 1000 };
                _set.Add(gg);
            }                
            return gg;
        }

        public Users GetOrCreate(SocketGuildUser user)
        {
            var usr = _set.AsNoTracking().Where(u => u.userid == user.Id && u.guildId == user.Guild.Id).First();
            if (usr == null)
            {
                usr = _set.Add(new Users() { userid = user.Id, ZeroCoin = 1000, guildId = user.Guild.Id }).Entity;
            }
            return usr;
        }
        public Users Get(ulong userId, ulong guildId)
        {
            return _set.FirstOrDefault(u => u.userid == userId && u.guildId == guildId);
        }
        public IEnumerable<Users> Get(ulong guildId)
        {
            return _set.AsNoTracking().Where(u => u.guildId == guildId);
        }
        public IEnumerable<Users> Where(Expression<Func<Users, bool>> predicate)
        {
            return _set.AsNoTracking().Where(predicate);
        }
    }
}
