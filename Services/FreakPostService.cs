using Echelon.Bot.Models;
using Echelon.Bot.Providers;
using Echelon.Bot.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Echelon.Bot.Services
{
    public class FreakPostService// : TimedServiceBase
        : TimedServiceBase<FreakPostProvider, FreakPostComponent, FreakPost>
    {
        private readonly IConfigurationRoot configuration;

        public FreakPostService(
            IConfigurationRoot configuration,
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter) 
            : base(serviceProvider, messageWriter)
        {
            this.updateInterval = 10;
            this.configuration = configuration;
        }

        public async override void DoWork(object? state)
        {
            await Task.Run(() => base.DoWork(state));
            if (this.message == null)
                return;

            var post = this.message as FreakPost;
            var message = $"Nytt innlegg i tråden {post.Title}, av {post.User}  {post.Link}";

            var queue = serviceProvider.GetRequiredService<QueueComponent>();
            queue.QueueMessage(new OutboundMessage
            {
                TargetID = configuration.GetValue<ulong>("defaultChannelId"),
                Text = message,
                Caller = GetServiceName()
            });
                        
            if(post != null && post.User != null)
            {
                var fileContent = await File.ReadAllLinesAsync("watchlist.txt");
                var isOnWatchlist = fileContent.Where(line =>
                    line.ToLower().Contains(post.User.ToLower()))
                    .Any();

                await Task.Delay(100);
                if (isOnWatchlist)
                {
                    queue.QueueMessage(new OutboundMessage
                    {
                        TargetID = configuration.GetValue<ulong>("modsWatchlistId"),
                        Text = "Watchlist: " + message,
                        Caller = GetServiceName()
                    });
                }
            }
        }
    }
}
