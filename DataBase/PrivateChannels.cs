using System;
using System.Collections.Generic;
using System.Text;

namespace DarlingBotNet.DataBase
{
    public class PrivateChannels
    {
        public int Id { get; set; }
        public ulong guildid { get; set; }
        public ulong userid { get; set; }
        public ulong channelid { get; set; }
    }
}
