using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;

namespace Papyrus.HtmlParser
{
    public static class CssHelpers
    {
        public static Style GetStyleFromCssProperties(Dictionary<string, string> props)
        {
            var style = new Style
            {
                FontSize = null,
                FontStyle = null,
                FontWeight = null,
                Foreground = null,
                TextAlignment = null
            };

            foreach (var prop in props)
            {
                switch (prop.Key)
                {
                    case "font-size":
                        style.FontSize = GetNumber(prop.Value);
                        break;
                    case "text-align":
                        style.TextAlignment = GetTextAlignment(prop.Value);
                        break;
                }
            }

            return style;
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

    }
}
