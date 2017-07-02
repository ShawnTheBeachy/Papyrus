using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace Papyrus
{
    public static class NavPointExtensions
    {
        /// <summary>
        /// Gets the XHTML content of a file.
        /// </summary>
        /// <param name="navPoint">The NavPoint for which to get the content.</param>
        /// <param name="embedImages">If true, image links will be replaced with a base64-encoded image.</param>
        /// <returns></returns>
        public static async Task<string> GetContentsAsync(this NavPoint navPoint, bool embedImages = true)
        {
            var contentFile = await navPoint._rootFolder.GetFileFromPathAsync(navPoint.ContentPath);
            var contents = await FileIO.ReadTextAsync(contentFile);

            if (embedImages)
            {
                var contentPath = Path.Combine(navPoint._rootFolder.Path, navPoint.ContentPath);
                var imageMatches = new Regex(@"<img.*/>", RegexOptions.IgnoreCase).Matches(contents).OfType<Match>().ToList();

                foreach (var match in imageMatches)
                {
                    var imageNode = HtmlNode.CreateNode(match.Value);
                    var imageSource = imageNode.Attributes["src"].Value;

                    var imgPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(contentPath), imageSource));
                    var imageFile = await navPoint._rootFolder.GetFileFromPathAsync(imgPath.Substring(navPoint._rootFolder.Path.Length));
                    var image = await FileIO.ReadBufferAsync(imageFile);
                    var base64 = Convert.ToBase64String(image.ToArray());
                    imageNode.Attributes["src"].Value = $"data:image/{imageFile.FileType};base64,{base64}";
                    contents = contents.Replace(match.Value, imageNode.OuterHtml);
                }
            }

            return contents;
        }
    }
}
