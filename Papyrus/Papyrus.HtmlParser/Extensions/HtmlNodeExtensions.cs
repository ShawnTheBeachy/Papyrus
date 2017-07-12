using HtmlAgilityPack;

namespace Papyrus.HtmlParser.Extensions
{
    public static class HtmlNodeExtensions
    {
        public static string GetClassName(this HtmlNode node)
        {
            if (node.HasAttributes && node.Attributes["class"] != null)
                return node.Attributes["class"].Value;
            else
                return null;
        }
    }
}
