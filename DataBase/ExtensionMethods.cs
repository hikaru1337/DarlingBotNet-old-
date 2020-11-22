using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
                    gg = new Users() { UserId = userId, GuildId = guildId, ZeroCoin = 1000};
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
                    gg = new Users() { UserId = user.Id, GuildId = user.Guild.Id, ZeroCoin = 1000 };
                    DBcontext.Add(gg);
                    await DBcontext.SaveChangesAsync();
                }
                return gg;
            }
        }
    }
}
