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
            LineHeight = style?.LineHeight;
            TextAlignment = style?.TextAlignment;
            TextDecoration = style?.TextDecoration;
            TextIndent = style?.TextIndent;
        }

        public void RebaseOn(Style style)
        {
            FontSize = style?.FontSize ?? FontSize;
            FontStyle = style?.FontStyle ?? FontStyle;
            FontWeight = style?.FontWeight ?? FontWeight;
            Foreground = style?.Foreground ?? Foreground;
            LineHeight = style?.LineHeight ?? LineHeight;
            TextAlignment = style?.TextAlignment ?? TextAlignment;
            TextDecoration = style?.TextDecoration ?? TextDecoration;
            TextIndent = style?.TextIndent ?? TextIndent;
        }

        public double? FontSize { get; set; } = 16;
        public FontStyle? FontStyle { get; set; } = Windows.UI.Text.FontStyle.Normal;
        public FontWeight? FontWeight { get; set; } = FontWeights.Normal;
        public Color? Foreground { get; set; } = Colors.Black;
        public double? LineHeight { get; set; } = null;
        public TextAlignment? TextAlignment { get; set; } = Windows.UI.Xaml.TextAlignment.Left;
        public TextDecorations? TextDecoration { get; set; } = TextDecorations.None;
        public double? TextIndent { get; set; } = 24;
    }
}
