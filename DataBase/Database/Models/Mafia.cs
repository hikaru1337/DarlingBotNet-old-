//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace DarlingBotNet.DataBase
//{

//    public class mafia
//    {
//        [Key]
//        public ulong Id { get; set; }
//        public ulong guildId { get; set; }
//        [NotMapped]
//        public IEnumerable<mafiaUsers> UserList
//        {
//            get
//            {
//                return new EEF<mafiaUsers>(new DataBases()).Get(x => x.guildId == guildId && x.mafiaId == Id);
//            }
//        }
//        public ulong KuratorId { get; set; }
//        public ulong categoryid { get; set; }
//        public ulong voiceid { get; set; }
//        public ulong chatid { get; set; }
//        public ulong mafiasid { get; set; }
//        public ulong doctorsid { get; set; }
//        public ulong policesid { get; set; }
//        public ulong putanasid { get; set; }
//        public bool DayOrNight { get; set; }
//        public ulong Days { get; set; }
//        public ulong KuratorVotedCount { get; set; }
//    }

//    public class mafiaUsers
//    {
//        [Key]
//        public ulong Id { get; set; }
//        public ulong userId { get; set; }
//        public ulong mafiaId { get; set; }
//        public ulong guildId { get; set; }
//        public string Role { get; set; }
//        public bool Killed { get; set; }
//        public bool Voted { get; set; }
//        public bool Sleep { get; set; }
//    }

//    public class mafiaRoleVoted
//    {
//        [Key]
//        public ulong Id { get; set; }
//        public ulong Role { get; set; }
//        public ulong mafiaId { get; set; }
//        public ulong guildId { get; set; }
//        public ulong UserVictimId { get; set; }
//        public ulong Day { get; set; }
//    }

//}
