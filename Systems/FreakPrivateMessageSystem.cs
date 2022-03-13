using HtmlAgilityPack;
using Echelon.Bot.Models;

namespace Echelon.Bot.Systems
{
    public class FreakPrivateMessageSystem : IParserSystem<IEnumerable<FreakPrivateMessage>>
    {
        private readonly IServiceProvider provider;
        private readonly IMessageWriter messageWriter;
        public FreakPrivateMessageSystem(
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter)
        {
            this.provider = serviceProvider;
            this.messageWriter = messageWriter;
        }
        public IEnumerable<FreakPrivateMessage> Execute(HtmlDocument document)
        {
            var allMessages = document.DocumentNode
                ?.SelectNodes("//td[contains(@class, 'alt1Active')]");

            if (allMessages == null)
                return new List<FreakPrivateMessage>();

            var skip = 0;
            var messages = new List<FreakPrivateMessage>();

            foreach(var message in allMessages)
            {
                var title = message
                    ?.SelectNodes("//a[contains(@href, 'do=showpm')]")
                    ?.Skip(skip)
                    ?.First()
                    ?.InnerText;

                var sender = document.DocumentNode
                    ?.SelectNodes("//span[contains(@onclick, 'member.php')]")
                    ?.Skip(skip)
                    ?.First()
                    ?.InnerText;
                messages.Add(new FreakPrivateMessage
                {
                    Title = title,
                    Sender = sender
                });
                skip++;
            }
            return messages;
        }
    }
}
