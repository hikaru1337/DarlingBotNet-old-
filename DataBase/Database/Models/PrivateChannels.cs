using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DarlingBotNet.DataBase
{
    public class PrivateChannels : DbEntity
    {
        
        public ulong guildid { get; set; }
        public ulong userid { get; set; }
        public ulong channelid { get; set; }
    }
}
