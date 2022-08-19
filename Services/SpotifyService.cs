using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace Echelon.Bot.Services
{
    public class SpotifyService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IMessageWriter messageWriter;

        public SpotifyClient? SpotifyClient { get; set; }

        public SpotifyService(IServiceProvider serviceProvider, IMessageWriter messageWriter)
        {
            this.serviceProvider = serviceProvider;
            this.messageWriter = messageWriter;
            messageWriter.Write("SpotifyClient Started");
        }

        public async Task<bool> AddSong(string accessCode, string link, string channelId, string playListId)
        {
            try
            {
                var trackId = link.Split(@"/")
                    .Last()
                    .Split("?")
                    .First();

                if (SpotifyClient is null)
                    SpotifyClient = new SpotifyClient(accessCode);
                                
                var track = await SpotifyClient.Tracks.Get(trackId);
                await SpotifyClient.Playlists.AddItems(playListId, new PlaylistAddItemsRequest(new[] { track.Uri }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
        
        public async Task<bool> PlayNextSong(string code)
        {
            try
            {
                if (SpotifyClient is null)
                    SpotifyClient = new SpotifyClient(code);

                await SpotifyClient.Player.SkipNext();
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

        /*public async Task<bool> PlaySong(string code, string url)
        {
            try
            {

                if (client == null)
                {
                    var token = await new OAuthClient()
                        .RequestToken(new AuthorizationCodeTokenRequest("a9bff9d392fd490cb3c7c90647886b07", "9ea9f1676ad54526a0db6d881042a694", code, new Uri("https://m0b.services/callback")));
                    
                    if (token.IsExpired)
                    {
                        messageWriter.Write("Expired token");
                    }
                    var config = SpotifyClientConfig
                          .CreateDefault()
                          .WithAuthenticator(new AuthorizationCodeAuthenticator("a9bff9d392fd490cb3c7c90647886b07", "9ea9f1676ad54526a0db6d881042a694", token));
                    client = new SpotifyClient(config);
                }

                
                var trackId = url.Split(@"/")
                    .Last()
                    .Split("?")
                    .First();
                var track = await client.Tracks.Get(trackId);
                await client.Player.AddToQueue(new PlayerAddToQueueRequest(track.Uri));
                Console.WriteLine(track.Uri);
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
                return false;
            }
            return true;
        }
        
        public async Task<bool> PlayNextSong(string code)
        {
            try
            {
                if (client == null)
                {
                    var token = await new OAuthClient().RequestToken(
                        new AuthorizationCodeTokenRequest("a9bff9d392fd490cb3c7c90647886b07", "9ea9f1676ad54526a0db6d881042a694", code, new Uri("https://m0b.services/callback"))
                        );
                    if (token.IsExpired)
                    {
                        messageWriter.Write("Expired token");
                    }

                    var config = SpotifyClientConfig
                          .CreateDefault()
                          .WithAuthenticator(new AuthorizationCodeAuthenticator("a9bff9d392fd490cb3c7c90647886b07", "9ea9f1676ad54526a0db6d881042a694", token));
                    client = new SpotifyClient(config);
                }
                await client.Player.SkipNext();
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
                return false;
            }
            return true;
        }
        
        public async Task<bool> AddSong(string code, string link, string channelId)
        {
            try
            {
                if(client == null)
                {
                    var token = await new OAuthClient().RequestToken(
                        new AuthorizationCodeTokenRequest("a9bff9d392fd490cb3c7c90647886b07", "9ea9f1676ad54526a0db6d881042a694", code, new Uri("https://m0b.services/callback"))
                        );
                    if (token.IsExpired)
                    {
                        messageWriter.Write("Expired token");
                    }
                    
                    var config = SpotifyClientConfig
                          .CreateDefault()
                          .WithAuthenticator(new AuthorizationCodeAuthenticator("a9bff9d392fd490cb3c7c90647886b07", "9ea9f1676ad54526a0db6d881042a694", token));
                    client = new SpotifyClient(config);
                }
                
                var trackId = link.Split(@"/")
                    .Last()
                    .Split("?")
                    .First();
                var track = await client.Tracks.Get(trackId);
                messageWriter.Write("GOT TRACK");
                await client.Playlists.AddItems("6tWMX0Hf2An9rP9vwAKNmP", new PlaylistAddItemsRequest(new[] { track.Uri }));
            }
            catch(Exception ex)
            {
                messageWriter.Write(ex.Message);
                return false;                
            }
            messageWriter.Write("OK");
            return true;

            var trackId = link.Split(@"/").Last();
            Console.WriteLine(trackId);

            var track = await client.Tracks.GetSeveral(new TracksRequest(new[] { trackId }));
            var playListId = "0VAT4R8LJJVJUWhfLA52e1?si=81e7950851a04504";
            await client.Playlists.AddItems(playListId, new PlaylistAddItemsRequest(new[] { trackId }));
        }

        // access_token=BQDEv3NNzc7IdxPFKb0m9TZVKdZ2ny74v1prQnArDubU3ngt3RvNxPyB3aG3BY0eKWBcjqrSFBISduASbuIJTWpaz8oEyH2-F9ZFiCcsf-JFv55qhlW1ivmyZ1aHBI-xNT-r7jnqbhymSBWiR2ebPhljLHGxWazai3n46XWmBpKCBysJ0Lzpqu7oHVE7yLtlXPsNo-BFufrccgcSeZf5D2AHjfXd8nmopacsiwTNt2gumC_jzQ&token_type=Bearer&expires_in=3600
        public override async void DoWork(object? state)
        {


            var playlists = client.Playlists.GetUsers("idlemob").Result;
             var singlePlaylist = playlists.Items.FirstOrDefault();


             await singlePlaylist.Tracks.Items.Add(track);
            ;
        }
        */
    }
}

