namespace Echelon.Bot.Providers
{
    public class VGProvider : DocumentProviderBase
    {
        public VGProvider(IMessageWriter messageWriter)
            : base(messageWriter)
        {
            url = "https://direkte.vg.no/krig-i-ukraina/news";
        }
    }
}
