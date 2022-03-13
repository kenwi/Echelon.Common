using Echelon.Bot.Models;
using Echelon.Bot.Providers;
using Echelon.Bot.Systems;
using Microsoft.Extensions.DependencyInjection;

namespace Echelon.Bot.Services
{
    public class LiveuamapService : TimedServiceBase
    {
        private readonly LiveuamapPostProvider documentProvider;
        private readonly LiveuamapPostSystem postParser;
        private readonly LiveuamapPopupSystem popupParser;
        private readonly ulong channelId = 0;
        private string currentTarget = "";
        public LiveuamapService(
            LiveuamapPostSystem postParser,
            LiveuamapPopupSystem popupParser,
            LiveuamapPostProvider documentProvider,
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter)
            : base(serviceProvider, messageWriter)
        {
            this.postParser = postParser;
            this.popupParser = popupParser;
            this.documentProvider = documentProvider;
            this.updateInterval = 30;
        }

        public override async void DoWork(object? state)
        {
            messageWriter.Write(GetServiceName());
            foreach (var target in new string[] { "ukraine", "russia", "cyberwar" })
            {
                documentProvider.SetTarget(target);
                var document = await documentProvider.GetAsync();
                var post = postParser.Execute(document);

                documentProvider.SetUrl(post.Link!);
                var popupDocument = await documentProvider.GetAsync();
                var link = popupParser.Execute(popupDocument);
                post.Link = link;
                var message = $"{post.Title} {post.Link}";

                currentTarget = target;
                var queueSystem = serviceProvider.GetRequiredService<QueueSystem>();
                queueSystem.QueueMessage(new OutboundMessage
                {
                    TargetID = channelId,
                    Text = message,
                    Caller = GetServiceName()
                });
            }
        }

        public override string GetServiceName() => $"{base.GetServiceName()}({currentTarget})";
    }
}
