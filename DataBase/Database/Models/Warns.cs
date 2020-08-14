using System.ComponentModel.DataAnnotations;

namespace DarlingBotNet.DataBase
{
    public class Warns : DbEntity
    {
        
        public ulong guildId { get; set; }
        public ulong CountWarn { get; set; }
        public string ReportWarn { get; set; }
    }
}
