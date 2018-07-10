using ExCSS;
using System.Collections.Generic;

namespace Papyrus.UWP
{
	public class ElementStyle
    {
		public IList<string> Keys { get; set; } = new List<string>();
		public IList<Property> Properties { get; set; } = new List<Property>();
	}
}
