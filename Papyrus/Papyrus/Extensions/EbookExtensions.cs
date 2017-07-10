using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Papyrus
{
    public static class EBookExtensions
    {
        /// <summary>
        /// Gets XHTML content for a file.
        /// </summary>
        /// <param name="ebook"></param>
        /// <param name="navPoint"></param>
        /// <returns></returns>
        public static async Task<string> GetContentsAsync(this EBook ebook, ManifestItem manifestItem)
        {
            var fullContentPath = Path.GetFullPath(ebook._rootFolder.Path.EnsureEnd("\\") + ebook.ContentLocation);
            var tocPath = Path.GetFullPath(Path.GetDirectoryName(fullContentPath).EnsureEnd("\\") + ebook.Manifest["ncx"].ContentLocation);
            var filePath = Path.GetFullPath(Path.GetDirectoryName(tocPath).EnsureEnd("\\") + manifestItem.ContentLocation);
            var contentFile = await ebook._rootFolder.GetFileFromPathAsync(filePath.Substring(ebook._rootFolder.Path.Length));
            var contents = await FileIO.ReadTextAsync(contentFile);
            return contents;
        }

        public static async Task<string> GetContentsAsync(this EBook ebook, SpineItem spineItem)
        {
            var manifestItem = ebook.Manifest[spineItem.IdRef];
            return await ebook.GetContentsAsync(manifestItem);
        }

        public static async Task<string> GetContentsAsync(this EBook ebook, NavPoint navPoint)
        {
            var manifestItem = ebook.Manifest.FirstOrDefault(a => Path.GetFileName(a.Value.ContentLocation) == Path.GetFileName(navPoint.ContentPath)).Value;
            return await ebook.GetContentsAsync(manifestItem);
        }

        /// <summary>
        /// Gets the location of the content.opf file.
        /// </summary>
        /// <param name="ebook">The EBook for which to get the content location.</param>
        /// <returns>A string location.</returns>
        internal static async Task<string> GetContentLocationAsync(this EBook ebook)
        {
            async Task<string> GetContentXmlAsync()
            {
                var folder = await ebook._rootFolder.GetFolderAsync("META-INF");
                var file = await folder.GetFileAsync("container.xml");
                var xml = await FileIO.ReadTextAsync(file);
                return xml;
            }

            XElement GetRootFileNode(string xml)
            {
                var doc = XDocument.Parse(xml);
                var ns = doc.Root.GetDefaultNamespace();
                var node = doc.Element(ns + "container").Element(ns + "rootfiles").Element(ns + "rootfile");
                return node;
            }

            bool VerifyMediaType(XElement node) =>
                node.Attribute("media-type")?.Value == "application/oebps-package+xml";
            
            var containerXml = await GetContentXmlAsync();
            var rootFileNode = GetRootFileNode(containerXml);

            if (!VerifyMediaType(rootFileNode))
                throw new Exception("Invalid media type on rootfile node.");

            return rootFileNode.Attribute("full-path")?.Value;
        }

        /// <summary>
        /// Gets a bitmap image of the cover.
        /// </summary>
        /// <param name="ebook"></param>
        /// <returns></returns>
        internal static async Task<ImageSource> GetCoverAsync(this EBook ebook)
        {
            if (!ebook.Manifest.ContainsKey("cover"))
                return null;

            var relativeLocation = ebook.Manifest["cover"].ContentLocation;
            var coverFile = await ebook._rootFolder.GetFileFromPathAsync(Path.Combine(Path.GetDirectoryName(ebook.ContentLocation), relativeLocation));
            var stream = await coverFile.OpenReadAsync();
            var bitmap = new BitmapImage();
            bitmap.SetSource(stream);
            return bitmap;
        }

        /// <summary>
        /// Gets a list of ManifestItems for an EBook.
        /// </summary>
        /// <param name="ebook"></param>
        /// <returns></returns>
        internal static async Task<IEnumerable<ManifestItem>> GetManifestAsync(this EBook ebook)
        {
            var items = new List<ManifestItem>();

            IEnumerable<XElement> GetManifestItemNodes(string xml)
            {
                var doc = XDocument.Parse(xml);
                var ns = doc.Root.GetDefaultNamespace();
                var node = doc.Element(ns + "package").Element(ns + "manifest");
                var itemNodes = node.Elements(ns + "item");
                return itemNodes;
            }

            var contentFile = await ebook._rootFolder.GetFileFromPathAsync(ebook.ContentLocation);
            var contentXml = await FileIO.ReadTextAsync(contentFile);

            foreach (var itemNode in GetManifestItemNodes(contentXml).ToList())
                items.Add(new ManifestItem
                {
                    ContentLocation = itemNode.Attribute("href").Value,
                    Id = itemNode.Attribute("id").Value,
                    MediaType = itemNode.Attribute("media-type").Value
                });

            return items;
        }

        /// <summary>
        /// Gets metadata from an EBook.
        /// </summary>
        /// <param name="ebook">The EBook for which to get metadata.</param>
        /// <returns>The metadata object.</returns>
        internal static async Task<Metadata> GetMetadataAsync(this EBook ebook)
        {
            XElement GetMetadataNode(string xml)
            {
                var doc = XDocument.Parse(xml);
                var ns = doc.Root.GetDefaultNamespace();
                var node = doc.Element(ns + "package").Element(ns + "metadata");
                return node;
            }

            var contentFile = await ebook._rootFolder.GetFileFromPathAsync(ebook.ContentLocation);
            var contentXml = await FileIO.ReadTextAsync(contentFile);
            var metadataNode = GetMetadataNode(contentXml);
            var dcNamespace = metadataNode.GetNamespaceOfPrefix("dc");

            string GetValue(string node) =>
                metadataNode.Element(dcNamespace + node)?.Value;
            
            var metadata = new Metadata
            {
                AlternativeTitle = GetValue("alternative"),
                Audience = GetValue("audience"),
                Available = GetValue("available") == null ? default(DateTime) : DateTime.Parse(GetValue("available")),
                Contributor = GetValue("contributor"),
                Created = GetValue("created") == null ? default(DateTime) : DateTime.Parse(GetValue("created")),
                Creator = GetValue("creator"),
                Date = GetValue("date") == null ? default(DateTime) : DateTime.Parse(GetValue("date")),
                Description = GetValue("description"),
                Language = GetValue("language"),
                Title = GetValue("title")
            };

            return metadata;
        }

        internal static async Task<Spine> GetSpineAsync(this EBook ebook)
        {
            var spine = new Spine();

            IEnumerable<XElement> GetSpineItemNodes(string xml)
            {
                var doc = XDocument.Parse(xml);
                var ns = doc.Root.GetDefaultNamespace();
                var node = doc.Element(ns + "package").Element(ns + "spine");
                spine.Toc = node.Attribute("toc").Value;
                var itemNodes = node.Elements(ns + "itemref");
                return itemNodes;
            }

            var contentFile = await ebook._rootFolder.GetFileFromPathAsync(ebook.ContentLocation);
            var contentXml = await FileIO.ReadTextAsync(contentFile);

            foreach (var itemNode in GetSpineItemNodes(contentXml).ToList())
                spine.Add(new SpineItem
                {
                    IdRef = itemNode.Attribute("idref").Value
                });

            return spine;
        }

        public static SpineItem GetSpineItem(this EBook ebook, ManifestItem manifestItem)
        {
            var spineItem = ebook.Spine.FirstOrDefault(a => a.IdRef == manifestItem.Id);
            return spineItem;
        }

        /// <summary>
        /// Gets the table of contents for an EBook.
        /// </summary>
        /// <param name="ebook"></param>
        /// <returns></returns>
        internal static async Task<TableOfContents> GetTableOfContentsAsync(this EBook ebook)
        {
            var relativeLocation = ebook.Manifest[ebook.Spine.Toc].ContentLocation;
            var tocFile = await ebook._rootFolder.GetFileFromPathAsync(Path.Combine(Path.GetDirectoryName(ebook.ContentLocation), relativeLocation));
            var xml = await FileIO.ReadTextAsync(tocFile);
            var doc = XDocument.Parse(xml);
            var ns = doc.Root.GetDefaultNamespace();

            var tableOfContents = new TableOfContents
            {
                Title = doc.Element(ns + "ncx").Element(ns + "docTitle").Element(ns + "text").Value
            };

            var navMapNode = doc.Element(ns + "ncx").Element(ns + "navMap");

            IEnumerable<NavPoint> ParseNavPoints(XElement node, int level)
            {
                var navPoints = new List<NavPoint>();
                var navPointNodes = node.Elements(ns + "navPoint").ToList();

                foreach (var navPointNode in navPointNodes)
                {
                    var navPoint = new NavPoint
                    {
                        ContentPath = navPointNode.Element(ns + "content")?.Attribute("src").Value,
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
                tableOfContents.Items.Add(navPoint);

            return tableOfContents;
        }

        /// <summary>
        /// Verifies that the EBook has a valid mimetype file.
        /// </summary>
        /// <param name="ebook">The EBook to be checked.</param>
        /// <returns>A bool indicating whether or not the miemtype is valid.</returns>
        internal static async Task<bool> VerifyMimetypeAsync(this EBook ebook)
        {
            bool VerifyMimetypeString(string value) =>
                value == "application/epub+zip";

            if (ebook._rootFolder == null)                                      // Make sure a root folder was specified.
                return false;

            var mimetypeFile = await ebook._rootFolder.GetItemAsync("mimetype");

            if (mimetypeFile == null)                                           // Make sure file exists.
                return false;

            var fileContents = await FileIO.ReadTextAsync(mimetypeFile as StorageFile);

            if (!VerifyMimetypeString(fileContents))                         // Make sure file contents are correct.
                return false;

            return true;
        }
    }
}
