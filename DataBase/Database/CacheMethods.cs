using Discord.Commands;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace DarlingBotNet.DataBase.Database
{

    static class CacheMethods
    {

        //public static bool HashChecking(Object one, Object two)
        //{

        //    byte[] tmpSource = ASCIIEncoding.ASCII.GetBytes(one.ToString());
        //    byte[] tmpHash = ASCIIEncoding.ASCII.GetBytes(two.ToString());

        //    byte[] tmpNewHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
        //    byte[] tmpNewSource = new MD5CryptoServiceProvider().ComputeHash(tmpSource);

        //    bool bEqual = false;
        //    if (tmpNewHash.Length == tmpNewSource.Length)
        //    {
        //        int i = 0;
        //        while ((i < tmpNewHash.Length) && (tmpNewHash[i] == tmpNewSource[i]))
        //            i += 1;

        //        if (i == tmpNewHash.Length)
        //            bEqual = true;

        //    }
        //    return bEqual;
        //}

        public static void Update(this IMemoryCache cache, Users entity)
        {
            var us = (Users)cache.Get((entity.UserId, entity.GuildId));
            if (us != null)
            {
                if (!Object.Equals(entity,us))
                {
                    cache.Remove((entity.UserId, entity.GuildId));
                    //cache.Set((entity.userid, entity.guildId), entity, new MemoryCacheEntryOptions()
                    //{ AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) });
                }
            }
        }

        public static void Update(this IMemoryCache cache, Channels entity)
        {
            var us = (Channels)cache.Get((entity.ChannelId, entity.GuildId));
            if (us != null)
            {
                if (!Object.Equals(entity, us))
                {
                    cache.Remove((entity.ChannelId, entity.GuildId));
                    //cache.Set((entity.channelid, entity.guildid), entity, new MemoryCacheEntryOptions()
                    //{ AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) });
                }
            }

        }

        public static void Update(this IMemoryCache cache, Guilds entity)
        {
            var us = (Guilds)cache.Get(entity.GuildId);
            if (us != null)
            {
                if(!Object.Equals(entity, us))
                {
                    cache.Remove(entity.GuildId);
                    //cache.Set(entity.guildid, entity, new MemoryCacheEntryOptions()
                    //{ AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) });
                }
            }
        }


        public static Guilds GetOrCreateGuldsCache(this IMemoryCache cache, ulong guildId)
        {
            using (var DBcontext = new DBcontext())
            {
                var x = cache.Get(guildId) as Guilds;
                if (x == null)
                {
                    x = DBcontext.Guilds.FirstOrDefault(x => x.GuildId == guildId);
                    cache.Set(guildId, x, new MemoryCacheEntryOptions()
                    { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10) });
                }
                return x;
            }
        }

        public static Channels GetOrCreateChannelCache(this IMemoryCache cache, ulong channelId, ulong guildId)
        {
            using (var DBcontext = new DBcontext())
            {
                var x = cache.Get((channelId, guildId)) as Channels;
                if (x == null)
                {
                    x = DBcontext.Channels.FirstOrDefault(x => x.ChannelId == channelId && x.GuildId == guildId);
                    cache.Set((channelId, guildId), x, new MemoryCacheEntryOptions()
                    { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10) });
                }
                return x;
            }
        }







        public static Users GetOrCreateUserCache(this IMemoryCache cache, ulong userId, ulong guildId)
        {
            using (var DBcontext = new DBcontext())
            {
                var x = cache.Get((userId, guildId)) as Users;
                if (x == null)
                {
                    x = DBcontext.Users.GetOrCreate(userId, guildId).Result;
                    cache.Set((userId, guildId), x, new MemoryCacheEntryOptions()
                    { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10) });
                }
                return x;
            }
        }



        public static void Removes(this IMemoryCache entity, ICommandContext context)
        {
            var UsersCache = (Users)entity.Get((context.User.Id, context.Guild.Id));
            if (UsersCache != null)
            {
                entity.Remove((context.User.Id, context.Guild.Id));
            }
                

            var ChannelCache = (Channels)entity.Get((context.Channel.Id, context.Guild.Id));
            if (ChannelCache != null)
                entity.Remove((context.Channel.Id, context.Guild.Id));

            var GuildCache = (Guilds)entity.Get(context.Guild.Id);
            if (GuildCache != null)
                entity.Remove(context.Guild.Id);
        }


        public static bool UpdateCache(object oldCar, object newCar)
        {
            Type type = oldCar.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                object oldCarValue = property.GetValue(oldCar, null);
                object newCarValue = property.GetValue(newCar, null);
                if (oldCarValue.ToString() != newCarValue.ToString())
                    return true;
            }
            return false;
        }
    }
}
