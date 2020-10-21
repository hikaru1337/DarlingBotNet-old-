using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DarlingBotNet.DataBase
{
    public class Clans
    {
        public ulong Id { get; set; }
        public string ClanName { get; set; }
        public ulong guildId { get; set; }
        public ulong OwnerId { get; set; }
        public long ClanMoney { get; set; }
        public uint ClanSlots { get; set; }
        public string LogoUrl { get; set; }
        public ulong ClanRole { get; set; }
        public DateTime DateOfCreate { get; set; }
        public DateTime LastClanSlotPays { get; set; }

        //public ulong ClanTop
        //{
        //    get
        //    {
        //        var clancount = new DBcontext().Clans.AsEnumerable().Where(x => x.DefUsers.Count() >= DefUsers.Count());

        //    }
        //}

        [NotMapped]
        public IEnumerable<Users> DefUsers
        {
            get
            {
                return new DBcontext().Users.AsQueryable().Where(x => x.clanId == (ulong)Id && x.clanInfo != Users.UserClanRole.wait);
            }
        }

        [NotMapped]
        public IEnumerable<Users> UsersWait
        {
            get
            {
                return new DBcontext().Users.AsQueryable().Where(x => x.clanId == (ulong)Id && x.clanInfo == Users.UserClanRole.wait);
            }
        }

        [NotMapped]
        public IEnumerable<Users> UsersModerators
        {
            get
            {
                return new DBcontext().Users.AsQueryable().Where(x => x.clanId == (ulong)Id && x.clanInfo == Users.UserClanRole.moder);
            }
        }

    }
}
