using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarlingBotNet.DataBase.Database
{
    static class CacheMethods
    {
        public static void Update(this IMemoryCache cache, Users entity)
        {

            var us = (Users)cache.Get((entity.userid, entity.guildId));
            if (us != null)
            {
                if (entity != us)
                {
                    cache.Remove((entity.userid, entity.guildId));
                    cache.Set((entity.userid, entity.guildId), entity, new MemoryCacheEntryOptions()
                    { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) });

                }
            }

        }

        public static void Update(this IMemoryCache cache, Channels entity)
        {

            var us = (Channels)cache.Get((entity.channelid, entity.guildid));
            if (us != null)
            {
                if (entity != us)
                {
                    cache.Remove((entity.channelid, entity.guildid));
                    cache.Set((entity.channelid, entity.guildid), entity, new MemoryCacheEntryOptions()
                    { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) });

                }
            }

        }

        public static void Update(this IMemoryCache cache, Guilds entity)
        {
            var us = (Guilds)cache.Get(entity.guildid);
            if (us != null)
            {
                if (entity != us)
                {
                    cache.Remove(entity.guildid);
                    cache.Set(entity.guildid, entity, new MemoryCacheEntryOptions()
                    { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) });

                }
            }
        }

        public static Guilds GetOrCreateGuldsCache(this IMemoryCache cache, ulong guildId)
        {
            using (var DBcontext = new DBcontext())
            {
                var x = (Guilds)cache.Get(guildId);
                if (x == null)
                {
                    x = DBcontext.Guilds.FirstOrDefault(x => x.guildid == guildId);
                    cache.Set(guildId, x, new MemoryCacheEntryOptions()
                    { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) });
                }
                return x;
            }
        }

        public static Channels GetOrCreateChannelCache(this IMemoryCache cache, ulong channelId, ulong guildId)
        {
            using (var DBcontext = new DBcontext())
            {
                var x = (Channels)cache.Get((channelId, guildId));
                if (x == null)
                {
                    x = DBcontext.Channels.FirstOrDefault(x => x.channelid == channelId && x.guildid == guildId);
                    cache.Set((channelId, guildId), x, new MemoryCacheEntryOptions()
                    { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) });
                }
                return x;
            }
        }

        public static Users GetOrCreateUserCache(this IMemoryCache cache, ulong userId, ulong guildId)
        {
            using (var DBcontext = new DBcontext())
            {
                var x = (Users)cache.Get((userId, guildId));
                if (x == null)
                {
                    x = DBcontext.Users.GetOrCreate(userId, guildId).Result;
                    cache.Set((userId, guildId), x, new MemoryCacheEntryOptions()
                    { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) });
                }
                return x;
            }
        }
    }
}
