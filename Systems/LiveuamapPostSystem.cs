using HtmlAgilityPack;
using Echelon.Bot.Models;

namespace Echelon.Bot.Systems
{
    public class LiveuamapPostSystem : IParserSystem<LiveuamapPost> //, IParserSystem<string>
    {
        private readonly IMessageWriter messageWriter;
        public LiveuamapPostSystem(IMessageWriter messageWriter)
        {
            this.messageWriter = messageWriter;
        }

        public LiveuamapPost Execute(HtmlDocument document)
        {
            var node = document?.DocumentNode
                ?.SelectNodes("//div[contains(@id, 'post')]")
                ?.Take(1)
                ?.SingleOrDefault();
            return new LiveuamapPost
            {
                Title = node?.ChildNodes
                    ?.Skip(1)
                    ?.Take(1)
                    ?.SingleOrDefault()
                    ?.InnerText,
                Link = node?.Attributes["data-link"]
                    ?.Value
            };
        }
    }
}
