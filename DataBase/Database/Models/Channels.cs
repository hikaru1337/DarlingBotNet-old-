using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DarlingBotNet.DataBase
{
    public class Channels
    {
        [Key]
        public ulong Id { get; set; }
        public ulong channelid { get; set; }
        public ulong guildid { get; set; }
        public bool UseCommand { get; set; }
        public bool UseRPcommand { get; set; }
        public bool GiveXP { get; set; }
        public bool DelUrl { get; set; }
        public bool DelUrlImage { get; set; }
        public bool DelCaps { get; set; }
        public bool Spaming { get; set; }
        public bool SendBadWord { get; set; }
        [NotMapped]
        public List<string> BadWordList
        {
            get
            {
                if (BadWordString != null)
                {
                    var comm = BadWordString.Split(',').ToList();
                    if (comm.First() == "") comm.Remove(comm.First());
                    if (comm == null) return new List<string>();
                    return comm;
                }
                return new List<string>();
            }
            set
            {
                BadWordString = string.Join(",", value.ToArray());
            }
        }
        public string BadWordString { get; set; }
        [NotMapped]
        public List<string> csUrlWhiteListList
        {
            get
            {
                if (csUrlWhiteListString != null)
                {
                    var comm = csUrlWhiteListString.Split(',').ToList();
                    if (comm.First() == "") comm.Remove(comm.First());
                    if (comm == null) return new List<string>();
                    return comm;
                }
                return new List<string>();
            }
            set
            {
                csUrlWhiteListString = string.Join(",", value.ToArray());
            }
        }
        public string csUrlWhiteListString { get; set; }
        public bool InviteMessage { get; set; }
    }
}
