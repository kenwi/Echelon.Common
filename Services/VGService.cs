using Echelon.Bot.Models;
using Echelon.Bot.Providers;
using Echelon.Bot.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Echelon.Bot.Services
{
    public class VGService : TimedServiceBase
    {
        private readonly IDocumentProvider documentProvider;
        private readonly VGComponent component;
        private readonly ulong channelId = 0;
        public VGService(
            VGComponent component,
            VGProvider documentProvider,
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter) 
            : base(serviceProvider, messageWriter)
        {
            this.component = component;
            this.documentProvider = documentProvider;
            this.updateInterval = 5;
        }

        public override async void DoWork(object? state)
        {
            messageWriter.Write(GetServiceName());

            var document = await documentProvider.GetAsync();
            var newsPost = component.Execute(document);
            var queue = serviceProvider.GetRequiredService<QueueComponent>();
            queue.QueueMessage(new OutboundMessage
            {
                TargetID = channelId,
                Text = newsPost.ToString(),
                Caller = GetServiceName()
            });
        }
    }
}
