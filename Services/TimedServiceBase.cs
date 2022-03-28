using Echelon.Bot.Providers;
using Echelon.Bot.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Echelon.Bot.Services
{
    public abstract class TimedServiceBase<TProvider, TParserComponent, IMessage> :
        TimedServiceBase
        where TProvider : DocumentProviderBase
        where TParserComponent : IParserComponent<IMessage>
    {
        protected IMessage? message;

        protected TimedServiceBase(
            IServiceProvider serviceProvider, 
            IMessageWriter messageWriter) 
            : base(serviceProvider, messageWriter)
        {

        }

        public override async void DoWork(object? state)
        {
            messageWriter.Write(GetServiceName());

            var provider = serviceProvider.GetRequiredService<TProvider>();
            var document = await provider.GetAsync();
            var parser = serviceProvider.GetRequiredService<TParserComponent>();
            this.message = parser.Execute(document);
        }
    }

    public abstract class TimedServiceBase : IHostedService
    {
        private Timer? timer;
        protected readonly IServiceProvider serviceProvider;
        protected readonly IMessageWriter messageWriter;
        protected int updateInterval = 1;

        public TimedServiceBase(IServiceProvider serviceProvider, IMessageWriter messageWriter)
        {
            this.serviceProvider = serviceProvider;
            this.messageWriter = messageWriter;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(updateInterval));
            await Task.Delay(0, cancellationToken);
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(0, cancellationToken);
        }

        public abstract void DoWork(object? state);

        public virtual string GetServiceName() => GetType().Name;
    }
}



