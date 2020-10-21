using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DarlingBotNet.DataBase
{
    public class Users
    {
        public ulong Id { get; set; }
        public ulong userid { get; set; }
        public bool Leaved { get; set; }
        public ulong guildId { get; set; }
        public ulong XP { get; set; }
        public ulong Level
        {
            get
            {
                return (ulong)Math.Sqrt(XP / 80);
            }
        }
        public ulong ZeroCoin { get; set; }
        public DateTime Daily { get; set; }
        public ulong Streak { get; set; }
        public uint countwarns { get; set; }
        public ulong marryedid { get; set; }
        public ulong clanId { get; set; }
        public UserClanRole clanInfo { get; set; }
        public ulong Bank { get; set; }
        public DateTime BankTimer { get; set; }
        public DateTime BankLastTransit { get; set; }
        public uint ClanOwner
        {
            get
            {
                var clan = new DBcontext().Clans.AsQueryable().FirstOrDefault(x => x.OwnerId == userid);
                if (clan == null) return 0;
                return (uint)clan.Id;
            }
        }

        public enum UserClanRole
        {
            ready,
            user,
            moder,
            wait
        }
    }
}
