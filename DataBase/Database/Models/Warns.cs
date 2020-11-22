namespace DarlingBotNet.DataBase
{
    public class Warns
    {
        public ulong Id { get; set; }
        public ulong GuildId { get; set; }
        public byte CountWarn { get; set; }
        public ulong MinutesWarn { get; set; }
        public ReportType ReportTypes { get; set; }

        public enum ReportType
        {
            ban,
            tban,
            mute,
            tmute,
            kick,
        }
    }
}
