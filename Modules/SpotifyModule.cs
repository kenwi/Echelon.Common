using Discord.Commands;
using Echelon.Bot.Models;
using Echelon.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using SpotifyAPI.Web;

namespace Echelon.Bot.Services
{
    [Group("spotify")]
    public class SpotifyModule : ModuleBase<SocketCommandContext>
    {
        private readonly IMessageWriter messageWriter;
        private readonly JsonService jsonService;
        private readonly SpotifyCallbackService callbackService;
        private readonly SpotifyService spotifyService;
        private readonly DiscordService discordService;

        public SpotifyModule(IServiceProvider serviceProvider, IMessageWriter messageWriter)
        {
            this.messageWriter = messageWriter;

            jsonService = serviceProvider.GetRequiredService<JsonService>();
            callbackService = serviceProvider.GetRequiredService<SpotifyCallbackService>();
            spotifyService = serviceProvider.GetRequiredService<SpotifyService>();
            discordService = serviceProvider.GetRequiredService<DiscordService>();

            messageWriter.Write("SpotifyModule Started");
        }

        [Command("register")]
        public async Task Register(string playlistName = "")
        {
            if (!Context.IsUserInModeratorRole())
                return;

            try
            {
                var (verifier, challenge) = PKCEUtil.GenerateCodes();
                var uri = callbackService.CreateLoginRequestUri(challenge);
                var channels = await jsonService.GetChannels();

                if (!channels.TryGetValue(Context.Channel.Id, out var channel))
                {
                    await Context.Channel.SendMessageAsync("This channel is already registered");
                    return;
                }

                if (channels.TryAdd(Context.Channel.Id, new SpotifyItem
                {
                    ChannelId = Context.Channel.Id,
                    ChannelName = Context.Channel.Name,
                    OwnerId = Context.User.Id,
                    OwnerName = Context.User.Username,
                    ServerName = Context.Guild.Name,
                    PlaylistName = $"{Context.Guild.Name} - {Context.Channel.Name}",
                    Challenge = challenge
                }))
                {
                    await jsonService.SaveChannels(channels);

                    if (string.IsNullOrEmpty(playlistName))
                        await callbackService.StartAuthorizationProcess(verifier, challenge);
                    else
                        await callbackService.StartAuthorizationProcess(verifier, challenge, playlistName);

                    await ReplyAsync($"Registered channel **{Context.Channel.Name}**. Please verify your Spotify account with the link received by PM");
                    discordService.SendMessage($"Please login to Spotify with this link: {uri}", Context.User.Id, true);
                }
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
            }
        }

        [Command("unregister")]
        public async Task Unregister()
        {
            if (!Context.IsUserInModeratorRole())
                return;

            try
            {
                var channels = await jsonService.GetChannels();
                if (!channels.TryGetValue(Context.Channel.Id, out var channel))
                {
                    await ReplyAsync($"Channel **{Context.Channel.Name}** is not registered");
                    return;
                }

                channels.Remove(Context.Channel.Id);
                await jsonService.SaveChannels(channels);
                await ReplyAsync($"Unregistered channel **{Context.Channel.Name}**");
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
            }
        }

        [Command("createplaylist")]
        public async Task CreatePlaylist(string name)
        {
            if (!Context.IsUserInModeratorRole())
                return;

            ulong channelId = 0;
            try
            {
                var channels = await jsonService.GetChannels();
                if (!channels.TryGetValue(Context.Channel.Id, out var channel))
                {
                    await ReplyAsync($"Channel **{Context.Channel.Name}** is not registered");
                    return;
                }
                channelId = Context.Channel.Id;
                await spotifyService.CreatePlaylist(name, Context.Channel.Id);
            }
            catch (Exception ex)
            {
                discordService.SendMessage(ex.Message, channelId);
                messageWriter.Write(ex.Message);
            }
        }

        [Command("playlist")]
        public async Task GetChannelPlaylist()
        {
            if (!Context.IsUserInModeratorRole())
                return;

            ulong channelId = 0;
            try
            {
                var channels = await jsonService.GetChannels();
                if (!channels.TryGetValue(Context.Channel.Id, out var channel))
                {
                    await ReplyAsync($"Channel **{Context.Channel.Name}** is not registered");
                    return;
                }
                await ReplyAsync($"https://open.spotify.com/playlist/{channel.PlaylistId}");
            }
            catch (Exception ex)
            {
                discordService.SendMessage(ex.Message, channelId);
                messageWriter.Write(ex.Message);
            }
        }

        [Command("scope")]
        public async Task GetScope()
        {
            if (!Context.IsUserInModeratorRole())
                return;

            ulong channelId = 0;
            try
            {
                var channels = await jsonService.GetChannels();
                if (!channels.TryGetValue(Context.Channel.Id, out var channel))
                {
                    await ReplyAsync($"Channel **{Context.Channel.Name}** is not registered");
                    return;
                }
                await ReplyAsync($"Channel Token Scope: {channel.Token!.Scope}");
            }
            catch (Exception ex)
            {
                discordService.SendMessage(ex.Message, channelId);
                messageWriter.Write(ex.Message);
            }
        }

        [Command("rename")]
        public async Task Rename(string newName)
        {
            if (!Context.IsUserInModeratorRole())
                return;

            try
            {
                var channels = await jsonService.GetChannels();
                if (!channels.TryGetValue(Context.Channel.Id, out var channel))
                {
                    await ReplyAsync($"Channel **{Context.Channel.Name}** is not registered");
                    return;
                }

                var oldname = channel.PlaylistName;
                channel.PlaylistName = newName;
                channels[channel.ChannelId] = channel;
                await jsonService.SaveChannels(channels);

                await spotifyService.RenamePlaylist(Context.Channel.Id, channel.PlaylistId, newName);
                await ReplyAsync($"Renamed playlist **{oldname}** to **{newName}**");
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

            try
            {
                var channels = await jsonService.GetChannels();
                if (!channels.TryGetValue(Context.Channel.Id, out var channel))
                {
                    messageWriter.Write("Failed getting item");
                    return;
                }

                await spotifyService.PlayNextSong(Context.Channel.Id);
                await ReplyAsync("Playing next track");
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
            }
        }
    }
}

