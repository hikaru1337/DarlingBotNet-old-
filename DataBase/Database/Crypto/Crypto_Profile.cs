using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DarlingBotNet.DataBase.Database.Crypto
{
    public class Crypto_Profile
    {
        public ulong id { get; set; }
        public ulong UserId { get; set; }
        public ulong HacksStreak { get; set; }
        public DateTime HackLast { get; set; }

        public ulong WorkId { get; set; }
        public ulong WorkStreak { get; set; }
        public DateTime WorkLast { get; set; }

        public ulong StudysId { get; set; }
        public ushort StudysDays { get; set; }
        public DateTime StudysLast { get; set; }

        public ulong ZeroCoins
        {
            get
            {
                using (var DBcontext = new DBcontext())
                {
                    return DBcontext.Users.FirstOrDefault(x => x.UserId == UserId).ZeroCoin;
                }
            }
        }

        //public IEnumerable<Crypto_Study> Studys
        //{
        //    get
        //    {
        //        using (var DBcontext = new DBcontext())
        //        {
        //            var Studys = DBcontext.Crypto_Studys.AsQueryable().Where(x => x.UserId == UserId);
        //            if (Studys.Count() == 0)
        //                return null;

        //            List<Crypto_Study> IStudys = new List<Crypto_Study>();

        //            foreach (var Study in Studys)
        //            {
        //                IStudys.Add(DBcontext.Crypto_Study.FirstOrDefault(x => x.StudyId == Study.StudyId));
        //            }
        //            return IStudys.AsEnumerable();
        //        }

        //    }
        //}
    }
}
