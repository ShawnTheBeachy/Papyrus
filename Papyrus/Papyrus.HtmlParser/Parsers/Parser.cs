using HtmlAgilityPack;

namespace Papyrus.HtmlParser.Parsers
{
    internal interface IParser
    {
        void Parse(HtmlNode node);
    }
}
