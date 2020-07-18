using System.ComponentModel.DataAnnotations;

namespace DarlingBotNet.DataBase.RussiaGame
{
    public class RussiaGame_Work
    {
        [Key] public ulong workid { get; set; }

        public string workName { get; set; }
        public ulong studyid { get; set; }
        public ulong money { get; set; }
    }
}