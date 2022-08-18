using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace Echelon.Bot.Services
{
    public class SpotifyCallbackService : BackgroundService
    {
        private readonly IConfigurationRoot configuration;
        private readonly IMessageWriter messageWriter;
        private readonly IServiceProvider serviceProvider;
        EmbedIOAuthServer server;

        public SpotifyCallbackService(IConfigurationRoot configuration, IMessageWriter messageWriter, IServiceProvider serviceProvider)
        {
            this.configuration = configuration;
            this.messageWriter = messageWriter;
            this.serviceProvider = serviceProvider;
            var spotifyCallbackUri = new Uri(configuration["Spotify-CallbackUri"]);
            var spotifyCallbackUriPort = int.Parse(configuration["Spotify-CallbackUri-Port"]);
            
            server = new EmbedIOAuthServer(spotifyCallbackUri, spotifyCallbackUriPort);
            server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
            messageWriter.Write("SpotifyCallbackService Started");
        }

        private async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
        {
            var clientId = configuration["Spotify-ClientId"];
            var clientSecret = configuration["Spotify-ClientSecret"];
            var uri = new Uri(configuration["Spotify-CallbackUri-Public"]);

            var config = SpotifyClientConfig.CreateDefault();
            var tokenResponse = await new OAuthClient()
                .RequestToken(new AuthorizationCodeTokenRequest(clientId, clientSecret, response.Code, uri));

            var spotifyService = serviceProvider.GetRequiredService<SpotifyService>();
            spotifyService.SpotifyClient = new SpotifyClient(tokenResponse.AccessToken);

            var jsonService = serviceProvider.GetRequiredService<JsonService>();
            var items = await jsonService.GetItems();
            
            var selectedItem = items.Where(i => i.Value.AccessCode == "").FirstOrDefault();
            items[selectedItem.Key].AccessCode = tokenResponse.AccessToken;
            
            await jsonService.SaveItems(items);

            messageWriter.Write("Received Authorization Code");
            await server.Stop();
        }

        public string CreateLoginRequestUri()
        {
            var spotifyCallbackUriPublic = new Uri(configuration["Spotify-CallbackUri-Public"]);
            var clientId = configuration["Spotify-ClientId"];
            var loginRequest = new LoginRequest(spotifyCallbackUriPublic, clientId, LoginRequest.ResponseType.Code)
            {
                Scope = new[] {
                    Scopes.PlaylistModifyPrivate,
                    Scopes.PlaylistModifyPublic,
                    Scopes.PlaylistReadCollaborative,
                    Scopes.PlaylistReadPrivate,
                    Scopes.Streaming,
                }
            };
            return loginRequest.ToUri().ToString();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public void Start()
        {
            server.Start();
            messageWriter.Write("Started authentication server");
        }

        public void Stop()
        {
            server.Stop();
            messageWriter.Write("Stopped authentication server");
        }
    }
}

