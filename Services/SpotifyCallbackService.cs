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
        
        private readonly string clientId;
        private readonly string callbackUri;
        private readonly int callbackPort;
        private readonly JsonService jsonService;
        private readonly SpotifyService spotifyService;
        private readonly DiscordService discordService;
        EmbedIOAuthServer? server;

        public SpotifyCallbackService(IConfigurationRoot configuration, IMessageWriter messageWriter, IServiceProvider serviceProvider)
        {
            this.configuration = configuration;
            this.messageWriter = messageWriter;

            this.clientId = configuration["Spotify-ClientId"];
            this.callbackUri = configuration["Spotify-CallbackUri-Public"];
            this.callbackPort = int.Parse(configuration["Spotify-CallbackUri-Port"]);

            this.jsonService = serviceProvider.GetRequiredService<JsonService>();
            this.spotifyService = serviceProvider.GetRequiredService<SpotifyService>();
            this.discordService = serviceProvider.GetRequiredService<DiscordService>();            
            
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
            try
            {
                server = new EmbedIOAuthServer(new Uri(callbackUri), callbackPort);
                await server.Start();
                server.AuthorizationCodeReceived += async (sender, response) =>
                {
                    var channels = await jsonService.GetItems();
                    var currentChannel = channels.FirstOrDefault(i => i.Value.Challenge == challenge);
                    var (channelId, channelName, serverName) = (currentChannel.Value.ChannelId, currentChannel.Value.ChannelName, currentChannel.Value.ServerName);
                    var playlistName = $"{serverName} - {channelName}";
                    var key = currentChannel.Key;

                    var token = await new OAuthClient().RequestToken(
                        new PKCETokenRequest(clientId!, response.Code, new Uri(callbackUri), verifier)
                    );
                    channels[key].Token = token;
                    await jsonService.SaveItems(channels);

                    var playlist = await spotifyService.CreatePlaylist(playlistName, channelId);
                    channels[key].PlaylistId = playlist.Id!;
                    await jsonService.SaveItems(channels);

                    discordService.SendMessage($"Registered playlist **{playlistName}** {Environment.NewLine}https://open.spotify.com/playlist/{playlist.Id}", ulong.Parse(channelId));
                    messageWriter.Write("Received authorization for " + currentChannel.Value.ChannelId);
                };
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
            }
            messageWriter.Write("Started authentication server");
        }

        public void Stop()
        {
            server?.Stop();
            messageWriter.Write("Stopped authentication server");
        }
    }
}

