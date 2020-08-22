using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DarlingBotNet.DataBase
{
    public class TempUser
    {
        public ulong Id { get; set; }
        public ulong guildid { get; set; }
        public ulong userId { get; set; }
        public DateTime ToTime { get; set; }
        public string Reason { get; set; }
        //public string typemute { get; set; }
    }
}
