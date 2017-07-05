namespace Papyrus
{
    internal static class StringExtensions
    {
        internal static string EnsureEnd(this string value, string end)
        {
            if (value.EndsWith(end))
                return value;

            return value + end;
        }
    }
}
