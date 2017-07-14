using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;

namespace Papyrus.HtmlParser
{
    internal class Style
    {
        public Style() { }

        public Style(Style style)
        {
            FontSize = style?.FontSize;
            FontStyle = style?.FontStyle;
            FontWeight = style?.FontWeight;
            Foreground = style?.Foreground;
            TextAlignment = style?.TextAlignment;
        }

        public void RebaseOn(Style style)
        {
            FontSize = style?.FontSize ?? FontSize;
            FontStyle = style?.FontStyle ?? FontStyle;
            FontWeight = style?.FontWeight ?? FontWeight;
            Foreground = style?.Foreground ?? Foreground;
            TextAlignment = style?.TextAlignment ?? TextAlignment;
        }

        public double? FontSize { get; set; } = 16;
        public FontStyle? FontStyle { get; set; } = Windows.UI.Text.FontStyle.Normal;
        public FontWeight? FontWeight { get; set; } = FontWeights.Normal;
        public Color? Foreground { get; set; } = Colors.Black;
        public TextAlignment? TextAlignment { get; set; } = Windows.UI.Xaml.TextAlignment.Left;
    }
}
