using HtmlAgilityPack;
using Papyrus.HtmlParser.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;

namespace Papyrus.HtmlParser
{
	public class Converter
    {
        private Dictionary<string, Style> _cssStyles = new Dictionary<string, Style>();
        private Dictionary<string, double> _headerFontSizes = new Dictionary<string, double>
        {
            ["h1"] = 36,
            ["h2"] = 32,
            ["h3"] = 28,
            ["h4"] = 24,
            ["h5"] = 20,
            ["h6"] = 16
        };
        private Paragraph _currentParagraph = new Paragraph();
        public ObservableCollection<Block> ConvertedBlocks { get; set; } = new ObservableCollection<Block>();
        
        public async Task ConvertAsync(StorageFile file, string css = null)
        {
            Convert(await FileIO.ReadTextAsync(file), css);
        }

        public void Convert(string html, string css = null)
        {
            _cssStyles.Clear();
            ConvertedBlocks.Clear();

            if (!string.IsNullOrWhiteSpace(css))
                ParseCss(css);

            var bodyIndex = html.IndexOf("<body");

            if (bodyIndex > -1)
                html = html.Substring(bodyIndex);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var baseNode = doc.DocumentNode.Element("body") ?? doc.DocumentNode;

            var s = new Style();

            foreach (var child in baseNode.ChildNodes)
                ParseNode(child, s);

            if (_currentParagraph != null)
                ConvertedBlocks.Add(_currentParagraph);
        }

        private void ParseCss(string css)
        {
            var classesRegex = new Regex(@"^(\.[^{]*)([^}]*)", RegexOptions.Multiline);
            var propertyRegex = new Regex(@"([a-z-]+):\s?([^;\r\n|\r|\n]+)");
            var classMatches = classesRegex.Matches(css).OfType<Match>();

            foreach (var match in classMatches)
            {
                var name = match.Groups.OfType<Group>().ElementAt(1).Value.Trim().TrimStart('.');
                var valueGroup = match.Groups.OfType<Group>().ElementAt(2);
                var propMatches = propertyRegex.Matches(match.Value).OfType<Match>();
                var props = new Dictionary<string, string>();

                foreach (var propMatch in propMatches)
                {
                    var groups = propMatch.Groups.OfType<Group>();
                    props.Add(groups.ElementAt(1).Value, groups.ElementAt(2).Value);
                }

                var style = CssHelpers.GetStyleFromCssProperties(props);
                _cssStyles.Add(name, style);
            }
        }

        #region ParseMethods

        Inline ParseNode(HtmlNode node, Style style)
        {
            switch (node.Name)
            {
                case "#text":
                    return ParseText(node, style);
                case "a":
                    return ParseLink(node, style);
                case "b":
                case "strong":
                    return ParseBold(node, style);
                case "blockquote":
                    var blockquoteStyle = new Style(style) { Foreground = Colors.Gray };
                    ParseParagraph(node, blockquoteStyle);
                    return null;
                case "br":
                    return new LineBreak();
                case "center":
                    var centerStyle = new Style(style) { TextAlignment = TextAlignment.Center };
                    ParseParagraph(node, centerStyle);
                    return null;
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    var headerStyle = new Style(style)
                    {
                        FontSize = _headerFontSizes[node.Name],
                        FontWeight = FontWeights.Bold
                    };
                    ParseParagraph(node, headerStyle);
                    return null;
                case "i":
                case "em":
                    return ParseItalic(node, style);
                case "img":
                    ParseImage(node, style);
                    return null;
                case "p":
                case "div":
                    ParseParagraph(node, style);
                    return null;
                case "span":
                    return ParseSpan(node, style);
                default:
                    return null;
            }
        }
        
        Bold ParseBold(HtmlNode node, Style style)
        {
            var bold = new Bold();
            bold.ApplyStyle(style);
            var boldStyle = new Style(style)
            {
                FontWeight = FontWeights.Bold
            };

            foreach (var child in node.ChildNodes)
                bold.Inlines.SafeAdd(ParseNode(child, boldStyle));

            return bold;
        }

        void ParseImage(HtmlNode node, Style style)
        {
            var container = new InlineUIContainer();
            container.ApplyStyle(style);
            var source = node.Attributes["src"]?.Value;

            if (source == null)
                return;

            var base64Regex = new Regex(@"data:image\/.*;base64,(.*)", RegexOptions.IgnoreCase);
            var base64Match = base64Regex.Match(source).Groups.OfType<Group>().ElementAt(1);

            if (base64Match == null)
                return;

            var bytes = System.Convert.FromBase64String(base64Match.Value);

            using (var ms = new InMemoryRandomAccessStream())
            {
                using (var writer = new DataWriter(ms.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(bytes);
                    writer.StoreAsync().GetResults();
                }

                var bitmap = new BitmapImage();
                bitmap.SetSource(ms);
                var image = new Image
                {
                    MaxWidth = 300,
                    Source = bitmap
                };
                container.Child = image;
            }

            ConvertedBlocks.Add(_currentParagraph);
            _currentParagraph = new Paragraph
            {
                TextAlignment = TextAlignment.Center
            };
            _currentParagraph.Inlines.Add(container);
            _currentParagraph.Inlines.Add(new LineBreak());
            ConvertedBlocks.Add(_currentParagraph);
            _currentParagraph = new Paragraph();
        }

        Italic ParseItalic(HtmlNode node, Style style)
        {
            var italic = new Italic();
            italic.ApplyStyle(style);
            var italicStyle = new Style(style)
            {
                FontStyle = FontStyle.Italic
            };

            foreach (var child in node.ChildNodes)
                italic.Inlines.SafeAdd(ParseNode(child, italicStyle));

            return italic;
        }

        Span ParseLink(HtmlNode node, Style style)
        {
            var span = new Span();
            span.ApplyStyle(new Style(style) { TextAlignment = TextAlignment.Center });

            var url = node.Attributes["href"]?.Value;
            url = url == null || url.Contains("://") ? url : $"epub://{url}";

            var hyperlink = new Hyperlink
            {
                NavigateUri = url == null ? null : new Uri(url, UriKind.RelativeOrAbsolute)
            };

            foreach (var child in node.ChildNodes)
            {
                var el = ParseNode(child, style);

                if (!hyperlink.Inlines.SafeAdd(el) && el is LineBreak lineBreak)
                {
                    span.Inlines.Add(hyperlink);
                    span.Inlines.Add(el);
                    hyperlink = new Hyperlink
                    {
                        NavigateUri = url == null ? null : new Uri(url, UriKind.RelativeOrAbsolute),
                    };
                }
            }

            span.Inlines.Add(hyperlink);

            return span;
        }

        void ParseParagraph(HtmlNode node, Style style)
        {
            if (_currentParagraph.Inlines.Count > 0)
                ConvertedBlocks.Add(_currentParagraph);

            _currentParagraph = new Paragraph();

            var className = node.GetClassName();

            if (!string.IsNullOrEmpty(className))
            {
                var cssStyle = _cssStyles.SafeGet(className);

                if (cssStyle != default(Style))
                {
                    var newStyle = new Style(style);
                    newStyle.RebaseOn(cssStyle);
                    _currentParagraph.ApplyStyle(newStyle);
                }
            }

            foreach (var child in node.ChildNodes)
                _currentParagraph.Inlines.SafeAdd(ParseNode(child, style));
        }

        Span ParseSpan(HtmlNode node, Style style)
        {
            var span = new Span();
            span.ApplyStyle(style);

            foreach (var child in node.ChildNodes)
                span.Inlines.SafeAdd(ParseNode(child, style));

            return span;
        }

        Run ParseText(HtmlNode node, Style style)
        {
            if (node.InnerText.Replace(" ", string.Empty).Replace("\n", string.Empty).Length == 0)
                return null;

            var run = new Run { Text = Regex.Replace(node.InnerText, @"\s+", " ").Replace('\n', ' ') };
            run.ApplyStyle(style);
            return run;
        }

        #endregion ParseMethods
    }

    public class Style
    {
        public Style() { }

        public Style(Style style)
        {
            FontSize = style.FontSize;
            FontStyle = style.FontStyle;
            FontWeight = style.FontWeight;
            Foreground = style.Foreground;
            TextAlignment = style.TextAlignment;
        }

        public void RebaseOn(Style style)
        {
            FontSize = style.FontSize ?? FontSize;
            FontStyle = style.FontStyle ?? FontStyle;
            FontWeight = style.FontWeight ?? FontWeight;
            Foreground = style.Foreground ?? Foreground;
            TextAlignment = style.TextAlignment ?? TextAlignment;
        }

        public double? FontSize { get; set; } = 16;
        public FontStyle? FontStyle { get; set; } = Windows.UI.Text.FontStyle.Normal;
        public FontWeight? FontWeight { get; set; } = FontWeights.Normal;
        public Color? Foreground { get; set; } = Colors.Black;
        public TextAlignment? TextAlignment { get; set; } = Windows.UI.Xaml.TextAlignment.Left;
    }
}
