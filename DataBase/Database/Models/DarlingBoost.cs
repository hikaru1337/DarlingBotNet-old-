using System;
using System.Collections.Generic;
using System.Text;

namespace DarlingBotNet.Modules
{
    public class DarlingBoost
    {
        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        public ulong Streak { get; set; }
        public DateTime Ends { get; set; }
    }
}
