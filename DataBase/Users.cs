using System;
using System.ComponentModel.DataAnnotations;

namespace DarlingBotNet.DataBase
{
    public class Users
    {
        [Key] public ulong Id { get; set; }

        public ulong userid { get; set; }
        public bool Leaved { get; set; }
        public ulong guildId { get; set; }
        public ulong XP { get; set; }

        public ulong Level => (ulong) Math.Sqrt(XP / 80);

        public ulong ZeroCoin { get; set; }
        public DateTime Daily { get; set; }
        public ulong Streak { get; set; }
        public uint countwarns { get; set; }
        public ulong marryedid { get; set; }
    }
}