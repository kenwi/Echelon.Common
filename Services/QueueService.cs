using Echelon.Bot.Systems;
using Microsoft.Extensions.DependencyInjection;

namespace Echelon.Bot.Services
{
    public class QueueService : TimedServiceBase
    {
        public QueueService(
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter)
            : base(serviceProvider, messageWriter)
        {
            this.updateInterval = 4;
            var queueSystem = serviceProvider.GetRequiredService<QueueSystem>();
        }

        public override async void DoWork(object? state)
        {
            await DispatchMessages();
        }

        public async Task DispatchMessages()
        {
            messageWriter.Write(GetServiceName());
            var queueSystem = serviceProvider.GetRequiredService<QueueSystem>();
            var hasWaitingMessages = !queueSystem.GetOutboundMessages().IsEmpty;
            messageWriter.Write(hasWaitingMessages ? "Dispatching messages" : "No messages in queue");

            await queueSystem.WriteToFile();

            var discordService = serviceProvider.GetRequiredService<DiscordService>();
            if (discordService.ConnectionState == Discord.ConnectionState.Connected)
            {
                var waitingMessages = queueSystem.GetOutboundMessages();
                while (waitingMessages.TryDequeue(out var message))
                {
                    discordService.SendMessage(message.Text, message.TargetID);
                }
            }
        }
    }
}

