using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Echelon.Bot.Services
{
    public class DiscordService : BackgroundService
    {
        private readonly DiscordSocketClient client = new();
        private readonly IConfigurationRoot configuration;
        private readonly IServiceProvider serviceProvider;
        private readonly IMessageWriter messageWriter;
        private readonly CommandService commands;
        private readonly Random rng = new Random((int)DateTime.Now.Ticks);
        private bool hasAlreadyFailed = false;
        public ConnectionState ConnectionState => client.ConnectionState;
        public DiscordSocketClient Client => client;

        
        public DiscordService(
            IConfigurationRoot configuration,
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter)
        {
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
            this.messageWriter = messageWriter;
            this.commands = new CommandService();

#if RELEASE
            CreateDirectoriesIfNotExist();
#endif
        }

        private void CreateDirectoriesIfNotExist()
        {
            var paths = new[] { "/data/Logs", "/data/Logs/PM", "mc-inbound" }.ToList();
            paths.ForEach(directory =>
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    messageWriter.Write($"Created {directory} directory");
                }
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var token = configuration.GetValue<string>("token");

#if RELEASE
            client.Log += (logMessage) =>
            {
                messageWriter.Write(logMessage.Message ?? logMessage.Exception.Message);
                return Task.CompletedTask;
            };
            client.MessageReceived += Client_MessageReceived;
            //client.ReactionAdded += Client_ReactionAdded;
            //client.MessageDeleted += Client_MessageDeleted;
            //client.UserJoined += Client_UserJoined;
            //client.UserLeft += Client_UserLeft;
#elif DEBUG
            client.MessageReceived += Client_MessageReceived;
#endif
            await commands.AddModuleAsync<SpotifyModule>(serviceProvider);
            await commands.AddModuleAsync<YoutubeModule>(null);
            await commands.AddModuleAsync<NoteModule>(null);
            await commands.AddModuleAsync<VerificationProcessModule>(null);
            await commands.AddModuleAsync<ActivityModule>(null);
            await commands.AddModuleAsync<HelpModule>(null);
            await commands.AddModuleAsync<VerificationModule>(null);
            await commands.AddModuleAsync<WatchlistModule>(null);
            await commands.AddModuleAsync<NotificationModule>(null);

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            messageWriter.Write("Connected to Discord");
        }

        private async Task Client_MessageReceived(SocketMessage messageParam)
        {
            
           var message = messageParam as SocketUserMessage;
            if (message == null)
            {
                messageWriter.Write("Null message");
                return;
            }


            var channel = message.Channel as SocketGuildChannel;
            var serverName = channel?.Guild.Name;
            var channelName = message.Channel.Name;

            var replace = new string[] { @"\", "@", "$", "#", "£", "(", ")" };
            for (var i = 0; i < replace.Length; i++)
            {
                serverName = serverName?.Replace(replace[i], "");
                channelName = channelName?.Replace(replace[i], "");
            }

#if RELEASE
            await ExecuteLogging(messageParam);
#endif

            if (!message.Content.StartsWith("!"))
                await ParseSocketMessageSpotify(messageParam);

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: serviceProvider);
        }

        private async Task ExecuteLogging(SocketMessage message)
        {
            var channel = message.Channel as SocketGuildChannel;
            if(channel is null)
            {
                messageWriter.Write("Received PM");
                if (message.Author is not null)
                {
                    var user = message.Author.Username;
                    await File.AppendAllTextAsync(Path.Join("/data", "Logs", "PM", $"{user}.log"), $"[{DateTime.Now}] [{user}] {message.Content}{Environment.NewLine}");
                    return;
                }                
            }
            
            var serverName = channel?.Guild.Name;
            var channelName = message.Channel.Name;
            var userName = message.Author!.Username;

            var replace = new string[] { @"\", "@", "$", "#", "£", "(", ")" };
            for (var i = 0; i < replace.Length; i++)
            {
                serverName = serverName?.Replace(replace[i], "");
                channelName = channelName?.Replace(replace[i], "");
                userName = userName?.Replace(replace[i], "");
            }

            var directory = Path.Join("/data/", "Logs", serverName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                messageWriter.Write($"Created {directory} directory");
            }

            var filename = Path.Join("/data/", "Logs", serverName, $"{channelName}.log");
            await File.AppendAllTextAsync(filename, $"[{DateTime.Now}] [{channelName}] {message.Content}{Environment.NewLine}");

            if (string.IsNullOrEmpty(channelName))
                return;

            if (channelName.Contains("minecraft-"))
            {
                var path = Path.Join("/app", "mc-inbound", $"{rng.Next()}-chat.txt");
                //var stringReplacements = configuration.GetValue<Dictionary<string, string>>("minecraftStringReplacements");
                //stringReplacements.TryGetValue(message.Author.Username, out string? minecraftNick);
                var minecraftMessage = $"<{message.Author.Username}> {message.Content}";
                if (!message.Author.IsBot)
                    await File.AppendAllTextAsync(path, minecraftMessage);
            }
        }

        private async Task ParseSocketMessageSpotify(SocketMessage messageParam)
        {
            if (messageParam is not SocketUserMessage message)
                return;

            try
            {
                var jsonService = serviceProvider.GetRequiredService<JsonService>();
                var spotifyService = serviceProvider.GetRequiredService<SpotifyService>();
                var channels = await jsonService.GetChannels();
                var channelId = message.Channel.Id;

                if (!message.Content.Contains("https://open.spotify.com/track/"))
                    return;

                var link = message.Content
                    .Split(" ")
                    .Where(word => word.Contains("https://open.spotify.com/track"))
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(link))
                    link = message.Content;

                channels.TryGetValue(channelId, out var channel);
                if (channel is null)
                    return;

                if (string.IsNullOrEmpty(channel.ChannelName))
                {
                    channels[channelId].ChannelName = message.Channel.Name;
                    await jsonService.SaveChannels(channels);
                    messageWriter.Write("Updated channel name");
                }
                
                if (!await spotifyService.AddSong(link, channelId, channel.PlaylistId))
                {
                    if (!hasAlreadyFailed)
                    {
                        SendMessage("Failed", message.Channel.Id);
                        hasAlreadyFailed = true;
                    }
                    messageWriter.Write("Failed " + message.Channel.Id);
                    return;
                }
                
                SendMessage("Added track to playlist", message.Channel.Id);
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
                if(!string.IsNullOrEmpty(ex.StackTrace))
                    messageWriter.Write(ex.StackTrace);
            }
        }

        public async void SetRole(uint userId, string roleName)
        {
            var role = client.Guilds
                ?.FirstOrDefault()
                ?.Roles
                ?.FirstOrDefault(u => u.Name == roleName);

            if (client.GetUser(userId) is IGuildUser user)
                await user.AddRoleAsync(role);
        }

        public async void NotifyMessage(string message)
        {
            SendMessage(message, 998632144731644095);
            await Task.CompletedTask;
        }

        public async void SendMessage(string message, ulong channelID, bool isPrivateMessage = false)
        {
            if (channelID == 0)
                return;

            var messageType = "";
            if (isPrivateMessage && await client.GetUserAsync(channelID) is IUser user)
            {
                messageType = "UserPM";
                await user.SendMessageAsync(message);
            }

            if (await client.GetChannelAsync(channelID) is IMessageChannel channel)
            {
                messageType = "ChannelMessage";
                await channel.SendMessageAsync(message);
            }
            messageWriter.Write($"DiscordService({messageType}:{channelID}): {message}");
        }
    }
}
