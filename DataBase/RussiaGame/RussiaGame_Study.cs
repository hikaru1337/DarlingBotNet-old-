using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DarlingBotNet.DataBase.RussiaGame
{
    public class RussiaGame_Study
    {
        [Key]
        public ulong studyid { get; set; }
        public string studyName { get; set; }
        public ulong StudyMoney { get; set; }
        public ulong DayStudying { get; set; }
    }
}
