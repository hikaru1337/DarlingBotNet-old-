﻿using System.ComponentModel.DataAnnotations;

namespace DarlingBotNet.DataBase
{
    public class LVLROLES : DbEntity
    {
        
        public ulong guildid { get; set; }
        public ulong roleid { get; set; }
        public ulong countlvl { get; set; }
    }
}