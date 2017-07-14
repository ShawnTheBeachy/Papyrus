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
    public class HtmlParser
    {
        private CssParser _cssParser = new CssParser();
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
            _cssParser.Styles.Clear();
            ConvertedBlocks.Clear();
            _currentParagraph = new Paragraph();

            if (!string.IsNullOrWhiteSpace(css))
                _cssParser.Parse(css);

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
            _cssParser.StyleElement(node, bold, style);

            foreach (var child in node.ChildNodes)
                bold.Inlines.SafeAdd(ParseNode(child, style));

            return bold;
        }

        void ParseImage(HtmlNode node, Style style)
        {
            var container = new InlineUIContainer();
            _cssParser.StyleElement(node, container, style);
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
            _cssParser.StyleElement(node, italic, style);

            foreach (var child in node.ChildNodes)
                italic.Inlines.SafeAdd(ParseNode(child, style));

            return italic;
        }

        Span ParseLink(HtmlNode node, Style style)
        {
            var span = new Span();
            _cssParser.StyleElement(node, span, new Style(style) { TextAlignment = TextAlignment.Center });

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
            _cssParser.StyleElement(node, _currentParagraph, style);

            foreach (var child in node.ChildNodes)
                _currentParagraph.Inlines.SafeAdd(ParseNode(child, style));
        }

        Span ParseSpan(HtmlNode node, Style style)
        {
            var span = new Span();
            _cssParser.StyleElement(node, span, style);

            foreach (var child in node.ChildNodes)
                span.Inlines.SafeAdd(ParseNode(child, style));

            return span;
        }

        Run ParseText(HtmlNode node, Style style)
        {
            if (node.InnerText.Replace(" ", string.Empty).Replace("\n", string.Empty).Length == 0)
                return null;

            var run = new Run { Text = Regex.Replace(node.InnerText, @"\s+", " ").Replace('\n', ' ') };
            _cssParser.StyleElement(node, run, style);
            return run;
        }

        #endregion ParseMethods
    }
}
