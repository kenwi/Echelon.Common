using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpotifyAPI.Web;

namespace Echelon.Bot.Services
{
    public class SpotifyService
    {
        private readonly IMessageWriter messageWriter;
        private readonly string clientId;
        private readonly JsonService jsonService;

        public SpotifyClient? SpotifyClient { get; set; }

        public SpotifyService(IServiceProvider serviceProvider, IMessageWriter messageWriter, IConfigurationRoot configuration)
        {
            this.messageWriter = messageWriter;

            clientId = configuration["Spotify-ClientId"];
            jsonService = serviceProvider.GetRequiredService<JsonService>();

            messageWriter.Write("SpotifyClient Started");
        }

        async Task InitializeClient(string channelId)
        {
            var channels = await jsonService.GetItems();
            var currentChannel = channels.FirstOrDefault(c => c.Value.ChannelId == channelId);
            var token = currentChannel.Value.Token;

            var authenticator = new PKCEAuthenticator(clientId!, token!);
            authenticator.TokenRefreshed += async (sender, token) =>
            {
                channels[currentChannel.Key].TokenUpdated = DateTime.Now;
                channels[currentChannel.Key].Token = token;

                await jsonService.SaveItems(channels);
            };

            var config = SpotifyClientConfig.CreateDefault()
                .WithAuthenticator(authenticator);
            SpotifyClient = new SpotifyClient(config);
        }

        public async Task<FullPlaylist> CreatePlaylist(string name, string channelId)
        {
            try
            {
                await InitializeClient(channelId);
                var user = await SpotifyClient!.UserProfile.Current();
                var playlist = await SpotifyClient.Playlists.Create(user.Id, new PlaylistCreateRequest(name));
                await SpotifyClient.Playlists.ChangeDetails(playlist.Id!, new PlaylistChangeDetailsRequest() { Public = false, Collaborative = true });
                return playlist;
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
                return null!;
            }
        }

        public async Task<bool> AddSong(string link, string channelId, string playListId)
        {
            try
            {
                var trackId = link.Split(@"/")
                    .Last()
                    .Split("?")
                    .First();
                
                await InitializeClient(channelId);
                var track = await SpotifyClient!.Tracks.Get(trackId);
                await SpotifyClient.Playlists.AddItems(playListId, new PlaylistAddItemsRequest(new[] { track.Uri }));
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
                return false;
            }
            return true;
        }
        
        public async Task<bool> PlayNextSong(string channelId)
        {
            try
            {
                await InitializeClient(channelId);
                await SpotifyClient!.Player!.SkipNext();
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
                if(!string.IsNullOrEmpty(ex.StackTrace))
                    messageWriter.Write(ex.StackTrace);                
                return false;
            }
            return true;
        }

    }
}

