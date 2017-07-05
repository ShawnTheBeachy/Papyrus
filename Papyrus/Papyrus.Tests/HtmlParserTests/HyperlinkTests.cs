using Papyrus.HtmlParser;
using System.Linq;
using Windows.UI.Xaml.Documents;
using Xunit;

namespace Papyrus.Tests.HtmlParserTests
{
    public class HyperlinkTests
    {
        [Fact]
        public void TestOne()
        {
            var converter = new Converter();
            var html = @"<a href=""http://microsoft.com"">Microsoft</a>";
            var elements = converter.Convert(html);
            var hyperlink = elements.FirstOrDefault(a => a is Hyperlink) as Hyperlink;

            Assert.NotNull(hyperlink);
            Assert.Equal(hyperlink.NavigateUri.AbsoluteUri, "http://microsoft.com");
            Assert.NotEmpty(hyperlink.Inlines);
        }
    }
}
