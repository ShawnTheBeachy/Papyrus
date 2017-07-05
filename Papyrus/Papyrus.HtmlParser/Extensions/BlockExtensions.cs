using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace Papyrus.HtmlParser.Extensions
{
    public static class BlockExtensions
    {
        public static void ApplyStyle(this Block block, Style style)
        {
            block.FontSize = style.FontSize;
            block.FontStyle = style.FontStyle;
            block.FontWeight = style.FontWeight;
            block.Foreground = style.Foreground != null ? new SolidColorBrush(style.Foreground) : null;
            block.TextAlignment = style.TextAlignment;
        }
    }
}
