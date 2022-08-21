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

        async Task InitializeClient(ulong channelId)
        {
            var channels = await jsonService.GetChannels();
            var currentChannel = channels.FirstOrDefault(c => c.Value.ChannelId == channelId);
            var key = currentChannel.Key;
            var token = currentChannel.Value.Token;

            var authenticator = new PKCEAuthenticator(clientId!, token!);
            authenticator.TokenRefreshed += async (sender, token) =>
            {
                channels[key].TokenUpdated = DateTime.Now;
                channels[key].Token = token;

                await jsonService.SaveChannels(channels);
            };

            var config = SpotifyClientConfig.CreateDefault()
                .WithAuthenticator(authenticator);
            SpotifyClient = new SpotifyClient(config);
        }

        public async Task<FullPlaylist> CreatePlaylist(string name, ulong channelId)
        {
            try
            {
                await InitializeClient(channelId);

                var user = await SpotifyClient!.UserProfile.Current();
                var playlist = await SpotifyClient.Playlists.Create(user.Id, new PlaylistCreateRequest(name));
                await SpotifyClient.Playlists.ChangeDetails(playlist.Id!, new PlaylistChangeDetailsRequest() { Public = false, Collaborative = true });
                return playlist;
            }
            catch(APIException ex)
            {
                messageWriter.Write(ex.Message);

                var response = ex?.Response?.Body?.ToString();
                if(!string.IsNullOrEmpty(response))
                    messageWriter.Write(response);
                
                throw new Exception(response);
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
                messageWriter.Write(ex.HResult.ToString());

                return null!;
            }
        }

        public async Task<bool> AddSong(string link, ulong channelId, string playListId)
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
                return true;
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
                return false;
            }
        }
        
        public async Task<bool> PlayNextSong(ulong channelId)
        {
            try
            {
                await InitializeClient(channelId);
                await SpotifyClient!.Player!.SkipNext();
                return true;
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
                if(!string.IsNullOrEmpty(ex.StackTrace))
                    messageWriter.Write(ex.StackTrace);                
                return false;
            }
        }

        public async Task<bool> RenamePlaylist(ulong channelId, string playlistId, string newName)
        {
            try
            {
                await InitializeClient(channelId);
                await SpotifyClient!.Playlists.ChangeDetails(playlistId, new PlaylistChangeDetailsRequest() { Name = newName });
                return true;
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
                return false;
            }
        }

        internal Task GetPlaylist(string playlistId)
        {
            throw new NotImplementedException();
        }
    }
}

