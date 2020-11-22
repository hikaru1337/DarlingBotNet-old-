using System;
using static DarlingBotNet.DataBase.Warns;

namespace DarlingBotNet.DataBase
{
    public class TempUser
    {
        public ulong Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public DateTime ToTime { get; set; }
        public ReportType Reason { get; set; }
        //public string typemute { get; set; }
    }
}
