using HtmlAgilityPack;
using Echelon.Bot.Models;
using Echelon.Bot.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Echelon.Bot.Components
{
    public class FreakPostComponent : IParserComponent<FreakPost>
    {
        private readonly IServiceProvider provider;
        private readonly IMessageWriter messageWriter;
        private readonly string stringReplacement;

        public FreakPostComponent(
            IConfigurationRoot configuration,
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter)
        {
            this.provider = serviceProvider;
            this.messageWriter = messageWriter;
            this.stringReplacement = configuration.GetValue<string>("stringReplacement");
        }
        public FreakPost Execute(HtmlDocument document)
        {
            var post = document?.DocumentNode.SelectSingleNode("//a[contains(@id, 'thread_title')]");
            var title = post?.InnerText;
            post = document?.DocumentNode.SelectSingleNode("//a[contains(@class, 'lastpost-arrow')]");
            var link = post?.Attributes["href"]
                .Value
                .Replace(stringReplacement, "");
            post = document?.DocumentNode.SelectSingleNode("//a[contains(@href, 'member.php')]");
            var user = post?.InnerText;

            var freakPostProvider = provider.GetService<FreakPostProvider>();
            return new FreakPost
            {
                Title = title,
                Link = freakPostProvider?.BuildUrl(link),
                User = user
            };
        }
    }
}
