using DarlingBotNet.DataBase;
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
        public static async Task<(Users, EmbedBuilder)> WarnUser(SocketGuildUser user)
        {
            var emb = new EmbedBuilder();
            await Task.Delay(1);
            var usr = SystemLoading.CreateUser(user).Result;
            if (usr.countwarns >= 15)
                usr.countwarns = 1;
            else
                usr.countwarns++;
            emb.WithDescription($"Пользователь {user.Mention} получил {usr.countwarns} нарушение");
            new EEF<Users>(new DBcontext()).Update(usr);
            return (usr,emb);
        }

        public static async Task<(Users,EmbedBuilder)> UnWarnUser(SocketGuildUser user)
        {
            var emb = new EmbedBuilder();
            await Task.Delay(1);
            var usr = SystemLoading.CreateUser(user).Result;
            if (usr.countwarns != 0)
            {
                emb.WithDescription($"У пользователя {user.Mention} снято {usr.countwarns} нарушение.");
                usr.countwarns--;
            }
            else emb.WithDescription($"У пользователя {user.Mention} нету нарушений.");
            new EEF<Users>(new DBcontext()).Update(usr);
            return (usr,emb);
        }
    }
}
