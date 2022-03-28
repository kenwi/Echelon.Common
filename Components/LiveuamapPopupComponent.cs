using HtmlAgilityPack;

namespace Echelon.Bot.Components
{
    public class LiveuamapPopupComponent : IParserComponent<string?>
    {
        private readonly IMessageWriter messageWriter;
        public LiveuamapPopupComponent(IMessageWriter messageWriter)
        {
            this.messageWriter = messageWriter;
        }

        public string? Execute(HtmlDocument document)
        {
            return document?.DocumentNode
                ?.SelectNodes("//div[contains(@class, 'head_popup')]")
                ?.Take(1)
                ?.SingleOrDefault()
                ?.FirstChild
                ?.Attributes["href"]
                ?.Value; ;
        }
    }
}
