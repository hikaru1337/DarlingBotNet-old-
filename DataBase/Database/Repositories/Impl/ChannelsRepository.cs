using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Channels;
using DarlingBotNet.DataBase.Database;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DarlingBotNet.DataBase
{
    public class ChannelsRepository : Repository<Channels>, IChannelsRepository
    {

        public ChannelsRepository(DbContext context) : base(context)
        {
        }

        public void EnsureCreated(ulong channelid, ulong guildId,bool givexp, string channeltype)
        {
            _context.Database.ExecuteSqlRaw($@"INSERT OR IGNORE INTO Channels (channelid, guildId, GiveXP,UseCommand,channeltype) VALUES ({channelid}, {guildId},{givexp},true,{channeltype});");
        }

        public Channels GetOrCreate(SocketTextChannel Channels, bool givexp)
        {
            var chnl = _set.FirstOrDefault(u => u.channelid == Channels.Id && u.guildid == Channels.Guild.Id);
            if (chnl == null)
            {
                chnl = new Channels() { guildid = Channels.Guild.Id, channelid = Channels.Id, GiveXP = givexp, UseCommand = true};
                _set.Add(chnl);
            }
            return chnl;
        }
        public Channels GetId(ulong channelid, ulong guildId)
        {
            return _set.FirstOrDefault(u => u.channelid == channelid && u.guildid == guildId);
        }
        public Channels Get(SocketTextChannel Channel)
        {
            return _set.FirstOrDefault(u => u.channelid == Channel.Id && u.guildid == Channel.Guild.Id);
        }

        public IEnumerable<Channels> Get(Guilds Guilds)
        {
            return _set.AsNoTracking().Where(u => u.guildid == Guilds.guildId);
        }

        public void CreateRange(IEnumerable<SocketTextChannel> Channels)
        {
            var lists = new List<Channels>();
            foreach (var TextChannel in Channels)
            {
                lists.Add(new Channels() { guildid = TextChannel.Guild.Id, channelid = TextChannel.Id, GiveXP = true, UseCommand = true });
            }
            _set.AddRange(lists);
        }

        public void CreateRange(SocketGuild Guild)
        {
            var lists = new List<Channels>();
            foreach (var TextChannel in Guild.TextChannels)
            {
                lists.Add(new Channels() { guildid = TextChannel.Guild.Id, channelid = TextChannel.Id, GiveXP = true, UseCommand = true });
            }
            _set.AddRange(lists);
        }

        public void RemoveRange(SocketGuild Guild)
        {
            var lists = _set.AsNoTracking().Where(x => Guild.Channels.Where(z => z.Id == x.channelid) != null);
            _set.RemoveRange(lists);
        }

        public void RemoveRange(IEnumerable<SocketTextChannel> Channels)
        {
            var lists = _set.AsNoTracking().Where(x => Channels.Where(z => z.Id == x.channelid) != null);
            if (lists != null) _set.RemoveRange(lists);
        }

        public void RemoveRange(ulong GuildId)
        {
            var lists = _set.AsNoTracking().Where(x => x.guildid == GuildId);
            _set.RemoveRange(lists);
        }

        public IEnumerable<Channels> Where(Expression<Func<Channels,bool>> predicate)
        {
            return _set.AsNoTracking().Where(predicate);
        }
    }
}
