using DarlingBotNet.DataBase.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DarlingBotNet.DataBase
{
    public class Clans : DbEntity
    {
        
        public ulong ClanId { get; set; }
        public string ClanName { get; set; }
        public ulong guildId { get; set; }
        public ulong OwnerId { get; set; }
        public ulong ClanMoney { get; set; }
        public ulong ClanSlots { get; set; }
        public string LogoUrl { get; set; }
        public ulong ClanRole { get; set; }
        public DateTime DateOfCreate { get; set; }
        //[NotMapped]
        //public IEnumerable<Users> DefUsers
        //{
        //    get
        //    {
        //        return new DbService().GetDbContext().Get(x => x.clanId == ClanId && x.clanInfo != "wait");
        //    }
        //}

        //[NotMapped]
        //public IEnumerable<Users> UsersWait
        //{
        //    get
        //    {
        //        return new EEF<Users>(new DBcontext()).Get(x => x.clanId == ClanId && x.clanInfo == "wait");
        //    }
        //}

        //[NotMapped]
        //public IEnumerable<Users> UsersModerators
        //{
        //    get
        //    {
        //        return new EEF<Users>(new DBcontext()).Get(x => x.clanId == ClanId && x.clanInfo == "moder");
        //    }
        //}

    }
}
