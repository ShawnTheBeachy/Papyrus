using System.Collections.Generic;
using Windows.UI.Xaml.Documents;

namespace Papyrus.UWP.Helpers.Extensions
{
	internal static class CollectionExtensions
	{
		internal static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
		{
			foreach (var item in items)
				list.Add(item);
		}

		internal static bool SafeAdd(this InlineCollection collection, Inline inline)
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
