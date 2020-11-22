using System;
using System.Collections.Generic;
using System.Text;

namespace DarlingBotNet.DataBase.Database.Crypto
{
    public class Crypto_Work
    {
        public ulong WorkId { get; set; }
        public string WorkName { get; set; }
        public ulong StudyId { get; set; }
        public ulong Money { get; set; }
        public bool Invise { get; set; }
    }
}
