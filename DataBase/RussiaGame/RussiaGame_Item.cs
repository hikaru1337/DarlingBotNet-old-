using System.ComponentModel.DataAnnotations;

namespace DarlingBotNet.DataBase.RussiaGame
{
    public class RussiaGame_Item
    {
        [Key] public ulong itemid { get; set; }

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
                return startprice;
            }
        }

        public ulong NowPrestije
        {
            get
            {
                if (countTrade != 0) return startprestije / countTrade;
                return startprestije;
            }
        }
    }
}