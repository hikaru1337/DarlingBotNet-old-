using DarlingBotNet.DataBase;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DarlingBotNet.Services.Sys
{
    public class Privates
    {

        public async Task CheckPrivate(SocketGuild _discord)
        {
            using (var DBcontext = new DBcontext())
            {
                var prv = DBcontext.PrivateChannels.AsQueryable().Where(x => x.guildid == _discord.Id);
                foreach (var PC in prv) // Проверка Приваток
                {
                    SocketVoiceChannel chnl = _discord.GetVoiceChannel(PC.channelid);
                    await Privatemethod(chnl, PC);
                }
            }

        } // ПРОВЕРКА ПРИВАТНЫХ КАНАЛОВ
        public async Task PrivateCreate(SocketGuildUser user)
        {
            using (var DBcontext = new DBcontext())
            {
                Guilds glds = DBcontext.Guilds.FirstOrDefault(x => x.guildid == user.Guild.Id);
                SocketVoiceChannel chnl = user.Guild.GetVoiceChannel(glds.PrivateChannelID);
                await chnl.AddPermissionOverwriteAsync(user, permissions: new OverwritePermissions(connect: PermValue.Deny));
                if (chnl.Category == null)
                {
                    if (user.Guild.CategoryChannels.FirstOrDefault(x => x.Name == "DARLING PRIVATE") == null)
                    {
                        RestCategoryChannel cat = await user.Guild.CreateCategoryChannelAsync("DARLING PRIVATE");
                        await chnl.ModifyAsync(x => x.CategoryId = cat.Id);
                    }
                    else await chnl.ModifyAsync(x => x.CategoryId = user.Guild.CategoryChannels.First(x => x.Name == "DARLING PRIVATE").Id);
                }

                if(chnl.Category.PermissionOverwrites.Where(x=>x.TargetId == user.Guild.EveryoneRole.Id && x.Permissions.Connect == PermValue.Deny).Count() == 0)
                    await chnl.Category.AddPermissionOverwriteAsync(user.Guild.EveryoneRole, permissions: new OverwritePermissions(connect: PermValue.Deny));

                var voiceChannel = await user.Guild.CreateVoiceChannelAsync($"{user}` VOICE", x => x.CategoryId = chnl.CategoryId);
                await voiceChannel.AddPermissionOverwriteAsync(user, permissions: new OverwritePermissions(connect: PermValue.Allow, muteMembers: PermValue.Allow, deafenMembers: PermValue.Allow, moveMembers: PermValue.Allow, manageChannel: PermValue.Allow));
                var prv = new PrivateChannels();
                try
                {
                    await user.ModifyAsync(x => x.Channel = voiceChannel);
                    prv = DBcontext.PrivateChannels.Add(new PrivateChannels() { userid = user.Id, channelid = voiceChannel.Id, guildid = user.Guild.Id }).Entity;
                }
                catch (Exception)
                {
                    //var prv =  DBcontext.PrivateChannels.FirstOrDefault(x=>x.userid == user.Id && x.guildid == user.Guild.Id && x.channelid == voiceChannel.Id);
                    if (prv != null) DBcontext.PrivateChannels.Remove(prv);
                    await voiceChannel.DeleteAsync();
                }
                await Task.Delay(1000);
                await chnl.RemovePermissionOverwriteAsync(user);
                await DBcontext.SaveChangesAsync();
            }
        } // Создание приваток

        private async Task Privatemethod(SocketVoiceChannel chnl, PrivateChannels PC)
        {
            using (var DBcontext = new DBcontext())
            {
                if (chnl == null || chnl.Users.Count == 0)
                {
                    if (chnl != null) 
                        await chnl.DeleteAsync();
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
                await DBcontext.SaveChangesAsync();
            }
        }


        public async Task PrivateDelete(SocketGuildUser user, SocketVoiceState ot)
        {
            using (var DBcontext = new DBcontext())
            {
                PrivateChannels prv = DBcontext.PrivateChannels.FirstOrDefault(x => x.userid == user.Id && x.guildid == user.Guild.Id && x.channelid == ot.VoiceChannel.Id);
                if (prv != null)
                {
                    SocketVoiceChannel chnl = user.Guild.GetVoiceChannel(prv.channelid);
                    await Privatemethod(chnl, prv);
                    await CheckPrivate(user.Guild);
                }
            }
        } // Удаление приваток
    }
}
