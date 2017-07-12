using System.Collections.Generic;

namespace Papyrus.HtmlParser.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue SafeGet<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            if (dict.ContainsKey(key))
                return dict[key];
            else
                return default(TValue);
        }
    }
}
