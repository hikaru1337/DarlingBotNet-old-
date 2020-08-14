using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DarlingBotNet.Services
{
    public class WarnSystem
    {
        private readonly DbService _db;

        public WarnSystem(DbService db)
        {
            _db = db;
        }
        public async Task<(Users, EmbedBuilder)> WarnUser(SocketGuildUser user)
        {
            
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder();
                await Task.Delay(1);
                var usr = DBcontext.Users.GetOrCreate(user);
                if (usr.countwarns >= 15)
                    usr.countwarns = 1;
                else
                    usr.countwarns++;
                emb.WithDescription($"Пользователь {user.Mention} получил {usr.countwarns} нарушение");
                DBcontext.Users.Update(usr);
                await DBcontext.SaveChangesAsync();
                return (usr, emb);
            }
        }

        public async Task<(Users,EmbedBuilder)> UnWarnUser(SocketGuildUser user)
        {
            
            using (var DBcontext = _db.GetDbContext())
            {
                var emb = new EmbedBuilder();
                await Task.Delay(1);
                var usr = DBcontext.Users.GetOrCreate(user);
                if (usr.countwarns != 0)
                {
                    emb.WithDescription($"У пользователя {user.Mention} снято {usr.countwarns} нарушение.");
                    usr.countwarns--;
                }
                else emb.WithDescription($"У пользователя {user.Mention} нету нарушений.");
                DBcontext.Users.Update(usr);
                await DBcontext.SaveChangesAsync();
                return (usr, emb);
            }
        }
    }
}
