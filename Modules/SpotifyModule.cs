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
        public async Task Register()
        {
            if (!Context.IsUserInModeratorRole())
                return;

            try
            {
                var (verifier, challenge) = PKCEUtil.GenerateCodes();
                var uri = callbackService.CreateLoginRequestUri(challenge);
                var channels = await jsonService.GetChannels();

                channels.Add(Context.Channel.Id.ToString(), new SpotifyItem
                {
                    ChannelId = Context.Channel.Id.ToString(),
                    ChannelName = Context.Channel.Name,
                    OwnerId = Context.User.Id.ToString(),
                    OwnerName = Context.User.Username,
                    ServerName = Context.Guild.Name,
                    PlaylistName = $"{Context.Guild.Name} - {Context.Channel.Name}",
                    Challenge = challenge
                });

                await jsonService.SaveChannels(channels);
                await callbackService.StartAuthorizationProcess(verifier, challenge);
                await ReplyAsync($"Registered channel **{Context.Channel.Name}**. Please verify your Spotify account with the link received by PM");
                discordService.SendMessage($"Please login to Spotify with this link: {uri}", Context.User.Id, true);
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
                var channel = channels.FirstOrDefault(x => x.Key == Context.Channel.Id.ToString()).Value;

                if (channel == null)
                {
                    await ReplyAsync($"Channel **{Context.Channel.Name}** is not registered");
                    return;
                }

                channels.Remove(Context.Channel.Id.ToString());
                await jsonService.SaveChannels(channels);
                await ReplyAsync($"Unregistered channel **{Context.Channel.Name}**");
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
            }
        }

        [Command("create")]
        public async Task Create(string name)
        {
            if (!Context.IsUserInModeratorRole())
                return;

            ulong channelId = 0;
            try
            {
                var channels = await jsonService.GetChannels();
                channels.TryGetValue(Context.Channel.Id.ToString(), out var channel);
                if (channel is null)
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

        [Command("rename")]
        public async Task Rename(string name)
        {
            if (!Context.IsUserInModeratorRole())
                return;

            try
            {
                var channels = await jsonService.GetChannels();
                channels.TryGetValue(Context.Channel.Id.ToString(), out var channel);
                if (channel is null)
                {
                    await ReplyAsync($"Channel **{Context.Channel.Name}** is not registered");
                    return;
                }

                var oldname = channel.PlaylistName;
                channel.PlaylistName = name;
                channels[channel.ChannelId] = channel;
                await jsonService.SaveChannels(channels);

                await spotifyService.RenamePlaylist(Context.Channel.Id, channel.PlaylistId, name);
                await ReplyAsync($"Renamed playlist **{oldname}** to **{name}**");
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
                channels.TryGetValue(Context.Channel.Id.ToString(), out var channel);
                if (channel is null)
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

