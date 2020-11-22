namespace DarlingBotNet.DataBase
{
    public class RoleSwaps
    {
        public ulong Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong GuildId { get; set; }
        public ulong Price { get; set; }
        public RoleType type { get; set; }


        public enum RoleType
        {
            RoleBuy,
            RoleGive
        }
    }
}