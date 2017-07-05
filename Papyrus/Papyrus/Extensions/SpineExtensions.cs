using System.Linq;

namespace Papyrus.Extensions
{
    internal static class SpineExtensions
    {
        public static SpineItem Next(this Spine spine, SpineItem item)
        {
            var index = spine.IndexOf(item);

            if (index + 1 < spine.Count)
                return spine.ElementAt(index + 1);

            return null;
        }

        public static SpineItem Previous(this Spine spine, SpineItem item)
        {
            var index = spine.IndexOf(item);

            if (index - 1 > 0)
                return spine.ElementAt(index - 1);

            return null;
        }
    }
}
