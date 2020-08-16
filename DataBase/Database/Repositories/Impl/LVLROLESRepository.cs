using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DarlingBotNet.DataBase
{
    public class LVLROLESRepository : Repository<LVLROLES>, ILVLROLESRepository
    {
        public LVLROLESRepository(DbContext context) : base(context)
        {
        }

        public void EnsureCreated(ulong userId, ulong guildId)
        {
            _context.Database.ExecuteSqlRaw($@"INSERT OR IGNORE INTO Users (userid, guildId, ZeroCoin) VALUES ({userId}, {guildId},1000);");
        }

        public LVLROLES GetIdOrCreate(ulong RoleId, ulong guildId, ulong countlvl)
        {
            var rolez = _set.FirstOrDefault(x=> x.countlvl == countlvl && x.guildid == guildId && x.roleid == RoleId);
            if (rolez == null)
            {
                rolez = new LVLROLES() { countlvl = countlvl, guildid = guildId, roleid = RoleId };
                _set.Add(rolez);
            }
            return rolez;
        }
        public LVLROLES GetOrCreate(SocketRole role, ulong countlvl)
        {
            var rolez = _set.FirstOrDefault(x => x.countlvl == countlvl && x.guildid == role.Guild.Id && x.roleid == role.Id);
            if (rolez == null)
            {
                rolez = new LVLROLES() { countlvl = countlvl, guildid = role.Guild.Id, roleid = role.Id };
                _set.Add(rolez);
            }
            return rolez;
        }

        public LVLROLES Get(SocketRole role, ulong countlvl)
        {
            return _set.FirstOrDefault(u => u.roleid == role.Id && u.guildid == role.Guild.Id && u.countlvl == countlvl);
        }

        public IEnumerable<LVLROLES> Get(ulong countlvl,ulong GuildId)
        {
            return _set.AsNoTracking().Where(u => u.countlvl == countlvl && u.guildid == GuildId);
        }
        public LVLROLES GetId(ulong roleId, ulong guildId, ulong countlvl)
        {
            return _set.FirstOrDefault(u => u.roleid == roleId && u.guildid == guildId && u.countlvl == countlvl);
        }
        public LVLROLES Get(SocketRole role)
        {
            return _set.FirstOrDefault(u => u.roleid == role.Id && u.guildid == role.Guild.Id);
        }

        public IEnumerable<LVLROLES> Get(Guilds Guild)
        {
            return _set.AsNoTracking().Where(u => u.guildid == Guild.guildId);
        }

        public IEnumerable<LVLROLES> Get(SocketGuild Guild)
        {
            return _set.AsNoTracking().Where(u => u.guildid == Guild.Id);
        }
        public LVLROLES GetId(ulong roleId, ulong guildId)
        {
            return _set.FirstOrDefault(u => u.roleid == roleId && u.guildid == guildId);
        }

        public void RemoveRange(ulong guildId)
        {
            var lvls = _set.AsNoTracking().Where(u => u.guildid == guildId);
            _set.RemoveRange(lvls);
        }

        public IEnumerable<LVLROLES> Where(Expression<Func<LVLROLES, bool>> predicate)
        {
            return _set.AsNoTracking().Where(predicate);
        }

        //public void RemoveRange(IEnumerable<LVLROLES> LVLROLES)
        //{
        //    var lists = _set.AsNoTracking().Where(x => LVLROLES.Where(z => z == x) != null);
        //    _set.RemoveRange(lists);
        //}
    }
}
