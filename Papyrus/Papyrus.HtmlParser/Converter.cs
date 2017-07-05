using HtmlAgilityPack;
using Papyrus.HtmlParser.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace Papyrus.HtmlParser
{
    public class Converter
    {
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
        
        public async Task ConvertAsync(StorageFile file)
        {
            Convert(await FileIO.ReadTextAsync(file));
        }

        public void Convert(string html)
        {
            ConvertedBlocks.Clear();
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

        Hyperlink ParseLink(HtmlNode node, Style style)
        {
            var url = node.Attributes["href"]?.Value;
            url = url == null || url.Contains("://") ? url : $"epub://{url}";

            var hyperlink = new Hyperlink
            {
                NavigateUri = url == null ? null : new Uri(url, UriKind.RelativeOrAbsolute)
            };
            hyperlink.ApplyStyle(style);

            foreach (var child in node.ChildNodes)
                hyperlink.Inlines.SafeAdd(ParseNode(child, style));

            return hyperlink;
        }

        void ParseParagraph(HtmlNode node, Style style)
        {
            if (_currentParagraph.Inlines.Count > 0)
                ConvertedBlocks.Add(_currentParagraph);

            _currentParagraph = new Paragraph();

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

        public double FontSize { get; set; } = 16;
        public FontStyle FontStyle { get; set; } = FontStyle.Normal;
        public FontWeight FontWeight { get; set; } = FontWeights.Normal;
        public Color Foreground { get; set; } = Colors.Black;
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;
    }
}
