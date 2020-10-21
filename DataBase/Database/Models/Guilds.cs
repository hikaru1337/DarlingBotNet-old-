using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DarlingBotNet.DataBase
{
    public class Guilds
    {
        
        public ulong Id { get; set; }

        public ulong guildid { get; set; }
        public bool Leaved { get; set; }
        public string Prefix { get; set; }
        public ulong chatmuterole { get; set; }
        public ulong voicemuterole { get; set; }
        public ulong PrivateChannelID { get; set; }
        [NotMapped]
        public List<string> CommandInviseList
        {
            get
            {
                if (CommandInviseString != null)
                {
                    var comm = CommandInviseString.Split(',').ToList();
                    if (comm == null || comm.Count == 0 || comm.First() == "") return new List<string>();
                    return comm;
                }
                return new List<string>();
            }
            set
            {
                CommandInviseString = string.Join(",", value.ToArray());
            }
        }
        public string CommandInviseString { get; set; }

        public bool GiveXPnextChannel { get; set; }

        public byte ViolationSystem { get; set; }

        public ulong banchannel { get; set; }
        public ulong unbanchannel { get; set; }
        public ulong kickchannel { get; set; }
        public ulong leftchannel { get; set; }
        public ulong joinchannel { get; set; }
        public ulong meseditchannel { get; set; }
        public ulong mesdelchannel { get; set; }
        public ulong voiceUserActions { get; set; }
        public bool inviteMessages { get; set; }

        public string WelcomeMessage { get; set; }
        public string WelcomeDMmessage { get; set; }
        public bool WelcomeDMuser { get; set; }
        public ulong WelcomeChannel { get; set; }
        public ulong WelcomeRole { get; set; }

        public string LeaveMessage { get; set; }
        public ulong LeaveChannel { get; set; }

        public bool RaidStop { get; set; }
        public uint RaidTime { get; set; }
        public uint RaidUserCount { get; set; }
        public DateTime RaidMuted { get; set; }

        public uint LimitRoleUserClan { get; set; }
        public ulong PriceBuyRole { get; set; }
        public bool GiveClanRoles { get; set; }

    }
}
