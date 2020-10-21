using System;
using System.ComponentModel.DataAnnotations;

namespace DarlingBotNet.DataBase.Database.Models
{
    public class Tasks
    {
        [Key]
        public ulong Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string Message { get; set; }
        public DateTime Times { get; set; }
        public bool Repeat { get; set; }
    }
}
