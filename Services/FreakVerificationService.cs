using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Echelon.Bot.Models;
using Echelon.Bot.Providers;
using Echelon.Bot.Components;
using Microsoft.Extensions.Configuration;

namespace Echelon.Bot.Services
{
    public class FreakVerificationService :
        TimedServiceBase<FreakPrivateMessageProvider, FreakPrivateMessageComponent, IEnumerable<FreakPrivateMessage>>
    {
        public FreakVerificationService(
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter)
            : base(serviceProvider, messageWriter)
        {
            this.updateInterval = 8;
        }

        public override async void DoWork(object? state)
        {
            await Task.Run(() => base.DoWork(state));
            if (message == null)
                return;

            var fileLines = await File.ReadAllLinesAsync("verified.txt");
            foreach (var m in this.message)
            {
                messageWriter.Write($"FreakPrivateMessage {m.Sender} {m.Title}");
                if (m.Title?.Length != Guid.NewGuid().ToString().Length)
                {
                    messageWriter.Write("Invalid key");
                    continue;
                }

                var isRegistered = fileLines.Where(line => 
                    line.Contains(m.Title!) 
                    && line.Contains("OK"))
                    .Any();

                if (isRegistered)
                {
                    messageWriter.Write("Already registered");
                    continue;
                }

                var id = fileLines.SingleOrDefault<string>(line => line.Contains(m.Title!))
                    ?.Split('\t')
                    ?.Skip(1)
                    ?.First();

                var discordUsername = fileLines.SingleOrDefault<string>(line => line.Contains(m.Title!))
                    ?.Split('\t')
                    ?.First();

                if (id == null || discordUsername == null)
                    continue;

                var userId = ulong.Parse(id);
                var queue = serviceProvider.GetRequiredService<QueueComponent>();
                queue.QueueMessage(new OutboundMessage
                {
                    TargetID = userId,
                    Text = $"Verifisering av Discord: {discordUsername} / Freak: {m.Sender} vellykket!",
                    Caller = GetServiceName()
                });
                messageWriter.Write($"{m.Sender} successfully registered");

                var content = await File.ReadAllTextAsync("verified.txt");
                content = content.Replace(m.Title, $"{m.Title}\t{m.Sender}\tOK");
                
                await File.WriteAllTextAsync("verified.txt", content);

                var configuration = serviceProvider.GetRequiredService<IConfigurationRoot>();
                var modsChannelId = configuration.GetValue<ulong>("modsChannelId");
                queue.QueueMessage(new OutboundMessage
                {
                    TargetID = modsChannelId,
                    Text = $"User successfully verified discord: {discordUsername} / freak: {m.Sender}",
                    Caller = GetServiceName()
                });
            }
        }
    }
}
