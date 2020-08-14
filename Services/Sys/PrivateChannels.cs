using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DarlingBotNet.Services.Sys
{
    public class Privates
    {
        private readonly DbService _db;

        public Privates(DbService db)
        {
            _db = db;
        }
            public async Task CheckPrivate(ulong glds, SocketGuild _discord)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                var prv = DBcontext.PrivateChannels.Get(glds);
                foreach (var PC in prv) // Проверка Приваток
                {
                    SocketVoiceChannel chnl = _discord.GetVoiceChannel(PC.channelid);
                    await Privatemethod(chnl, PC);
                }
            }

        } // ПРОВЕРКА ПРИВАТНЫХ КАНАЛОВ
        public async Task PrivateCreate(SocketGuildUser user)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                Guilds glds = DBcontext.Guilds.Get(user.Guild.Id);
                SocketVoiceChannel chnl = user.Guild.GetVoiceChannel(glds.PrivateChannelID);
                await chnl.AddPermissionOverwriteAsync(user, permissions: new OverwritePermissions(connect: PermValue.Deny));
                if (chnl.Category == null)
                {
                    if (user.Guild.CategoryChannels.Where(x => x.Name == "DARLING PRIVATE") == null)
                    {
                        RestCategoryChannel cat = await user.Guild.CreateCategoryChannelAsync("DARLING PRIVATE");
                        await chnl.ModifyAsync(x => x.CategoryId = cat.Id);
                    }
                    else await chnl.ModifyAsync(x => x.CategoryId = user.Guild.CategoryChannels.Where(x => x.Name == "DARLING PRIVATE").First().Id);
                }
                var voiceChannel = await user.Guild.CreateVoiceChannelAsync($"{user}` VOICE", x => x.CategoryId = chnl.CategoryId);
                await voiceChannel.AddPermissionOverwriteAsync(user.Guild.EveryoneRole, permissions: new OverwritePermissions(connect: PermValue.Deny));
                await voiceChannel.AddPermissionOverwriteAsync(user, permissions: new OverwritePermissions(connect: PermValue.Allow, muteMembers: PermValue.Allow, deafenMembers: PermValue.Allow, moveMembers: PermValue.Allow, manageChannel: PermValue.Allow));
                try
                {
                    await user.ModifyAsync(x => x.Channel = voiceChannel);
                    DBcontext.PrivateChannels.GetOrCreate(user.Id, voiceChannel);
                }
                catch (Exception)
                {
                    var prv = DBcontext.PrivateChannels.Get(user.Id,voiceChannel);
                    if (prv != null) DBcontext.PrivateChannels.Remove(prv);
                    await voiceChannel.DeleteAsync();
                }
                await Task.Delay(2000);
                await chnl.RemovePermissionOverwriteAsync(user);
                await DBcontext.SaveChangesAsync();
            }
        } // Создание приваток

        private async Task Privatemethod(SocketVoiceChannel chnl,PrivateChannels PC)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                if (chnl == null || chnl.Users.Count == 0)
                {
                    if (chnl != null) await chnl.DeleteAsync();
                    DBcontext.PrivateChannels.Remove(PC);
                }
                else if (chnl.Users.Count > 0 && chnl.Users.Where(x => x.Id == PC.userid) == null)
                {
                    var newusr = chnl.Users.First();
                    PC.userid = newusr.Id;
                    DBcontext.PrivateChannels.Update(PC);
                    if (chnl.Name.Contains($"{chnl.GetUser(PC.userid)}"))
                        await chnl.ModifyAsync(x => x.Name = chnl.Name.Replace($"{chnl.GetUser(PC.userid)}", $"{newusr}"));

                    await chnl.AddPermissionOverwriteAsync(newusr, permissions: new OverwritePermissions(connect: PermValue.Allow, muteMembers: PermValue.Allow, deafenMembers: PermValue.Allow, moveMembers: PermValue.Allow, manageChannel: PermValue.Allow));
                }
            }
        }


        public async Task PrivateDelete(SocketGuildUser user, SocketVoiceState ot)
        {
            using (var DBcontext = _db.GetDbContext())
            {
                PrivateChannels prv = DBcontext.PrivateChannels.Get(user.Id, ot.VoiceChannel);
                if (prv != null)
                {
                    SocketVoiceChannel chnl = user.Guild.GetVoiceChannel(prv.channelid);
                    await Privatemethod(chnl, prv);
                }
            }
        } // Удаление приваток
    }
}
