using System.ComponentModel.DataAnnotations;

namespace DarlingBotNet.DataBase
{
    public class Warns
    {
        [Key] public ulong Id { get; set; }

        public ulong guildId { get; set; }
        public ulong CountWarn { get; set; }
        public string ReportWarn { get; set; }
    }
}