using System.Collections.Generic;

namespace DarlingBotNet.Services
{
    public class BotSettings
    {
        public const int TOTAL_SHARDS = 4;
        public static string config_file = "_config.yml";

        public static string bannerBoturl =
            "https://cdn.discordapp.com/attachments/642712334145421321/733726055382122566/-1_1.jpg";

        public static string Bot_Name = "DARLING";
        public static string Prefix = "h.";
        public static ulong SystemMessage = 715803775901761638;
        public static ulong hikaruid = 551373471536513024;
        public static ulong darlingbug = 726418172676276225;

        public static List<string> CommandNotInvise = new List<string>
        {
            "modules",
            "m",
            "commands",
            "c",
            "info",
            "i",
            "use",
            "u",
            "commandinvise",
            "ci"
        };
    }
}