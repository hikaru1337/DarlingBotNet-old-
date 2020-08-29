using DarlingBotNet.DataBase;

using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DarlingBotNet.Services
{
    public class WarnSystem
    {
        
        public async Task<(Users, EmbedBuilder)> WarnUser(SocketGuildUser user)
        {
            
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder();
                var gldswrn = DBcontext.Warns.AsQueryable().Where(x=>x.guildid == user.Guild.Id);
                var usr = DBcontext.Users.GetOrCreate(user).Result;
                if (usr.countwarns >= 15 || usr.countwarns >= gldswrn.Count())
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
            
            using (var DBcontext = new DBcontext())
            {
                var emb = new EmbedBuilder();
                var usr = DBcontext.Users.GetOrCreate((user as SocketGuildUser)).Result;
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
