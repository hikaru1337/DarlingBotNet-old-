using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using DarlingBotNet.Modules;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DarlingBotNet.DataBase
{
    public class PrivateChannelsRepository : Repository<PrivateChannels>, IPrivateChannelsRepository
    {
        public PrivateChannelsRepository(DbContext context) : base(context)
        {
        }

        public void EnsureCreated(ulong userId, ulong guildId)
        {
            _context.Database.ExecuteSqlRaw($@"INSERT OR IGNORE INTO Users (userid, guildId, ZeroCoin) VALUES ({userId}, {guildId},1000);");
        }
        
        public PrivateChannels GetOrCreate(ulong UserId, ulong ChannelId, ulong GuildId)
        {
            //EnsureCreated(userId, guildId);
            var warn = _set.FirstOrDefault(x => x.userid == UserId && x.channelid == ChannelId && x.guildid == GuildId);
            if (warn == null)
            {
                warn = new PrivateChannels() { userid = UserId, channelid = ChannelId, guildid = GuildId };
                _set.Add(warn);
            }
            return warn;
        }

        public PrivateChannels GetOrCreate(ulong UserId, RestVoiceChannel Channel)
        {
            var warn = _set.FirstOrDefault(x => x.userid == UserId && x.channelid == Channel.Id && x.guildid == Channel.GuildId);
            if (warn == null)
            {
                warn = new PrivateChannels() { userid = UserId, channelid = Channel.Id, guildid = Channel.GuildId };
                _set.Add(warn);
            }
            return warn;
        }

        public PrivateChannels Get(ulong UserId, RestVoiceChannel Channel)
        {
            //EnsureCreated(userId, guildId);
            return _set.FirstOrDefault(u => u.userid == UserId && u.guildid == Channel.GuildId && u.channelid == Channel.Id);
        }

        public PrivateChannels Get(ulong UserId, SocketVoiceChannel Channel)
        {
            //EnsureCreated(userId, guildId);
            var warn = new PrivateChannels() { userid = UserId, channelid = Channel.Id, guildid = Channel.Guild.Id };
            _set.Add(warn);
            return warn;
        }

        public IEnumerable<PrivateChannels> Get(ulong guildId)
        {
            return _set.AsNoTracking().Where(u => u.guildid == guildId);
        }

        public PrivateChannels Get(ulong guildId, ulong ChannelId)
        {
            return _set.FirstOrDefault(u => u.guildid == guildId && u.channelid == ChannelId);
        }

        public void RemoveRange(ulong GuildId)
        {
            var lists = _set.AsNoTracking().Where(x => x.guildid == GuildId);
            _set.RemoveRange(lists);
        }
    }
}
