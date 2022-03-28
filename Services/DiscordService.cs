using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Echelon.Bot.Services
{
    public class DiscordService : BackgroundService
    {
        private readonly DiscordSocketClient client = new ();
        private readonly IConfigurationRoot configuration;
        private readonly IServiceProvider serviceProvider;
        private readonly IMessageWriter messageWriter;
        private readonly CommandService commands;

        public DiscordService(
            IConfigurationRoot configuration,
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter)
        {
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
            this.messageWriter = messageWriter;
            this.commands = new CommandService();
        }

        public ConnectionState ConnectionState => client.ConnectionState;
        public DiscordSocketClient Client => client;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var token = configuration.GetValue<string>("token");
            client.Log += (logMessage) =>
            {
                messageWriter.Write(logMessage.Message);
                return Task.CompletedTask;
            };
            client.MessageReceived += Client_MessageReceived;

            await commands.AddModuleAsync<NoteModule>(null);
            await commands.AddModuleAsync<VerificationProcessModule>(null);
            await commands.AddModuleAsync<ActivityModule>(null);
            await commands.AddModuleAsync<HelpModule>(null);
            await commands.AddModuleAsync<VerificationModule>(null);
            await commands.AddModuleAsync<WatchlistModule>(null);
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
        }

        private async Task Client_MessageReceived(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) 
                return;

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

        public async void SendMessage(string message, ulong channelID)
        {
            messageWriter.Write($"DiscordService({channelID}): {message}");
            if (channelID == 0)
                return;

            if (await client.GetUserAsync(channelID) is IUser user)
                await user.SendMessageAsync(message);

            if (await client.GetChannelAsync(channelID) is IMessageChannel channel)
                await channel.SendMessageAsync(message);
        }
    }
}

