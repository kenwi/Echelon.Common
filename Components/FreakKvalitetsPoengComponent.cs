using HtmlAgilityPack;
using Echelon.Bot.Models;

namespace Echelon.Bot.Components
{
    public class FreakKvalitetsPoengComponent : IParserComponent<FreakKvalitetsPoeng>
    {
        private readonly IMessageWriter messageWriter;

        public FreakKvalitetsPoengComponent(IMessageWriter messageWriter)
        {
            this.messageWriter = messageWriter;
        }

        public FreakKvalitetsPoeng Execute(HtmlDocument document)
        {
            return new FreakKvalitetsPoeng
            {
                ToUser = document.DocumentNode
                    ?.SelectNodes("//span[string-length(@data-username) > 0]")
                    ?.FirstOrDefault()
                    ?.InnerText,
                FromUser = document.DocumentNode
                    ?.SelectNodes("//span[string-length(@data-username) > 0]")
                    ?.Skip(1)
                    ?.FirstOrDefault()
                    ?.InnerText,
                Title = document.DocumentNode
                    ?.SelectNodes("//a[contains(@href, 'showthread')]")
                    ?.FirstOrDefault()
                    ?.InnerText,
                Forum = document.DocumentNode
                    ?.SelectNodes("//a[contains(@href, 'forumdisplay')]")
                    ?.FirstOrDefault()
                    ?.InnerText,
                Link = document.DocumentNode
                    ?.SelectNodes("//a[contains(@href, '#post')]")
                    ?.FirstOrDefault()
                    ?.Attributes["href"]
                    ?.Value
            };
        }
    }
}
