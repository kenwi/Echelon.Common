using Echelon.Bot.Models;
using Echelon.Bot.Providers;
using Echelon.Bot.Systems;
using Microsoft.Extensions.DependencyInjection;

namespace Echelon.Bot.Services
{
    public class VGService : TimedServiceBase
    {
        private readonly IDocumentProvider documentProvider;
        private readonly VGSystem system;
        private readonly ulong channelId = 0;
        public VGService(
            VGSystem system,
            VGProvider documentProvider,
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
