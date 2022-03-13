using HtmlAgilityPack;

namespace Echelon.Bot.Providers
{
    public interface IDocumentProvider
    {
        Task<HtmlDocument> GetAsync();
        Task<HtmlDocument> GetWithBrowserAsync();
        string GetUrl();
    }
}
