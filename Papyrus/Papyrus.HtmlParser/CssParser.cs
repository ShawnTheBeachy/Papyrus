using HtmlAgilityPack;
using Papyrus.HtmlParser.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Documents;

namespace Papyrus.HtmlParser
{
    internal class CssParser
    {
        public Dictionary<string, Style> Styles { get; } = new Dictionary<string, Style>();

        public void Parse(string css)
        {
            var classesRegex = new Regex(@"^(\.[^{]*)([^}]*)", RegexOptions.Multiline);
            var propertyRegex = new Regex(@"([a-z-]+):\s?([^;\r\n|\r|\n]+)");
            var classMatches = classesRegex.Matches(css).OfType<Match>();

            foreach (var match in classMatches)
            {
                var name = match.Groups.OfType<Group>().ElementAt(1).Value;
                var valueGroup = match.Groups.OfType<Group>().ElementAt(2);
                var propMatches = propertyRegex.Matches(match.Value).OfType<Match>();
                var props = new Dictionary<string, string>();

                foreach (var propMatch in propMatches)
                {
                    var groups = propMatch.Groups.OfType<Group>();
                    props.Add(groups.ElementAt(1).Value, groups.ElementAt(2).Value);
                }

                var style = CssHelpers.GetStyleFromCssProperties(props);
                Styles.Add(name.Trim(), style);
            }
        }

        public Style GetForTagName(string name)
        {
            var style = Styles.SafeGet(name);
            return style;
        }

        public Style GetForClassName(string className)
        {
            var style = Styles.SafeGet($".{className.Trim()}");
            return style;
        }

        public void StyleElement(HtmlNode node, Block element, Style baseStyle) =>
            element.ApplyStyle(GetStyleForNode(node, baseStyle));

        public void StyleElement(HtmlNode node, Inline element, Style baseStyle) =>
            element.ApplyStyle(GetStyleForNode(node, baseStyle));

        private Style GetStyleForNode(HtmlNode node, Style baseStyle)
        {
            var newStyle = new Style(baseStyle);
            newStyle.RebaseOn(GetForTagName(node.Name));                    // If a style for this HTML node type (e.g. "div", "a", "ul") exists, apply its styles.

            var classNames = node.GetClassName();

            if (!string.IsNullOrWhiteSpace(classNames))
            {
                var classSplits = classNames.Split(' ');                    // There could be multiple classes applied. Split on spaces.

                foreach (var className in classSplits)
                    newStyle.RebaseOn(GetForClassName(className));          // If a style for this className exists, apply it.
            }

            return newStyle;
        }
    }
}
