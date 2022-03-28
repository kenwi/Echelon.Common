using HtmlAgilityPack;

namespace Echelon.Bot.Providers
{
    public class FreakUserIdProvider : DocumentProviderBase
    {
        private string? username;

        public FreakUserIdProvider(IMessageWriter messageWriter)
            : base(messageWriter)
        {

        }

        public void SetUsername(string username) => this.username = username;

        public override async Task<HtmlDocument> GetAsync()
        {
            this.url = $"https://freak.no/@{username}";
            return await base.GetAsync();
        }
    }
}
