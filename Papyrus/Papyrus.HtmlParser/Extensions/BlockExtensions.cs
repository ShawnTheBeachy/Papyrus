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

            if (style.LineHeight != null)
                block.LineHeight = style.LineHeight.Value;

            block.TextAlignment = style.TextAlignment.Value;
            block.TextDecorations = style.TextDecoration.Value;

            if (block is Paragraph paragraph)
                paragraph.TextIndent = style.TextIndent.Value;
        }
    }
}
