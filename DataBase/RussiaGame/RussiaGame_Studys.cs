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
        public Proffesion Proffesion_Study { get; set; }

        public enum Proffesion
        {
            none,
            hacker,
            information_security,
            police,
        }

    }
}
