using HtmlAgilityPack;

namespace Echelon.Bot.Components
{
    public interface IParserComponent<T>
    {
        public T Execute(HtmlDocument document);
    }
}
