using Echelon.Bot.Models;
using Echelon.Bot.Providers;
using Echelon.Bot.Systems;
using Microsoft.Extensions.DependencyInjection;

namespace Echelon.Bot.Services
{
    public class FreakPostService// : TimedServiceBase
        : TimedServiceBase<FreakPostProvider, FreakPostSystem, FreakPost>
    {
        private readonly ulong channelId = 0;

        public FreakPostService(
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter) 
            : base(serviceProvider, messageWriter)
        {
            this.updateInterval = 10;
        }

        public async override void DoWork(object? state)
        {
            await Task.Run(() => base.DoWork(state));
            if (this.message == null)
                return;

            var post = this.message as FreakPost;
            var message = $"Nytt innlegg i tråden {post.Title}, av {post.User}  {post.Link}";

            var queueSystem = serviceProvider.GetRequiredService<QueueSystem>();
            queueSystem.QueueMessage(new OutboundMessage
            {
                TargetID = channelId,
                Text = message,
                Caller = GetServiceName()
            });
        }
    }
}
