using HtmlAgilityPack;
using Papyrus.HtmlParser.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace Papyrus.HtmlParser
{
    public class Converter
    {
        public ObservableCollection<Block> ConvertedBlocks { get; set; } = new ObservableCollection<Block>();

        private Dictionary<string, IParser> ParserMap => new Dictionary<string, IParser>
        {
            ["#text"] = TextParser,
            ["a"] = LinkParser,
            ["b"] = BoldParser,
            ["blockquote"] = BlockquoteParser,
            ["br"] = LineBreakParser,
            ["center"] = CenterParser,
            ["div"] = ParagraphParser,
            ["em"] = ItalicParser,
            ["h1"] = HeaderOneParser,
            ["h2"] = HeaderTwoParser,
            ["h3"] = HeaderThreeParser,
            ["h4"] = HeaderFourParser,
            ["h5"] = HeaderFiveParser,
            ["h6"] = HeaderSixParser,
            ["i"] = ItalicParser,
            ["p"] = ParagraphParser,
            ["span"] = SpanParser,
            ["strong"] = BoldParser
        };

        private void ParseChildren(HtmlNode node, TextElement parent)
        {
            if (!ParserMap.ContainsKey(node.Name))
                return;

            var parser = ParserMap[node.Name];
            parser.Parse(node);
        }

        public async Task ConvertAsync(StorageFile file)
        {
            Convert(await FileIO.ReadTextAsync(file));
        }

        public void Convert(string html)
        {
            var bodyIndex = html.IndexOf("<body");
            html = html.Substring(bodyIndex);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var bodyNode = doc.DocumentNode.Element("body");

            foreach (var child in bodyNode.ChildNodes)
                ParseNode(child);
        }

        #region Parsers

        private BlockParser BlockquoteParser => new BlockParser { Parse = ParseBlockquote };
        private InlineParser BoldParser => new InlineParser { Parse = ParseBold };
        private BlockParser CenterParser => new BlockParser { Parse = ParseCenter };
        private BlockParser HeaderOneParser => new BlockParser { Parse = ParseHeaderOne };
        private BlockParser HeaderTwoParser => new BlockParser { Parse = ParseHeaderTwo };
        private BlockParser HeaderThreeParser => new BlockParser { Parse = ParseHeaderThree };
        private BlockParser HeaderFourParser => new BlockParser { Parse = ParseHeaderFour };
        private BlockParser HeaderFiveParser => new BlockParser { Parse = ParseHeaderFive };
        private BlockParser HeaderSixParser => new BlockParser { Parse = ParseHeaderSix };
        private InlineParser ItalicParser => new InlineParser { Parse = ParseItalic };
        private InlineParser LineBreakParser => new InlineParser { Parse = ParseLineBreak };
        private InlineParser LinkParser => new InlineParser { Parse = ParseLink };
        private BlockParser ParagraphParser => new BlockParser { Parse = ParseParagraph };
        private InlineParser SpanParser => new InlineParser { Parse = ParseSpan };
        private InlineParser TextParser => new InlineParser { Parse = ParseText };

        #endregion Parsers

        #region ParseMethods

        private void ParseBlockquote(HtmlNode node)
        {
            var paragraphs = ParseParagraph(node);

            foreach (var paragraph in paragraphs)
            {
                paragraph.Foreground = new SolidColorBrush(Colors.Gray);
                paragraph.Margin = new Thickness(12, 0, 0, 0);

                ConvertedBlocks.Add(paragraph);
            }
        }

        private void ParseBold(HtmlNode node)
        {
            var bold = new Bold();

            foreach (var child in node.ChildNodes)
            {
                bold.Inlines.Add(ParseNode(child) as Inline);
            }

            return bold;
        }

        private IEnumerable<Paragraph> ParseCenter(HtmlNode node)
        {
            var paragraphs = ParseParagraph(node);

            foreach (var paragraph in paragraphs)
                paragraph.TextAlignment = TextAlignment.Center;

            return paragraphs;
        }

        private IEnumerable<Paragraph> ParseHeaderOne(HtmlNode node)
        {
            var paragraphs = ParseParagraph(node);

            foreach (var paragraph in paragraphs)
            {
                paragraph.FontSize = 36;
                paragraph.FontWeight = FontWeights.SemiBold;
            }

            return paragraphs;
        }

        private IEnumerable<Paragraph> ParseHeaderTwo(HtmlNode node)
        {
            var paragraphs = ParseParagraph(node);

            foreach (var paragraph in paragraphs)
            {
                paragraph.FontSize = 32;
                paragraph.FontWeight = FontWeights.SemiBold;
            }

            return paragraphs;
        }

        private Paragraph ParseHeaderThree(HtmlNode node)
        {
            var paragraph = ParseParagraph(node).FirstOrDefault();
            paragraph.FontSize = 28;
            paragraph.FontWeight = FontWeights.SemiBold;
            return paragraph;
        }

        private Paragraph ParseHeaderFour(HtmlNode node)
        {
            var paragraph = ParseParagraph(node).FirstOrDefault();
            paragraph.FontSize = 24;
            paragraph.FontWeight = FontWeights.SemiBold;
            return paragraph;
        }

        private Paragraph ParseHeaderFive(HtmlNode node)
        {
            var paragraph = ParseParagraph(node).FirstOrDefault();
            paragraph.FontSize = 20;
            paragraph.FontWeight = FontWeights.SemiBold;
            return paragraph;
        }

        private Paragraph ParseHeaderSix(HtmlNode node)
        {
            var paragraph = ParseParagraph(node).FirstOrDefault();
            paragraph.FontSize = 16;
            paragraph.FontWeight = FontWeights.SemiBold;
            return paragraph;
        }

        private Italic ParseItalic(HtmlNode node)
        {
            var italic = new Italic();

            foreach (var child in node.ChildNodes)
                italic.Inlines.Add(ParseNode(child) as Inline);

            return italic;
        }

        private LineBreak ParseLineBreak(HtmlNode node)
        {
            var lineBreak = new LineBreak();
            return lineBreak;
        }

        private Hyperlink ParseLink(HtmlNode node)
        {
            var url = node.Attributes["href"]?.Value;
            var hyperlink = new Hyperlink
            {
                NavigateUri = url == null ? null : new Uri(url)
            };

            foreach (var child in node.ChildNodes)
                hyperlink.Inlines.Add(ParseNode(child) as Inline);

            return hyperlink;
        }

        private IEnumerable<Paragraph> ParseParagraph(HtmlNode node)
        {
            var paragraphs = new List<Paragraph>();
            var paragraph = new Paragraph();

            foreach (var child in node.ChildNodes)
            {
                var element = ParseNode(child);

                if (element is Block)
                {
                    paragraphs.Add(paragraph);
                    paragraph = element as Paragraph;
                }

                else
                    paragraph.Inlines.Add(element as Inline);
            }

            return paragraphs;
        }

        private Span ParseSpan(HtmlNode node)
        {
            var span = new Span();

            foreach (var child in node.ChildNodes)
                span.Inlines.Add(ParseNode(child) as Inline);

            return span;
        }

        private Run ParseText(HtmlNode node)
        {
            return new Run { Text = node.InnerText };
        }

        #endregion ParseMethods
    }
}
