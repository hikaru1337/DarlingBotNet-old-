using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DarlingBotNet.Services;
using NekosSharp;
using System.Threading.Tasks;
using System.Linq;

namespace DarlingBotNet.Modules
{
    [Name("NSFWgif")]
    [NotDBCommand]
    public class NSFW : ModuleBase<SocketCommandContext>
    {
        private NekoClient NekoClient = new NekoClient("DARLING");

        private EmbedBuilder checkNSFW(SocketCommandContext Context)
        {
            var emb = new EmbedBuilder().WithColor(255, 0, 94);
            if (!(Context.Message.Channel as SocketTextChannel).IsNsfw)
            {
                var nsfw = Context.Guild.TextChannels.Where(x => x.IsNsfw);
                emb.WithDescription("Данный канал не является NSFW для использования этой команды.\n");
                if (nsfw == null)
                    emb.Description += "Попросите админа создать канал с параметров NSFW.";
                else
                    emb.Description += $"Используйте эту команду в {nsfw.FirstOrDefault().Mention}";
            }
            return emb;
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Yuri()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Yuri GIF 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.YuriGif();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Anal()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Anal GIF 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                var Gif = await NekoClient.Nsfw_v3.AnalGif();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Blowjob()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Blowjob GIF 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.BlowjobGif();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Boobs()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Boobs GIF 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.BoobsGif();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Classic()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Classic GIF 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.ClassicGif();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Cum()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Cum GIF 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.CumGif();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Feet()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Feet GIF 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.FeetGif();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Hentai()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Hentai GIF 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.HentaiGif();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Kuni()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Kuni GIF 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.KuniGif();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Neko()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Neko GIF 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.NekoGif();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Pussy()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Pussy GIF 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.PussyGif();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Pwank()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Pwank GIF 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.PwankGif();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Solo()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Solo GIF 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.SoloGif();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Spank()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Spank GIF 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.SpankGif();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task EroNeko()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"EroNeko PNG 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.EroNeko();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Ahegao()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Ahegao PNG 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.Ahegao();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Cosplay()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"Cosplay PNG 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.Cosplay();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task EroFeet()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"EroFeet PNG 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.EroFeet();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Trap()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor($"IT'S A TRAP PNG 18+");
            var nsfw = checkNSFW(Context);
            if (nsfw.Description == null)
            {
                Request Gif = await NekoClient.Nsfw_v3.Trap();
                if (Gif.Success) embed.WithImageUrl(Gif.ImageUrl);
                else embed.WithDescription("Повторите попытку");
            }
            else embed = nsfw;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

    }
}
