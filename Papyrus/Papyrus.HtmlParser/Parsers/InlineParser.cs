using HtmlAgilityPack;
using System;

namespace Papyrus.HtmlParser.Parsers
{
    internal class InlineParser : IParser
    {
        public Action<HtmlNode> Parse;

        void IParser.Parse(HtmlNode node) =>
            Parse(node);
    }
}
