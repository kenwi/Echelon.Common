using HtmlAgilityPack;
using Echelon.Bot.Models;

namespace Echelon.Bot.Components
{
    public class NRKComponent : IParserComponent<NRKNewsPost>
    {
        private readonly IMessageWriter messageWriter;

        public NRKComponent(IMessageWriter messageWriter)
        {
            this.messageWriter = messageWriter;
        }

        public NRKNewsPost Execute(HtmlDocument document)
        {
            var node = document.DocumentNode
                .SelectNodes("//div[contains(@class, 'bulletin-text')]")
                ?.Take(1)
                ?.SingleOrDefault();

            return new NRKNewsPost
            {
                Title = node?.ChildNodes["h2"]
                    ?.InnerText,
                Link = node?.ChildNodes["time"]
                    ?.ChildNodes["a"]
                    ?.Attributes["href"]
                    ?.Value
            };
        }
    }
}
