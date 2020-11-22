namespace DarlingBotNet.DataBase
{
    public class EmoteClick
    {
        public ulong Id { get; set; }
        public ulong GuildId { get; set; }
        public string emote { get; set; }
        public ulong MessageId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong RoleId { get; set; }
        public bool get { get; set; }
    }
}
