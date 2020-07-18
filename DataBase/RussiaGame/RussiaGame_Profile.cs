using DarlingBotNet.DataBase.RussiaGame;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DarlingBotNet.DataBase
{
    public class RussiaGame_Profile
    {
        [Key]
        public ulong userid { get; set; }
        public long money { get; set; }
        public ulong Prestije
        {
            get
            {
                var items = new EEF<RussiaGame_Item>(new DBcontext()).Get(x => x.userid == userid);
                return (ulong)items.Sum(p => (float)p.NowPrestije);
            }
        }
        public ulong laststudyid { get; set; }
        public uint DaysStudy { get; set; }
        public ulong workid { get; set; }
        public ulong workStreak { get; set; }
    }
}
