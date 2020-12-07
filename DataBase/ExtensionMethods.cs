using DarlingBotNet.Services;
using Discord.WebSocket;
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
                var gg = DBcontext.Users.FirstOrDefault(u => u.UserId == userId && u.GuildId == guildId);
                if (gg == null)
                {
                    var Time = DateTime.Now.AddDays(-1);
                    gg = new Users() { UserId = userId, GuildId = guildId, ZeroCoin = 1000,Daily = Time, BankLastTransit = Time,BankTimer = Time};
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
                var gg = DBcontext.Users.FirstOrDefault(u => u.UserId == user.Id && u.GuildId == user.Guild.Id);
                if (gg == null)
                {
                    var Time = DateTime.Now.AddDays(-1);
                    gg = new Users() { UserId = user.Id, GuildId = user.Guild.Id, ZeroCoin = 1000, Daily = Time, BankLastTransit = Time, BankTimer = Time };
                    DBcontext.Add(gg);
                    await DBcontext.SaveChangesAsync();
                }
                return gg;
            }
        }

        public static async Task<Guilds> GetOrCreate(this DbSet<Guilds> Us, SocketGuild Guild)
        {
            using (var DBcontext = new DBcontext())
            {
                var gg = DBcontext.Guilds.FirstOrDefault(u => u.GuildId == Guild.Id);
                if (gg == null)
                {
                    gg = new Guilds() { GuildId = Guild.Id, GiveXPnextChannel = true, Prefix = BotSettings.Prefix, OwnerId = Guild.OwnerId };
                    DBcontext.Add(gg);
                    await DBcontext.SaveChangesAsync();
                }
                return gg;
            }
        }

        public static async Task<Channels> GetOrCreate(this DbSet<Channels> Us, SocketTextChannel TextChannel,bool GuildGiveXPnextChannel)
        {
            using (var DBcontext = new DBcontext())
            {
                var gg = DBcontext.Channels.FirstOrDefault(u => u.GuildId == TextChannel.Guild.Id && u.ChannelId == TextChannel.Id);
                if (gg == null)
                {
                    gg = new Channels() { GuildId = TextChannel.Guild.Id, ChannelId = TextChannel.Id, GiveXP = GuildGiveXPnextChannel, UseCommand = true };
                    DBcontext.Add(gg);
                    await DBcontext.SaveChangesAsync();
                }
                return gg;
            }
        }
    }
}
