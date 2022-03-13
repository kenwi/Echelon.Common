using Microsoft.Extensions.DependencyInjection;
using Echelon.Bot.Providers;
using Echelon.Bot.Systems;
using Echelon.Bot.Models;

namespace Echelon.Bot.Services
{
    public class NRKService : TimedServiceBase
    {
        private readonly IDocumentProvider documentProvider;
        private readonly NRKSystem system;
        private readonly ulong channelId = 0;

        public NRKService(
            NRKSystem system,
            NRKProvider documentProvider,
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter)
            : base(serviceProvider, messageWriter)
        {
            this.system = system;
            this.documentProvider = documentProvider;
            this.updateInterval = 5;
        }

        public override async void DoWork(object? state)
        {
            messageWriter.Write(GetServiceName());

            var document = await documentProvider.GetAsync();
            var newsPost = system.Execute(document);
            var queueSystem = serviceProvider.GetRequiredService<QueueSystem>();
            queueSystem.QueueMessage(new OutboundMessage
            {
                TargetID = channelId,
                Text = newsPost.ToString(),
                Caller = GetServiceName()
            });
        }
    }
}
