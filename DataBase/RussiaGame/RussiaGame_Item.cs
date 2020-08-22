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
        public ulong guildid { get; set; }
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
        public ItemType ItemTypes { get; set; }
        //public DateTime DateCreate { get; set; }

        public enum ItemType
        {
            none,
            blackshop,
            food,
            technology,
            domian
        }

        public enum ItemProperties
        {
            none,
            hacking,
            antihacking,
            Drugs,
        }
    }
}
