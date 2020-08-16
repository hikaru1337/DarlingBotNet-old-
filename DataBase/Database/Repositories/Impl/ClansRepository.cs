using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DarlingBotNet.Modules;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DarlingBotNet.DataBase
{
    public class ClansRepository : Repository<Clans>, IClansRepository
    {
        public ClansRepository(DbContext context) : base(context)
        {
        }

        public void EnsureCreated(ulong userId, ulong guildId)
        {
            _context.Database.ExecuteSqlRaw($@"INSERT OR IGNORE INTO Users (userid, guildId, ZeroCoin) VALUES ({userId}, {guildId},1000);");
        }

        public Clans GetOrCreate(string clanname, string url, SocketGuildUser Owner)
        {
            var gg = _set.AsEnumerable().FirstOrDefault(x=>x.OwnerId == Owner.Id && x.guildId == Owner.Guild.Id);
            if (gg == null)
            {
                gg = _set.Add(new Clans() { OwnerId = Owner.Id, ClanName = clanname, DateOfCreate = DateTime.Now, ClanMoney = 1000, ClanSlots = 5, guildId = Owner.Guild.Id, LogoUrl = url }).Entity;
            }
            return gg;
        }
        public Clans GetOwnerClan(ulong guildId, ulong userid)
        {
            return _set.AsNoTracking().AsEnumerable().FirstOrDefault(u => u.guildId == guildId && u.OwnerId == userid);
        }

        public Clans GetUserClan(ulong guildId, uint clanid)
        {
            return _set.AsNoTracking().AsEnumerable().FirstOrDefault(u => u.guildId == guildId && u.Id == clanid);
        }

        public IEnumerable<Clans> GetClans(ulong guildId)
        {
            return _set.AsNoTracking().Where(u => u.guildId == guildId);
        }

        public IEnumerable<Clans> GetClans()
        {
            return _set;
        }


        public IEnumerable<Clans> Where(Expression<Func<Clans, bool>> predicate)
        {
            return _set.AsNoTracking().Where(predicate);
        }
    }
}
