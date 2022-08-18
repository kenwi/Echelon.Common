using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace Echelon.Bot.Services
{
    public class SpotifyCallbackService : BackgroundService
    {
        private readonly IConfigurationRoot configuration;
        private readonly IMessageWriter messageWriter;
        EmbedIOAuthServer server;

        public SpotifyCallbackService(IConfigurationRoot configuration, IMessageWriter messageWriter)
        {
            this.configuration = configuration;
            this.messageWriter = messageWriter;

            var spotifyCallbackUri  = new Uri(configuration["Spotify-CallbackUri"]);
            var spotifyCallbackUriPort = int.Parse(configuration["Spotify-CallbackUri-Port"]);            
            server = new EmbedIOAuthServer(spotifyCallbackUri, spotifyCallbackUriPort);
        }

        public string CreateLoginRequestUri()
        {
            var spotifyCallbackUriPublic  = new Uri(configuration["Spotify-CallbackUri-Public"]);
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
            server.Start();
            messageWriter.Write("Started SpotifyCallbackService");
            return Task.CompletedTask;
        }
    }
}

