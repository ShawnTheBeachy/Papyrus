using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Papyrus
{
	public static class EBookExtensions
	{
		public static string GetContentLocation(this EBook eBook)
		{
			var containerPath = Path.Combine(eBook.BaseDirectory, "META-INF", "container.xml");
			var xml = File.ReadAllText(containerPath);
			var doc = XDocument.Parse(xml);
			var ns = doc.Root.GetDefaultNamespace();
			var node = doc.Element(ns + "container").Element(ns + "rootfiles").Element(ns + "rootfile");

			if (node.Attribute("media-type")?.Value != "application/oebps-package+xml")
				throw new Exception($"Invalid media type on rootfile node. Unexpected value \"{node.Attribute("media-type")?.Value}\"");
			
			var path = Path.GetFullPath(Path.Combine(eBook.BaseDirectory, node.Attribute("full-path")?.Value));
			return path;
		}

		public static string GetContents(this EBook eBook, NavPoint navPoint, bool embedImages = false, bool embedStyles = false)
		{
			var manifestItem = eBook.Manifest.SingleOrDefault(item => Path.GetFileName(item.Value.ContentLocation) == Path.GetFileName(navPoint.ContentPath));
			return eBook.GetContents(manifestItem.Value, embedImages, embedStyles);
		}

		public static string GetContents(this EBook eBook, SpineItem item, bool embedImages = false, bool embedStyles = false)
		{
			var manifestItem = eBook.Manifest[item.IdRef];
			return GetContents(eBook, manifestItem, embedImages, embedStyles);
		}

		public static string GetContents(this EBook eBook, ManifestItem manifestItem, bool embedImages = false, bool embedStyles = false)
		{
			var contents = File.ReadAllText(manifestItem.ContentLocation);
			contents = WebUtility.HtmlDecode(contents);

			if (embedImages)
			{
				var imageMatches = new Regex(@"<img.*/>", RegexOptions.IgnoreCase).Matches(contents).OfType<Match>().ToList();

				foreach (var match in imageMatches)
				{
					var node = HtmlNode.CreateNode(match.Value);
					var source = node.Attributes["src"].Value;

					if (source.StartsWith("data:"))
						continue;

					var imagePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(manifestItem.ContentLocation), source));
					var buffer = File.ReadAllBytes(imagePath);
					var base64 = Convert.ToBase64String(buffer);
					node.Attributes["src"].Value = $"data:image/{new FileInfo(imagePath).Extension};base64,{base64}";
					contents = contents.Replace(match.Value, node.OuterHtml);
				}
			}

			if (embedStyles)
			{
				var styleRegex = new Regex(@"<style>(.*?)<\/style>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
				var styleMatch = styleRegex.Match(contents);
				var styleBuilder = new StringBuilder();

				if (styleMatch.Success)
				{
					styleBuilder.Append(styleMatch.Groups[styleMatch.Groups.Count - 1]);
				}

				var stylesheetRegex = new Regex(@"<link.*?rel=""stylesheet"".*?>", RegexOptions.IgnoreCase);
				var stylesheetMatches = stylesheetRegex.Matches(contents).OfType<Match>().ToList();

				foreach (var match in stylesheetMatches)
				{
					var node = HtmlNode.CreateNode(match.Value);
					var location = node.Attributes["href"].Value;
					var file = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(manifestItem.ContentLocation), location));

					if (!File.Exists(file))
					{
						continue;
					}

					var css = File.ReadAllText(file);
					styleBuilder.Append(css);
					contents = contents.Replace(match.Value, string.Empty);
				}

				var styleTag = $"<style>{styleBuilder.ToString()}</style>";

				if (styleMatch.Success)
				{
					contents = contents.Replace(styleMatch.Value, styleTag);
				}

				else
				{
					var endOfHead = contents.IndexOf("</head>");
					contents = contents.Insert(endOfHead, styleTag);
				}
			}

			return contents;
		}

		public static string GetCoverLocation(this EBook eBook)
		{
			if (!eBook.Manifest.ContainsKey("cover"))
				return null;

			var relativeLocation = eBook.Manifest["cover"].ContentLocation;
			var coverLocation = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(eBook.ContentLocation), relativeLocation));
			return coverLocation;
		}

		public static IEnumerable<ManifestItem> GetManifest(this EBook eBook)
		{
			var contentXml = File.ReadAllText(eBook.ContentLocation);
			var doc = XDocument.Parse(contentXml);
			var ns = doc.Root.GetDefaultNamespace();
			var manifestNode = doc.Element(ns + "package").Element(ns + "manifest");
			var itemNodes = manifestNode.Elements(ns + "item").ToList();

			foreach (var node in itemNodes)
			{
				yield return new ManifestItem
				{
					ContentLocation = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(eBook.ContentLocation), node.Attribute("href").Value)),
					Id = node.Attribute("id").Value,
					MediaType = node.Attribute("media-type").Value
				};
			}
		}

		public static Metadata GetMetadata(this EBook eBook)
		{
			var contentXml = File.ReadAllText(eBook.ContentLocation);
			var doc = XDocument.Parse(contentXml);
			var ns = doc.Root.GetDefaultNamespace();
			var metadataNode = doc.Element(ns + "package").Element(ns + "metadata");
			var dcNamespace = metadataNode.GetNamespaceOfPrefix("dc");

			string GetValue(string node)
			{
				return metadataNode.Element(dcNamespace + node)?.Value;
			}

			var metadata = new Metadata
			{
				AlternativeTitle = GetValue("alternative"),
				Audience = GetValue("audience"),
				Available = GetValue("available") == null ? default(DateTime) : DateTime.Parse(GetValue("available")),
				Contributor = GetValue("contributor"),
				Created = GetValue("created") == null ? default(DateTime) : DateTime.Parse(GetValue("created")),
				Creator = GetValue("creator"),
				Description = GetValue("description"),
				Language = GetValue("language"),
				Title = GetValue("title")
			};

			foreach (var node in metadataNode.Elements(dcNamespace + "date").ToList())
			{
				// TODO: Parse dates. May be a parseable date or just a year.
			}

			return metadata;
		}

		public static Spine GetSpine(this EBook eBook)
		{
			var spine = new Spine();
			var contentXml = File.ReadAllText(eBook.ContentLocation);
			var doc = XDocument.Parse(contentXml);
			var ns = doc.Root.GetDefaultNamespace();
			var spineNode = doc.Element(ns + "package").Element(ns + "spine");
			spine.Toc = spineNode.Attribute("toc").Value;
			var itemNodes = spineNode.Elements(ns + "itemref").ToList();

			foreach (var node in itemNodes)
			{
				spine.Add(new SpineItem
				{
					IdRef = node.Attribute("idref").Value
				});
			}

			return spine;
		}

		public static TableOfContents GetTableOfContents(this EBook eBook)
		{
			if (eBook.Spine == null)
				eBook.Spine = eBook.GetSpine();
			
			var tocXml = File.ReadAllText(eBook.Manifest[eBook.Spine.Toc].ContentLocation);
			var doc = XDocument.Parse(tocXml);
			var ns = doc.Root.GetDefaultNamespace();
			var ncxNode = doc.Element(ns + "ncx");
			var tableOfContents = new TableOfContents
			{
				Title = ncxNode.Element(ns + "docTitle").Element(ns + "text").Value
			};
			var navMapNode = ncxNode.Element(ns + "navMap");

			IEnumerable<NavPoint> ParseNavPoints(XElement node, int level)
			{
				var navPoints = new List<NavPoint>();
				var navPointNodes = node.Elements(ns + "navPoint").ToList();

				foreach (var navPointNode in navPointNodes)
				{
					var filePath = navPointNode.Element(ns + "content").Attribute("src").Value;
					var pathBuilder = new StringBuilder();

					foreach (var c in filePath)
					{
						if (c == '#' && pathBuilder.ToString().EndsWith(".html"))
							break;

						pathBuilder.Append(c);
					}

					var navPoint = new NavPoint
					{
						ContentPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(eBook.Manifest[eBook.Spine.Toc].ContentLocation), pathBuilder.ToString())),
						Id = navPointNode.Attribute("id")?.Value,
						Level = level,
						PlayOrder = int.Parse(navPointNode.Attribute("playOrder")?.Value),
						Text = navPointNode.Element(ns + "navLabel")?.Element(ns + "text")?.Value
					};

					foreach (var subNavPoint in ParseNavPoints(navPointNode, level + 1).ToList())
						navPoint.Items.Add(subNavPoint);

					navPoints.Add(navPoint);
				}

				return navPoints;
			}

			foreach (var navPoint in ParseNavPoints(navMapNode, 0).ToList())
			{
				tableOfContents.Items.Add(navPoint);
			}

			return tableOfContents;
		}

		public static void Initialize(this EBook eBook)
		{
			eBook.ContentLocation = eBook.GetContentLocation();
			var manifestItems = eBook.GetManifest().ToList();

			foreach (var item in manifestItems)
			{
				eBook.Manifest[item.Id] = item;
			}

			eBook.CoverLocation = eBook.GetCoverLocation();
			eBook.Metadata = eBook.GetMetadata();
		}

		public static bool IsMimetypeValid(this EBook eBook)
		{
			var expectedMimetype = "application/epub+zip";
			var mimetypeFiles = Directory.GetFiles(eBook.BaseDirectory, "mimetype");

			if (mimetypeFiles.Length != 1)
				return false;

			var mimetypeFile = mimetypeFiles[0];
			var mimetype = File.ReadAllText(mimetypeFile).Trim(' ', '\n', '\r', '\t').ToLower();

			if (mimetype != expectedMimetype)
				return false;

			return true;
		}
	}
}
