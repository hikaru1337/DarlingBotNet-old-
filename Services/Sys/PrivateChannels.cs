using DarlingBotNet.DataBase;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarlingBotNet.Services.Sys
{
    public class Privates
    {
        public static async Task CheckPrivate(ulong glds, SocketGuild _discord)
        {
            var prv = new EEF<PrivateChannels>(new DBcontext()).Get(x => x.guildid == glds);
            foreach (var PC in prv) // Проверка Приваток
            {
                SocketVoiceChannel chnl = _discord.GetVoiceChannel(PC.channelid);
                if (chnl == null) new EEF<PrivateChannels>(new DBcontext()).Remove(PC);
                else
                {
                    if (chnl.Users.Count == 0)
                    {
                        await chnl.DeleteAsync();
                        new EEF<PrivateChannels>(new DBcontext()).Remove(PC);
                    }
                    else if (chnl.Users.Where(x => x.Id == PC.userid) == null)
                    {
                        if (chnl.Name.Contains($"{chnl.GetUser(PC.userid)}"))
                            await chnl.ModifyAsync(x => x.Name = chnl.Name.Replace($"{chnl.GetUser(PC.userid)}", $"{chnl.Users.FirstOrDefault()}"));

                        await chnl.AddPermissionOverwriteAsync(chnl.Users.FirstOrDefault(), permissions: new OverwritePermissions(connect: PermValue.Allow, muteMembers: PermValue.Allow, deafenMembers: PermValue.Allow, moveMembers: PermValue.Allow, manageChannel: PermValue.Allow));
                        await chnl.RemovePermissionOverwriteAsync(_discord.GetUser(PC.userid) as IGuildUser);
                        PC.userid = chnl.Users.FirstOrDefault().Id;
                        new EEF<PrivateChannels>(new DBcontext()).Update(PC);
                    }
                }
            }

        } // ПРОВЕРКА ПРИВАТНЫХ КАНАЛОВ
        public static async Task PrivateCreate(SocketGuildUser user)
        {
            Guilds glds = new EEF<Guilds>(new DBcontext()).GetF(x => x.guildId == user.Guild.Id);
            SocketVoiceChannel chnl = user.Guild.GetVoiceChannel(glds.PrivateChannelID);
            await chnl.AddPermissionOverwriteAsync(user, permissions: new OverwritePermissions(connect: PermValue.Deny));
            if (chnl.Category == null)
            {
                var cat = await user.Guild.CreateCategoryChannelAsync("DARLING PRIVATE");
                await chnl.ModifyAsync(x => x.CategoryId = cat.Id);
            }
            var voiceChannel = await user.Guild.CreateVoiceChannelAsync($"{user}` VOICE", x => x.CategoryId = chnl.CategoryId);
            await voiceChannel.AddPermissionOverwriteAsync(user.Guild.EveryoneRole, permissions: new OverwritePermissions(connect: PermValue.Deny));
            await voiceChannel.AddPermissionOverwriteAsync(user, permissions: new OverwritePermissions(connect: PermValue.Allow, muteMembers: PermValue.Allow, deafenMembers: PermValue.Allow, moveMembers: PermValue.Allow, manageChannel: PermValue.Allow));
            try
            {
                await user.ModifyAsync(x => x.Channel = voiceChannel);
                new EEF<PrivateChannels>(new DBcontext()).Create(new PrivateChannels() { userid = user.Id, channelid = voiceChannel.Id, guildid = user.Guild.Id });
            }
            catch (Exception)
            {
                var prv = new EEF<PrivateChannels>(new DBcontext()).GetF(x => x.userid == user.Id && user.Guild.Id == x.guildid && voiceChannel.Id == x.channelid);
                if (prv != null) new EEF<PrivateChannels>(new DBcontext()).Remove(prv);
                await voiceChannel.DeleteAsync();
            }
            await chnl.RemovePermissionOverwriteAsync(user);
        } // Создание приваток
        public static async Task PrivateDelete(SocketGuildUser user, SocketVoiceState ot)
        {
            PrivateChannels prv = new EEF<PrivateChannels>(new DBcontext()).GetF(x => x.userid == user.Id && user.Guild.Id == x.guildid && ot.VoiceChannel.Id == x.channelid);
            if (prv != null)
            {
                SocketVoiceChannel chnl = user.Guild.GetVoiceChannel(prv.channelid);
                if (chnl != null)
                {
                    if (chnl.Users.Count == 0)
                    {
                        await chnl.DeleteAsync();
                        new EEF<PrivateChannels>(new DBcontext()).Remove(prv);
                    }
                    else
                    {
                        SocketGuildUser usr = user.Guild.GetVoiceChannel(prv.channelid).Users.FirstOrDefault();
                        if (chnl.Name.Contains($"{user}")) await chnl.ModifyAsync(x => x.Name = chnl.Name.Replace($"{user}", $"{usr}"));
                        await chnl.AddPermissionOverwriteAsync(usr, permissions: new OverwritePermissions(connect: PermValue.Allow, muteMembers: PermValue.Allow, deafenMembers: PermValue.Allow, moveMembers: PermValue.Allow, manageChannel: PermValue.Allow));
                        await chnl.RemovePermissionOverwriteAsync(user as IGuildUser);
                        prv.userid = usr.Id;
                        new EEF<PrivateChannels>(new DBcontext()).Update(prv);
                    }
                }
            }
        } // Удаление приваток
    }
}
