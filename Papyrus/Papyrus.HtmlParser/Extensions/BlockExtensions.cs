using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace Papyrus.HtmlParser.Extensions
{
    internal static class BlockExtensions
    {
        public static void ApplyStyle(this Block block, Style style)
        {
            block.FontSize = style.FontSize.Value;
            block.FontStyle = style.FontStyle.Value;
            block.FontWeight = style.FontWeight.Value;
            block.Foreground = style.Foreground != null ? new SolidColorBrush(style.Foreground.Value) : null;
            block.TextAlignment = style.TextAlignment.Value;
        }
    }
}
