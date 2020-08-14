﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DarlingBotNet.DataBase
{
    public class Users : DbEntity
    {

        public ulong userid { get; set; }
        public bool Leaved { get; set; }
        public ulong guildId { get; set; }
        public ulong XP { get; set; }
        public ulong Level
        {
            get
            {
                return (ulong)Math.Sqrt(XP / 80);
            }
        }
        public ulong ZeroCoin { get; set; }
        public DateTime Daily { get; set; }
        public ulong Streak { get; set; }
        public uint countwarns { get; set; }
        public ulong marryedid { get; set; }
        public ulong clanId { get; set; }
        public string clanInfo { get; set; }
    }
}