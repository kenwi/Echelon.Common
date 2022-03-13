namespace Echelon.Bot.Providers
{
    public class LiveuamapPopupProvider : DocumentProviderBase
    {
        public LiveuamapPopupProvider(IMessageWriter messageWriter)
            : base(messageWriter)
        {
            this.url = "";
        }
    }
}
