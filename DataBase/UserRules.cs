using System.ComponentModel.DataAnnotations;

namespace DarlingBotNet.DataBase
{
    public class UserRules
    {
        [Key] public ulong Id { get; set; }

        public ulong guildId { get; set; }
        public ulong userId { get; set; }
        public ulong RulesNumber { get; set; }
        public ulong RulesPoint { get; set; }
    }
}