using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System.Text.Json;

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
            messageWriter.Write("SpotifyCallbackService Started");
        }

        public string CreateLoginRequestUri(string challenge)
        {
            var spotifyCallbackUriPublic = new Uri(configuration["Spotify-CallbackUri-Public"]);
            var clientId = configuration["Spotify-ClientId"];
            var loginRequest = new LoginRequest(spotifyCallbackUriPublic, clientId, LoginRequest.ResponseType.Code)
            {
                CodeChallenge = challenge,
                CodeChallengeMethod = "S256",
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

        public async Task StartAuthorizationProcess(string verifier, string challenge)
        {
            var clientId = configuration["Spotify-ClientId"];
            await server.Start();
            server.AuthorizationCodeReceived += async (sender, response) =>
            {
                try
                {
                    var jsonService = serviceProvider.GetRequiredService<JsonService>();
                    var channels = await jsonService.GetItems();
                    var selectedChannel = channels.Where(i => i.Value.Challenge == challenge).FirstOrDefault();
                    
                    PKCETokenResponse token = await new OAuthClient().RequestToken(
                      new PKCETokenRequest(clientId!, response.Code, new Uri(configuration["Spotify-CallbackUri-Public"]), verifier)
                    );
                    channels[selectedChannel.Key].Token = JsonSerializer.Serialize<PKCETokenResponse>(token);
                    
                    await jsonService.SaveItems(channels);
                    await server.Stop();
                    messageWriter.Write("Received authorization for " + selectedChannel.Value.ChannelId);
                }
                catch(Exception ex)
                {
                    messageWriter.Write(ex.Message);
                }
            };
            messageWriter.Write("Started authentication server");
        }

        public void Stop()
        {
            server.Stop();
            messageWriter.Write("Stopped authentication server");
        }
    }
}

