using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;

namespace Papyrus.HtmlParser
{
    internal static class CssHelpers
    {
        public static Style GetStyleFromCssProperties(Dictionary<string, string> props, CssOptions options)
        {
            var style = new Style
            {
                FontSize = null,
                FontStyle = null,
                FontWeight = null,
                Foreground = null,
                LineHeight = null,
                TextAlignment = null,
                TextDecoration = null,
                TextIndent = null
            };

            foreach (var prop in props)
            {
                switch (prop.Key)
                {
                    case "color":
                        if (options.Foreground)
                            style.Foreground = GetColor(prop.Value);
                        break;
                    case "font-size":
                        if (options.FontSize)
                            style.FontSize = GetNumber(prop.Value);
                        break;
                    case "line-height":
                        if (options.LineHeight)
                            style.LineHeight = GetNumber(prop.Value);
                        break;
                    case "text-align":
                        if (options.TextAlign)
                            style.TextAlignment = GetTextAlignment(prop.Value);
                        break;
                    case "text-decoration":
                        if (options.TextDecoration)
                            style.TextDecoration = GetTextDecoration(prop.Value);
                        break;
                    case "text-indent":
                        if (options.TextIndent)
                            style.TextIndent = GetNumber(prop.Value);
                        break;
                }
            }

            return style;
        }

        private static Windows.UI.Color GetColor(string css)
        {
            var namedColorRegex = new Regex(@"(?:\s|:)([a-z]*);", RegexOptions.IgnoreCase);
            var hexColorRegex = new Regex(@"#([a-z]*);", RegexOptions.IgnoreCase);
            var rgbColorRegex = new Regex(@"rgb\(([0-9]+),\s?([0-9]+),\s?([0-9]+)\);", RegexOptions.IgnoreCase);
            var rgbaColorRegex = new Regex(@"rgba\(([0-9]+),\s?([0-9]+),\s?([0-9]+),\s?([0-9]+)\);", RegexOptions.IgnoreCase);

            if (namedColorRegex.Match(css) != null)
            {
                var colorName = namedColorRegex.Match(css).Groups.OfType<Group>().ElementAt(1).Value;
                var color = System.Drawing.Color.FromName(colorName);
                return Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);
            }

            else if (hexColorRegex.Match(css) != null)
            {
                var hexColor = hexColorRegex.Match(css).Groups.OfType<Group>().ElementAt(1).Value;
                var color = (Windows.UI.Color)new ColorConverter().ConvertFromString(hexColor);
                return color;
            }

            else if (rgbColorRegex.Match(css) != null)
            {
                var groups = rgbColorRegex.Match(css).Groups.OfType<Group>();
                var r = groups.ElementAt(1).Value;
                var g = groups.ElementAt(2).Value;
                var b = groups.ElementAt(3).Value;
                return Windows.UI.Color.FromArgb(255, byte.Parse(r), byte.Parse(g), byte.Parse(b));
            }

            else if (rgbaColorRegex.Match(css) != null)
            {
                var groups = rgbColorRegex.Match(css).Groups.OfType<Group>();
                var r = groups.ElementAt(1).Value;
                var g = groups.ElementAt(2).Value;
                var b = groups.ElementAt(3).Value;
                var a = groups.ElementAt(4).Value;
                return Windows.UI.Color.FromArgb(byte.Parse(a), byte.Parse(r), byte.Parse(g), byte.Parse(b));
            }

            else return Colors.Black;
        }

        private static double GetNumber(string css)
        {
            var numbers = css.Where(a => char.IsNumber(a) || a == '.');
            var value = new string(numbers.ToArray());
            return double.Parse(value);
        }

        private static TextAlignment GetTextAlignment(string css)
        {
            switch (css)
            {
                case "center":
                    return TextAlignment.Center;
                case "right":
                    return TextAlignment.Right;
                case "left":
                default:
                    return TextAlignment.Left;
            }
        }

        private static TextDecorations GetTextDecoration(string css)
        {
            switch (css)
            {
                case "line-through":
                    return TextDecorations.Strikethrough;
                case "underline":
                    return TextDecorations.Underline;
                default:
                    return TextDecorations.None;
            }
        }
    }
}
