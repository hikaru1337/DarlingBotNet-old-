using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DarlingBotNet.DataBase
{
    public class Users
    {
        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        public bool Leaved { get; set; }
        public ulong GuildId { get; set; }
        public ulong XP { get; set; }
        public ulong Level
        {
            get
            {
                return (ulong)Math.Sqrt(XP / 80);
            }
        }
        public ulong ZeroCoin { get; set; }
        public ulong RealCoin { get; set; }
        public DateTime Daily { get; set; }
        public ulong Streak { get; set; }
        public uint countwarns { get; set; }
        public ulong marryedid { get; set; }
        public ulong ClanId { get; set; }
        public UserClanRole clanInfo { get; set; }
        public ulong Bank { get; set; }
        public DateTime BankTimer { get; set; }
        public DateTime BankLastTransit { get; set; }

        public enum UserClanRole
        {
            ready,
            user,
            moder,
            wait,
            owner
        }
    }
}
