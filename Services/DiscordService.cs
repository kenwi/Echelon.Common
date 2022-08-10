using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text;

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

        public DiscordService(
            IConfigurationRoot configuration,
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter)
        {
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
            this.messageWriter = messageWriter;
            this.commands = new CommandService();
            CreateDirectoriesIfNotExist();
        }

        private void CreateDirectoriesIfNotExist()
        {
            var paths = new[] { "/data/Logs", "/data/Logs/PM", "mc-inbound" }.ToList();
            paths.ForEach(directory => {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    messageWriter.Write($"Created {directory} directory");
                }
            });
        }

        public ConnectionState ConnectionState => client.ConnectionState;
        public DiscordSocketClient Client => client;

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
            client.ReactionAdded += Client_ReactionAdded;
            client.MessageDeleted += Client_MessageDeleted;
            client.UserJoined += Client_UserJoined;
            client.UserLeft += Client_UserLeft;
#elif DEBUG
            client.JoinedGuild += Client_JoinedGuild;
            client.GuildScheduledEventUserAdd += Client_GuildScheduledEventUserAdd;
            client.GuildMembersDownloaded += Client_GuildMembersDownloaded;
            client.GuildMemberUpdated += Client_GuildMemberUpdated;
            client.ReactionAdded += Client_ReactionAdded;
            client.MessageDeleted += Client_MessageDeleted;
            client.UserUpdated += Client_UserUpdated;
            client.PresenceUpdated += Client_PresenceUpdated;
            client.MessageUpdated += Client_MessageUpdated;
            client.RoleUpdated += Client_RoleUpdated;
            client.ButtonExecuted += Client_ButtonExecuted;
#endif

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
        }

        private async Task Client_MessageReceived(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null)
                return;

            if (message.Channel is not null)
            {
                var channel = message.Channel.Name;
                var filename = Path.Join("/data/", "Logs", $"{channel}.log");
                await File.AppendAllTextAsync(filename, $"[{DateTime.Now}] [{channel}] {message.Content}{Environment.NewLine}");

                if (channel.Contains("minecraft-"))
                {                    
                    var path = Path.Join("/app", "mc-inbound", $"{rng.Next()}-chat.txt");
                    var minecraftMessage = $"<{message.Author.Username}> {message.Content}";

                    if(message.Author.Username != "Echelon")
                    {
                        await File.AppendAllTextAsync(path, minecraftMessage);
                    }
                }
            }

            if (message.Author is not null)
            {
                var user = message.Author.Username;

                var filename = Path.Join("/data", "Logs", "PM", $"{user}.log");
                await File.AppendAllTextAsync(filename, $"[{DateTime.Now}] [{user}] {message.Content}{Environment.NewLine}");
            }

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


        private Task Client_JoinedGuild(SocketGuild arg)
        {
            return Task.CompletedTask;
        }

        private Task Client_GuildScheduledEventUserAdd(Cacheable<SocketUser, Discord.Rest.RestUser, IUser, ulong> arg1, SocketGuildEvent arg2)
        {
            return Task.CompletedTask;
        }

        private Task Client_GuildMembersDownloaded(SocketGuild arg)
        {
            return Task.CompletedTask;
        }

        private Task Client_ButtonExecuted(SocketMessageComponent arg)
        {
            return Task.CompletedTask;
        }

        private Task Client_RoleUpdated(SocketRole arg1, SocketRole arg2)
        {
            return Task.CompletedTask;
        }

        private Task Client_GuildMemberUpdated(Cacheable<SocketGuildUser, ulong> arg1, SocketGuildUser arg2)
        {
            return Task.CompletedTask;
        }

        private Task Client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            //NotifyMessage($"Message updated: {user.Username}");
            return Task.CompletedTask;
        }

        private Task Client_PresenceUpdated(SocketUser user, SocketPresence arg2, SocketPresence arg3)
        {
            NotifyMessage($"Presence updated: {user.Username}");
            return Task.CompletedTask;
        }

        private Task Client_UserUpdated(SocketUser arg1, SocketUser arg2)
        {
            return Task.CompletedTask;
        }

        private Task Client_UserLeft(SocketGuild arg1, SocketUser user)
        {
            NotifyMessage($"User left: {user.Username}");
            return Task.CompletedTask;
        }

        private Task Client_UserJoined(SocketGuildUser user)
        {
            NotifyMessage($"User joined: {user.Username}");
            return Task.CompletedTask;
        }

        private Task Client_MessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            NotifyMessage($"A message was deleted in #{channel.Value.Name}");
            return Task.CompletedTask;
        }

        private Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
        {
            //var filename = Path.Join("Logs", message.Channel.Name.Replace("#", "") + "-" + id + ".log");
            //await File.AppendAllTextAsync(filename, "DateTime\tChannel\tUser\tContent" + Environment.NewLine);
            //await File.AppendAllTextAsync(filename, msg);

            return Task.CompletedTask;
        }        
    }
}

