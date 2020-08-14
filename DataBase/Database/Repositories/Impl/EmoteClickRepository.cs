using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DarlingBotNet.DataBase
{
    public class EmoteClickRepository : Repository<EmoteClick>, IEmoteClickRepository
    {
        public EmoteClickRepository(DbContext context) : base(context)
        {
        }

        public void EnsureCreated(ulong userId, ulong guildId)
        {
            _context.Database.ExecuteSqlRaw($@"INSERT OR IGNORE INTO Users (userid, guildId, ZeroCoin) VALUES ({userId}, {guildId},1000);");
        }

        public EmoteClick GetOrCreate(ulong guildId, ulong channelid, string emote, bool GetOrRemove,ulong roleid,ulong messageid)
        {
            var emc = new EmoteClick() { guildid = guildId, channelid = channelid, emote = emote, get = GetOrRemove, roleid = roleid, messageid = messageid };
            _set.Add(emc);
            //EnsureCreated(userId, guildId);
            return emc;
        }
        public IEnumerable<EmoteClick> GetM(ulong guildId, ulong messageid)
        {
            return _set.AsNoTracking().Where(u => u.guildid == guildId && u.messageid == messageid);
        }
        public IEnumerable<EmoteClick> GetC(ulong guildId, ulong channelid)
        {
            return _set.AsNoTracking().Where(u => u.guildid == guildId && u.channelid == channelid);
        }


        public IEnumerable<EmoteClick> GetCS(ulong guildId, IEnumerable<Channels> channels)
        {
            return _set.AsNoTracking().Where(u => u.guildid == guildId ).ToList().Where(u=> channels.Where(x=>x.channelid == u.channelid) != null);
        }

        public IEnumerable<EmoteClick> GetR(SocketRole Role)
        {
            return _set.AsNoTracking().Where(u => u.guildid == Role.Guild.Id && u.roleid == Role.Id);
        }
        public void RemoveRange(ulong GuildId)
        {
            var lists = _set.AsNoTracking().Where(x => x.guildid == GuildId);
            _set.RemoveRange(lists);
        }

        //public void RemoveRange(IEnumerable<EmoteClick> Emotes)
        //{
        //    var lists = _set.AsNoTracking().Where(x => Emotes.Where(z => z == x) != null);
        //    _set.RemoveRange(lists);
        //}
        public IEnumerable<EmoteClick> Where(Expression<Func<EmoteClick, bool>> predicate)
        {
            return _set.AsNoTracking().Where(predicate);
        }
    }
}
