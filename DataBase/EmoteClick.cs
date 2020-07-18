using System.ComponentModel.DataAnnotations;

namespace DarlingBotNet.DataBase
{
    public class EmoteClick
    {
        [Key] public int Id { get; set; }

        public ulong guildid { get; set; }
        public string emote { get; set; }
        public ulong messageid { get; set; }
        public ulong channelid { get; set; }
        public ulong roleid { get; set; }
        public bool get { get; set; }
    }
}