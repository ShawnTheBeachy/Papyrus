using HtmlAgilityPack;
using Papyrus.UWP.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Papyrus.UWP.Parsers
{
	public class HtmlParser : IDisposable
	{
		private IList<Block> _convertedBlocks { get; } = new List<Block>();
		private Paragraph _currentParagraph = new Paragraph();
		private readonly StylesheetParser _stylesheetParser;
		private IList<ElementStyle> _styles = new List<ElementStyle>();

		public HtmlParser()
		{
			_stylesheetParser = new StylesheetParser();
		}

		public void Parse(string html, bool ignoreStyles = true)
		{
			_convertedBlocks.Clear();

			if (!ignoreStyles)
			{
				var styleRegex = new Regex(@"<style>(.*?)<\/style>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
				var styleMatch = styleRegex.Match(html);

				if (styleMatch.Success)
				{
					var css = styleMatch.Groups[styleMatch.Groups.Count - 1].Value;
					_styles = _stylesheetParser.ParseStyles(css).ToList();
				}
			}

			var body = GetBody(html);
			var doc = new HtmlDocument();
			doc.LoadHtml(body);
			var baseNode = doc.DocumentNode.Element("body") ?? doc.DocumentNode;

			foreach (var child in baseNode.ChildNodes)
				ParseNode(child);

			if (_currentParagraph != null)
			{
				_convertedBlocks.Add(_currentParagraph);
			}
			
			_currentParagraph = new Paragraph();
		}

		public void Dispose()
		{
			_convertedBlocks.Clear();
			_styles.Clear();
		}

		public IList<Block> GetBlocks()
		{
			return _convertedBlocks;
		}

		private string GetBody(string html)
		{
			var bodyIndex = html.IndexOf("<body");

			if (bodyIndex > -1)
				return html.Substring(bodyIndex);
			else
				return html;
		}

		private Bold ParseBold(HtmlNode node)
		{
			var bold = new Bold();
			_stylesheetParser.ApplyStyles(bold, _styles, node.Name, node.GetAttributeValue("class", ""), node.GetAttributeValue("id", ""));

			foreach (var child in node.ChildNodes)
			{
				bold.Inlines.SafeAdd(ParseNode(child));
			}

			return bold;
		}

		private void ParseImage(HtmlNode node)
		{
			var container = new InlineUIContainer();
			var source = node.GetAttributeValue("src", null);

			if (source == null)
				return;

			var base64Regex = new Regex(@"data:image\/.*;base64,(.*)", RegexOptions.IgnoreCase);
			var base64Match = base64Regex.Match(source);

			if (!base64Match.Success)
				return;

			var data = base64Match.Groups.OfType<Group>().ElementAt(1);
			var bytes = Convert.FromBase64String(data.Value);

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

			_convertedBlocks.Add(_currentParagraph);
			_currentParagraph = new Paragraph
			{
				TextAlignment = TextAlignment.Center
			};
			_currentParagraph.Inlines.Add(container);
			_currentParagraph.Inlines.Add(new LineBreak());
			_convertedBlocks.Add(_currentParagraph);
			_currentParagraph = new Paragraph();
		}

		private Italic ParseItalic(HtmlNode node)
		{
			var italic = new Italic();
			_stylesheetParser.ApplyStyles(italic, _styles, node.Name, node.GetAttributeValue("class", ""), node.GetAttributeValue("id", ""));

			foreach (var child in node.ChildNodes)
			{
				italic.Inlines.SafeAdd(ParseNode(child));
			}

			return italic;
		}

		private Span ParseLink(HtmlNode node)
		{
			var span = new Span();

			var url = node.Attributes["href"]?.Value;
			url = url == null || url.Contains("://") ? url : $"epub://{url}";

			var hyperlink = new Hyperlink
			{
				NavigateUri = url == null ? null : new Uri(url, UriKind.RelativeOrAbsolute)
			};

			foreach (var child in node.ChildNodes)
			{
				var el = ParseNode(child);

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

		private Inline ParseNode(HtmlNode node)
		{
			switch (node.Name)
			{
				case "#text":
					return ParseText(node);
				case "a":
					return ParseLink(node);
				case "b":
				case "strong":
					return ParseBold(node);
				case "br":
					return new LineBreak();
				case "i":
				case "em":
					return ParseItalic(node);
				case "img":
					ParseImage(node);
					return null;
				case "blockquote":
				case "center":
				case "h1":
				case "h2":
				case "h3":
				case "h4":
				case "h5":
				case "h6":
				case "p":
				case "div":
					ParseParagraph(node);
					return null;
				case "span":
					return ParseSpan(node);
				default:
					return null;
			}
		}

		private void ParseParagraph(HtmlNode node)
		{
			if (_currentParagraph.Inlines.Count > 0)
				_convertedBlocks.Add(_currentParagraph);

			_currentParagraph = new Paragraph();

			if (node.Name == "h1")
			{
				_currentParagraph.FontSize = 36;
				_currentParagraph.FontWeight = FontWeights.Bold;
			}

			else if (node.Name == "h2")
			{
				_currentParagraph.FontSize = 32;
				_currentParagraph.FontWeight = FontWeights.Bold;
			}

			else if (node.Name == "h3")
			{
				_currentParagraph.FontSize = 28;
				_currentParagraph.FontWeight = FontWeights.Bold;
			}

			else if (node.Name == "h4")
			{
				_currentParagraph.FontSize = 24;
				_currentParagraph.FontWeight = FontWeights.Bold;
			}

			else if (node.Name == "h5")
			{
				_currentParagraph.FontSize = 20;
				_currentParagraph.FontWeight = FontWeights.Bold;
			}

			else if (node.Name == "h6")
			{
				_currentParagraph.FontSize = 16;
				_currentParagraph.FontWeight = FontWeights.Bold;
			}

			else if (node.Name == "center")
			{
				_currentParagraph.TextAlignment = TextAlignment.Center;
			}

			else if (node.Name == "blockquote")
			{
				_currentParagraph.Foreground = new SolidColorBrush(Colors.Gray);
			}

			_stylesheetParser.ApplyStyles(_currentParagraph, _styles, node.Name, node.GetAttributeValue("class", ""), node.GetAttributeValue("id", ""));

			foreach (var child in node.ChildNodes)
			{
				_currentParagraph.Inlines.SafeAdd(ParseNode(child));
			}
		}

		private Span ParseSpan(HtmlNode node)
		{
			var span = new Span();
			_stylesheetParser.ApplyStyles(span, _styles, node.Name, node.GetAttributeValue("class", ""), node.GetAttributeValue("id", ""));

			foreach (var child in node.ChildNodes)
			{
				span.Inlines.SafeAdd(ParseNode(child));
			}

			return span;
		}

		private Run ParseText(HtmlNode node)
		{
			if (node.InnerText.Replace(" ", string.Empty).Replace("\n", string.Empty).Length == 0)
				return null;

			var run = new Run
			{
				Text = Regex.Replace(node.InnerText, @"\s+", " ").Replace("\n", " ")
			};
			_stylesheetParser.ApplyStyles(run, _styles, node.Name, node.GetAttributeValue("class", ""), node.GetAttributeValue("id", ""));
			return run;
		}
	}
}
