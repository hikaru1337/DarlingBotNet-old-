using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DarlingBotNet.DataBase.RussiaGame
{
    class RussiaGame_Studys
    {
        [Key]
        public ulong id { get; set; }
        public ulong studyid { get; set; }
        public ulong userid { get; set; }
    }
}
