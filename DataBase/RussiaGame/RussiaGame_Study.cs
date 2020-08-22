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
        public ushort DayStudying { get; set; }
        public bool Invise { get; set; }
        public Proffesion Proffesion_Study {get;set;}

        public enum Proffesion
        {
            none,
            hacker,
            information_security,
            police,
        }
    }
}
