using HtmlAgilityPack;
using Echelon.Bot.Models;

namespace Echelon.Bot.Systems
{
    public class VGSystem : IParserSystem<VGNewsPost>
    {
        private readonly IMessageWriter messageWriter;

        public VGSystem(IMessageWriter messageWriter)
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
