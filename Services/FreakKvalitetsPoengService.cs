using Echelon.Bot.Models;
using Echelon.Bot.Providers;
using Echelon.Bot.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Echelon.Bot.Services
{
    public class FreakKvalitetsPoengService
        : TimedServiceBase<FreakKvalitetsPoengProvider, FreakKvalitetsPoengComponent, FreakKvalitetsPoeng>
    {
        private readonly IConfigurationRoot configuration;
        public FreakKvalitetsPoengService(
            IConfigurationRoot configuration,
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter)
            : base(serviceProvider, messageWriter)
        {
            this.updateInterval = 6;
            this.configuration = configuration;
        }

        public override async void DoWork(object? state)
        {
            await Task.Run(() => base.DoWork(state));
            if (this.message == null)
                return;

            var kvalitetspoeng = this.message as FreakKvalitetsPoeng;
            var message = $"Kvalitetspoeng til {kvalitetspoeng.ToUser} fra {kvalitetspoeng.FromUser} i forumet {kvalitetspoeng.Forum} {kvalitetspoeng.Link}";

            var queue = serviceProvider.GetRequiredService<QueueComponent>();
            queue.QueueMessage(new OutboundMessage
            {
                TargetID = configuration.GetValue<ulong>("kvalitetspoengChannelId"),
                Text = message,
                Caller = GetServiceName()
            });


            if (kvalitetspoeng != null && kvalitetspoeng.FromUser != null)
            {
                var fileContent = await File.ReadAllLinesAsync("watchlist.txt");
                var isOnWatchlist = fileContent.Where(line =>
                    line.ToLower().Contains(kvalitetspoeng.FromUser.ToLower()))
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
