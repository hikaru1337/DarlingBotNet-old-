using System.ComponentModel.DataAnnotations;

namespace DarlingBotNet.DataBase.RussiaGame
{
    public class RussiaGame_Programmer
    {
        [Key]
        public ulong Id { get; set; }
        public ulong RG_ProfileID { get; set; }
        public Proffesion Proffesion_user { get; set; }
        public ulong count_hacking { get; set; }
        public bool IsActive { get; set; }

        public enum Proffesion
        {
            hacker,
            information_security
        }
    }
}
