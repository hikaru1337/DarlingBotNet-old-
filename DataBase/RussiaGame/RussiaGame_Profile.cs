using DarlingBotNet.DataBase.RussiaGame;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace DarlingBotNet.DataBase
{
    public class RussiaGame_Profile
    {
        [Key]
        public ulong id { get; set; }
        public ulong userid { get; set; }
        public ulong guildid { get; set; }
        public long money { get; set; }
        public ulong Prestije
        {
            get
            {
                var items = new DBcontext().RG_Item.AsNoTracking().Where(x => x.userid == userid && x.guildid == guildid);
                if(items.Count() > 0)
                    return (ulong)items.Sum(p => (float)p.NowPrestije);
                return 0;
            }
        }
        public ulong StudyNowid { get; set; }
        public ushort DaysStudy { get; set; }
        public DateTime LastStudy { get; set; }
        public ulong workid { get; set; }
        public ulong workStreak { get; set; }
        public DateTime LastWork { get; set; }

        [NotMapped]
        public IQueryable<RussiaGame_Item> UserItems
        {
            get
            {
                return new DBcontext().RG_Item.AsNoTracking().Where(x => x.userid == userid && x.guildid == guildid && !x.traded);
            }
        }

        [NotMapped]
        public IQueryable<RussiaGame_Item> UserItemsTraded
        {
            get
            {
                return new DBcontext().RG_Item.AsNoTracking().Where(x => x.userid == userid && x.guildid == guildid && x.traded);
            }
        }

        [NotMapped]
        public IQueryable<RussiaGame_Studys> Studys
        {
            get
            {
                return new DBcontext().RG_Studys.AsNoTracking().Where(x => x.userid == userid && x.guildid == guildid);
            }
        }
    }
}
