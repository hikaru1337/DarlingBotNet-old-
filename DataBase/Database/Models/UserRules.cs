﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DarlingBotNet.DataBase
{
    public class UserRules
    {
        
        public ulong guildId { get; set; }
        public ulong userId { get; set; }
        public ulong RulesNumber { get; set; }
        public ulong RulesPoint { get; set; }
    }
}