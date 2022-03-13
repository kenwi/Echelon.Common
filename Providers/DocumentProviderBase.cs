using HtmlAgilityPack;
using System.Net;

namespace Echelon.Bot.Providers
{
    public class DocumentProviderBase : IDocumentProvider
    {
        protected readonly IMessageWriter messageWriter;
        protected string url = "";

        public DocumentProviderBase(IMessageWriter messageWriter)
        {
            ;
            this.messageWriter = messageWriter;
        }

        public virtual async Task<HtmlDocument> GetAsync()
        {          
            var web = new HtmlWeb();
#if DEBUG
            messageWriter.Write($"Requesting {url}");
#endif
            return await web.LoadFromWebAsync(url);
        }

        public Task<HtmlDocument> GetWithBrowserAsync()
        {
            throw new NotImplementedException();
        }

        public virtual string GetUrl() => url;
    }
}
