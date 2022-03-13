using Echelon.Bot.Models;
using Echelon.Bot.Providers;
using Echelon.Bot.Systems;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Echelon.Bot.Services
{
    public class FreakKvalitetsPoengService
        : TimedServiceBase<FreakKvalitetsPoengProvider, FreakKvalitetsPoengSystem, FreakKvalitetsPoeng>
    {
        ulong channelId = 0;
        public FreakKvalitetsPoengService(
            IConfigurationRoot configuration,
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter)
            : base(serviceProvider, messageWriter)
        {
            this.updateInterval = 6;
            this.channelId = configuration.GetValue<ulong>("kvalitetspoengChannelId");
        }

        public override async void DoWork(object? state)
        {
            await Task.Run(() => base.DoWork(state));
            if (this.message == null)
                return;

            var kvalitetspoeng = this.message as FreakKvalitetsPoeng;
            var message = $"Kvalitetspoeng til {kvalitetspoeng.ToUser} fra {kvalitetspoeng.FromUser} i forumet {kvalitetspoeng.Forum} {kvalitetspoeng.Link}";

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
