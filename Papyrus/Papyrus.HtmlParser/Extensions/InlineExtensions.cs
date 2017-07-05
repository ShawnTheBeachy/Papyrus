using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace Papyrus.HtmlParser.Extensions
{
    public static class InlineExtensions
    {
        public static void ApplyStyle(this Inline inline, Style style)
        {
            inline.FontSize = style.FontSize;
            inline.FontStyle = style.FontStyle;
            inline.FontWeight = style.FontWeight;
            inline.Foreground = style.Foreground != null ? new SolidColorBrush(style.Foreground) : null;
        }
    }
}
