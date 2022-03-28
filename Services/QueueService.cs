using Echelon.Bot.Components;
using Echelon.Bot.Models;
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
        }

        public override async void DoWork(object? state)
        {
            await DispatchMessages();
        }

        public void DispatchDiscordMessages()
        {
            var queue = serviceProvider.GetRequiredService<QueueComponent>();
            var discordService = serviceProvider.GetRequiredService<DiscordService>();
            if (discordService.ConnectionState == Discord.ConnectionState.Connected)
            {
                var waitingMessages = queue.GetOutboundMessages();
                while (waitingMessages.TryDequeue(out var message))
                {
                    if (message == null)
                        continue;
                    if (message.Text == null)
                        continue;
                    if (message.MessageType == OutboundMessageType.Freak)
                        continue;

                    discordService.SendMessage(message.Text, message.TargetID);
                }
            }
        }

        public async Task DispatchMessages()
        {
            var queue = serviceProvider.GetRequiredService<QueueComponent>();
            messageWriter.Write(GetServiceName());
            var hasWaitingMessages = !queue.GetOutboundMessages().IsEmpty;
            messageWriter.Write(hasWaitingMessages ? "Dispatching messages" : "No messages in queue");
            await queue.WriteToFile();

            DispatchDiscordMessages();

            var freakMessages = queue.GetOutboundMessages()
                .Where(message => message != null
                    && message.MessageType == OutboundMessageType.Freak);

            var otherMessages = queue.GetOutboundMessages()
                .Where(message => message != null
                && message.MessageType != OutboundMessageType.Freak);

            if (freakMessages.Any())
            {
                // dispatch
            }

            if(otherMessages.Any())
            {
                // dispatch
            }
        }
    }
}

