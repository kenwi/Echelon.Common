namespace Echelon.Bot.Providers
{
    public class NRKProvider : DocumentProviderBase
    {
        public NRKProvider(IMessageWriter messageWriter)
            : base(messageWriter)
        {
            url = "https://www.nrk.no/nyheter/ukraina-1.11480927";
        }
    }
}
