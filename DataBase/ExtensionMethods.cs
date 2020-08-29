﻿using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DarlingBotNet.DataBase
{
    public static class ExtensionMethods
    {

        public static async Task<Users> GetOrCreate(this DbSet<Users> Us, ulong userId,ulong guildId)
        {
            using (var DBcontext = new DBcontext())
            {
                var gg = DBcontext.Users.FirstOrDefault(u => u.userid == userId && u.guildId == guildId);
                if (gg == null)
                {
                    gg = new Users() { userid = userId, guildId = guildId, ZeroCoin = 1000, clanInfo = Users.UserClanRole.ready };
                    DBcontext.Add(gg);
                    await DBcontext.SaveChangesAsync();
                }
                return gg;
            }
        }

        public static async Task<Users> GetOrCreate(this DbSet<Users> Us, SocketGuildUser user)
        {
            using (var DBcontext = new DBcontext())
            {
                var gg = DBcontext.Users.FirstOrDefault(u => u.userid == user.Id && u.guildId == user.Guild.Id);
                if (gg == null)
                {
                    gg = new Users() { userid = user.Id, guildId = user.Guild.Id, ZeroCoin = 1000, clanInfo = Users.UserClanRole.ready };
                    DBcontext.Add(gg);
                    await DBcontext.SaveChangesAsync();
                }
                return gg;
            }
        }
    }
}
