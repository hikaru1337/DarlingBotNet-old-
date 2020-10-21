using System.ComponentModel.DataAnnotations;

namespace DarlingBotNet.DataBase.RussiaGame
{
    public class RussiaGame_Studys
    {
        [Key]
        public ulong id { get; set; }
        public ulong studyid { get; set; }
        public ulong userid { get; set; }
        public ulong guildid { get; set; }

    }
}
