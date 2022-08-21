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
        private EmbedIOAuthServer? server;

        public SpotifyCallbackService(IConfigurationRoot configuration, IMessageWriter messageWriter, IServiceProvider serviceProvider)
        {
            this.configuration = configuration;
            this.messageWriter = messageWriter;

            clientId = configuration["Spotify-ClientId"];
            callbackUri = configuration["Spotify-CallbackUri-Public"];
            callbackPort = int.Parse(configuration["Spotify-CallbackUri-Port"]);
            
            jsonService = serviceProvider.GetRequiredService<JsonService>();
            spotifyService = serviceProvider.GetRequiredService<SpotifyService>();
            discordService = serviceProvider.GetRequiredService<DiscordService>();            
            
            messageWriter.Write("SpotifyCallbackService Started");
        }

        public string CreateLoginRequestUri(string challenge)
        {
            var spotifyCallbackUriPublic = new Uri(callbackUri);
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
                    Scopes.UserReadEmail
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
                    var channels = await jsonService.GetChannels();
                    var channel = channels.FirstOrDefault(i => i.Value.Challenge == challenge).Value;
                    var channelId = channel.ChannelId;
                    var (channelName, serverName) = (channel.ChannelName, channel.ServerName);
                    var playlistName = $"{serverName} - {channelName}";

                    var token = await new OAuthClient().RequestToken(
                        new PKCETokenRequest(clientId!, response.Code, new Uri(callbackUri), verifier)
                    );
                    channels[channelId].Token = token;
                    await jsonService.SaveChannels(channels);

                    var playlist = await spotifyService.CreatePlaylist(playlistName, ulong.Parse(channelId));
                    if(playlist is null)
                    {
                        discordService.SendMessage("Could not create playlist", ulong.Parse(channelId));
                        return;
                    }

                    channels[channelId].PlaylistId = playlist.Id!;
                    channels[channelId].PlaylistName = playlist.Name!;
                    await jsonService.SaveChannels(channels);

                    discordService.SendMessage($"Registered playlist **{playlistName}** {Environment.NewLine}https://open.spotify.com/playlist/{playlist.Id}", ulong.Parse(channelId));
                    messageWriter.Write("Received authorization for " + channelId);
                    await server.Stop();
                };            
                messageWriter.Write("Started authentication server. Waiting for authentication reply");
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
            }
        }

        public void Stop()
        {
            server?.Stop();
            messageWriter.Write("Stopped authentication server");
        }
    }
}

