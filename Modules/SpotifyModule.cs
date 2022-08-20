using Discord.Commands;
using Echelon.Bot.Models;
using Echelon.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpotifyAPI.Web;

namespace Echelon.Bot.Services
{
    [Group("spotify")]
    public class SpotifyModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IMessageWriter messageWriter;
        private readonly JsonService jsonService;
        private readonly SpotifyCallbackService callbackService;
        private readonly SpotifyService spotifyService;

        public SpotifyModule(IServiceProvider serviceProvider, IMessageWriter messageWriter)
        {
            this.serviceProvider = serviceProvider;
            this.messageWriter = messageWriter;

            jsonService = serviceProvider.GetRequiredService<JsonService>();
            callbackService = serviceProvider.GetRequiredService<SpotifyCallbackService>();
            spotifyService = serviceProvider.GetRequiredService<SpotifyService>();

            messageWriter.Write("SpotifyModule Started");
        }

        private async void PrintHelp(ulong channelId)
        {
            await ReplyAsync("Please provide a channel-id and playlist-id or link to a playlist"
                + "```!spotify register <discord-channel-id> <spotify-playlist-id-or-link>```"
                + "For this specific channel example: "
                + $"```!spotify register {channelId} https://open.spotify.com/playlist/4a16Ug0LyXkAAlVZah5X2k?si=0e79cae526094196```");
        }

        [Command("register")]
        public async Task Register(string channelId = "", string playlistId = "")
        {
            if (!Context.IsUserInModeratorRole())
                return;

            try
            {
                var items = await jsonService.GetItems();
                var (verifier, challenge) = PKCEUtil.GenerateCodes();
                var uri = callbackService.CreateLoginRequestUri(challenge);

                items.Add(Context.Channel.Id.ToString(), new SpotifyItem
                {
                    ChannelId = Context.Channel.Id.ToString(),
                    ChannelName = Context.Channel.Name,
                    PlaylistId = playlistId,
                    OwnerId = Context.User.Id.ToString(),
                    OwnerName = Context.User.Username,
                    ServerName = Context.Guild.Name,
                    Challenge = challenge
                });
                await jsonService.SaveItems(items);

                var code = uri.Split("code=").Last();
                await callbackService.StartAuthorizationProcess(verifier, challenge);

                await ReplyAsync($"Registered channel **{Context.Channel.Name}**. Please verify your Spotify account with the link received by PM");

                var discordService = serviceProvider.GetRequiredService<DiscordService>();
                discordService.SendMessage($"Please login to Spotify with this link: {uri}", Context.User.Id, true);
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
            }
        }

        [Command("next")]
        public async Task Next()
        {
            if (!Context.IsUserInModeratorRole())
                return;

            var items = await jsonService.GetItems();
            if (items.ContainsKey(Context.Channel.Id.ToString()))
            {
                items.TryGetValue(Context.Channel.Id.ToString(), out var item);
                if (item is null)
                {
                    messageWriter.Write("Failed getting item");
                    return;
                }
                await spotifyService.PlayNextSong(Context.Channel.Id.ToString());
                await ReplyAsync("Playing next track");
            }
        }
    }
}

