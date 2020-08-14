﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DarlingBotNet.DataBase
{
    public class TempUser : DbEntity
    {
        
        public ulong guildId { get; set; }
        public ulong userId { get; set; }
        public DateTime ToTime { get; set; }
        public string Reason { get; set; }
        //public string typemute { get; set; }
    }
}