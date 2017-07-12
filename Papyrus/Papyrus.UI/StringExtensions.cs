namespace Papyrus.UI
{
    public static class StringExtensions
    {
        public static string EnsureEnd(this string value, string end)
        {
            if (value.EndsWith(end))
                return value;

            return value + end;
        }
    }
}
