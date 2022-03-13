namespace Echelon.Bot.Providers
{
    public class FreakPostProvider : DocumentProviderBase
    {
        public FreakPostProvider(IMessageWriter messageWriter)
            : base(messageWriter)
        {
            this.url = "https://freak.no/forum/search.php?do=getdaily";
        }

        public string? BuildUrl(string? target) => $"https://freak.no/forum/{target}";
    }
}
