using Windows.UI.Xaml.Documents;

namespace Papyrus.HtmlParser.Extensions
{
    public static class InlineCollectionExtensions
    {
        public static bool SafeAdd(this InlineCollection collection, Inline inline)
        {
            if (inline != null)
            {
                try
                {
                    collection.Add(inline);
                }

                catch
                {
                    return false;
                }
            }

            else
                return false;

            return true;
        }
    }
}
