namespace Echelon.Bot.Providers
{
    public class LiveuamapPostProvider : DocumentProviderBase
    {
        public LiveuamapPostProvider(IMessageWriter messageWriter)
            : base(messageWriter)
        {

        }
        public override string GetUrl()
        {
            return this.url;
        }

        public void SetUrl(string url)
        {
            this.url = url;
        }

        public void SetTarget(string target)
        {
            messageWriter.Write($"Setting new target: {target}");
            this.url = $"https://{target}.liveuamap.com/";
        }
    }
}
