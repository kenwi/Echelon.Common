using HtmlAgilityPack;

namespace Echelon.Bot.Systems
{
    public interface IParserSystem<T>
    {
        public T Execute(HtmlDocument document);
    }
}
