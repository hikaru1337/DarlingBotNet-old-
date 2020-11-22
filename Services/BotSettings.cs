using Discord;
using System;
using System.Collections.Generic;

namespace DarlingBotNet.Services
{
    public class BotSettings
    {
        public static string config_file = "_config.yml";
        public static string ConnectionString = @"Data Source = DarlingBotNet.db";
        public static string bannerBoturl = "https://cdn.discordapp.com/attachments/642712334145421321/733726055382122566/-1_1.jpg";
        public static string EnableDMmessageURL = "https://media.discordapp.net/attachments/642712334145421321/738149876549812375/assets2F-MBTPU7ahRWnPag_7AsM2F-MDRDtMippLlKyFKIQtV2F-MDRFGCyncfIm-uyHmhs2Fimage.png";
        public static string Bot_Name = "DARLING";
        public static string PayURL = "https://bill.discord-bot.net/botpay/get?token=5e43cb4e72a78";
        public static string PayUserURL = "https://bill.discord-bot.net/botpay/user?owner_id=551373471536513024&discord_id={0}&payment_method=qiwi&amount={1}";
        public static string Prefix = "h.";
        public static ulong SystemMessage = 715803775901761638;
        public static ulong hikaruid = 551373471536513024;
        public static ulong darlingbug = 726418172676276225;
        public static ulong darlingpre = 736506441388785704;
        public static ulong usermessage = 736545747020939305;
        public static string EmoteBoostNot = "<:BoostNot:773094372756815912>";
        public static string EmoteBoostNo = "<:BoostNo:772846181033574410>";
        public static string EmoteBoost = "<:Boost:772846179666100225>";
        public static string EmoteBoostLastDay = "<:BoostNoLastDay:772846181268586517>";
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
            "ci",
            "bug",
            "b",
            "pre",
            "pr",
            "boost",
            "invitebot"
        };
    }
}
