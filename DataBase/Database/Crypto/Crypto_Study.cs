using System;
using System.Collections.Generic;
using System.Text;

namespace DarlingBotNet.DataBase.Database.Crypto
{
    public class Crypto_Study
    {
        public ulong StudyId { get; set; }
        public string StudyName { get; set; }
        public ulong StudyMoney { get; set; }
        public ushort DayStudying { get; set; }
        public bool Invise { get; set; }
    }
}
