using HtmlAgilityPack;
using Echelon.Bot.Models;

namespace Echelon.Bot.Components
{
    public class VGComponent : IParserComponent<VGNewsPost>
    {
        private readonly IMessageWriter messageWriter;

        public VGComponent(IMessageWriter messageWriter)
        {
            this.messageWriter = messageWriter;
        }
        public VGNewsPost Execute(HtmlDocument document)
        {
            var title = document.Text;
                //.DocumentNode
                //.SelectNodes("//h2[contains(@itemprop, 'headline')]");
                

            throw new NotImplementedException();
        }
    }
}
