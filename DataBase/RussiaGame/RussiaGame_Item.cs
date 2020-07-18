using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DarlingBotNet.DataBase.RussiaGame
{
    public class RussiaGame_Item
    {
        [Key]
        public ulong itemid { get; set; }
        public string ItemName { get; set; }
        public ulong startprice { get; set; }
        public ulong startprestije { get; set; }
        public ulong countTrade { get; set; }
        public ulong userid { get; set; }
        public bool traded { get; set; }
        public ulong NowPrice
        {
            get
            {
                if (countTrade != 0) return startprice / countTrade;
                else return startprice;
            }
        }
        public ulong NowPrestije
        {
            get
            {
                if (countTrade != 0) return startprestije / countTrade;
                else return startprestije;
            }
        }
    }
}
