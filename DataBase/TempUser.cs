using System;
using System.ComponentModel.DataAnnotations;

namespace DarlingBotNet.DataBase
{
    public class TempUser
    {
        [Key] public ulong Id { get; set; }

        public ulong guildId { get; set; }
        public ulong userId { get; set; }
        public DateTime ToTime { get; set; }

        public string Reason { get; set; }
        //public string typemute { get; set; }
    }
}