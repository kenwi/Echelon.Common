using HtmlAgilityPack;

namespace Echelon.Bot.Systems
{
    public class LiveuamapPopupSystem : IParserSystem<string?>
    {
        private readonly IMessageWriter messageWriter;
        public LiveuamapPopupSystem(IMessageWriter messageWriter)
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
