using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DarlingBotNet.Services;
using NekosSharp;
using System.Threading.Tasks;

namespace DarlingBotNet.Modules
{
    [Name("RPgif")]
    public class RPgif : ModuleBase<SocketCommandContext>
    {
        private NekoClient NekoClient = new NekoClient("DARLING");

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Cuddle(SocketUser user = null)
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Cuddle GIF");
            Request Gif = await NekoClient.Action_v3.CuddleGif();
            if (Gif.Success)
                embed.WithImageUrl(Gif.ImageUrl).WithDescription($"{Context.User.Mention} Прижался(ась) {(user != null ? $"к {user.Mention}" : "")}");
            else embed.WithDescription("Повторите попытку");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Feed(SocketUser user = null)
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Feed GIF");
            Request Gif = await NekoClient.Action_v3.FeedGif();
            if (Gif.Success)
                embed.WithImageUrl(Gif.ImageUrl).WithDescription($"{Context.User.Mention} {(user != null ? $"кормит {user.Mention}" : "кушает")}");
            else embed.WithDescription("Повторите попытку");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Hug(SocketUser user = null)
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Hug GIF");
            Request Gif = await NekoClient.Action_v3.HugGif();
            if (Gif.Success)
                embed.WithImageUrl(Gif.ImageUrl).WithDescription($"{Context.User.Mention} {(user != null ? $"обнял(а) {user.Mention}" : "обнимается")}");
            else embed.WithDescription("Повторите попытку");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Kiss(SocketUser user = null)
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Kiss GIF");
            Request Gif = await NekoClient.Action_v3.KissGif();
            if (Gif.Success)
                embed.WithImageUrl(Gif.ImageUrl).WithDescription($"{Context.User.Mention} {(user != null ? $"поцеловал(а) {user.Mention}" : "целуется")}");
            else embed.WithDescription("Повторите попытку");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Pat(SocketUser user = null)
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Pat GIF");
            Request Gif = await NekoClient.Action_v3.PatGif();
            if (Gif.Success)
                embed.WithImageUrl(Gif.ImageUrl).WithDescription($"{Context.User.Mention} {(user != null ? $"погладил(а) {user.Mention}" : "погладил(а)")}");
            else embed.WithDescription("Повторите попытку");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Poke(SocketUser user = null)
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Poke GIF");
            Request Gif = await NekoClient.Action_v3.PokeGif();
            if (Gif.Success)
                embed.WithImageUrl(Gif.ImageUrl).WithDescription($"{Context.User.Mention} {(user != null ? $"ткнул(а) {user.Mention}" : "тыкает пустоту...")}");
            else embed.WithDescription("Повторите попытку");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Slap(SocketUser user = null)
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Slap GIF");
            Request Gif = await NekoClient.Action_v3.SlapGif();
            if (Gif.Success)
                embed.WithImageUrl(Gif.ImageUrl).WithDescription($"{Context.User.Mention} {(user != null ? $"дал(а) пощечину {user.Mention}" : "шлепнул пустоту...")}");
            else embed.WithDescription("Повторите попытку");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Tickle(SocketUser user = null)
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Tickle GIF");
            Request Gif = await NekoClient.Action_v3.TickleGif();
            if (Gif.Success)
                embed.WithImageUrl(Gif.ImageUrl).WithDescription($"{Context.User.Mention} {(user != null ? $"щекочет {user.Mention}" : "щекочет пустоту...")}");
            else embed.WithDescription("Повторите попытку");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Baka(SocketUser user = null)
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Baka GIF");
            Request Gif = await NekoClient.Image_v3.BakaGif();
            if (Gif.Success)
                embed.WithImageUrl(Gif.ImageUrl).WithDescription($"{Context.User.Mention} {(user != null ? $"сказал(а) что {user.Mention} дурак" : $"дурак")}");
            else embed.WithDescription("Повторите попытку");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
        public async Task Nekos()
        {
            var embed = new EmbedBuilder().WithColor(255, 0, 94).WithAuthor("Neko GIF");
            Request Gif = await NekoClient.Image_v3.NekoGif();
            if (Gif.Success)
                embed.WithImageUrl(Gif.ImageUrl).WithDescription($"{Context.User.Mention} приносит кошечку");
            else embed.WithDescription("Повторите попытку");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
