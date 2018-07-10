using ExCSS;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Text;
using Windows.UI.Xaml.Documents;

namespace Papyrus.UWP.Parsers
{
	public class StylesheetParser
	{
		public IEnumerable<string> ApplyStyles(TextElement element, IEnumerable<ElementStyle> styles, string node, string classes = "", string id = "", IEnumerable<string> force = null)
		{
			var matches = new List<string>();

			foreach (var style in styles)
			{
				var newMatches = FindMatches(style, node, classes, id, force).ToList();

				if (newMatches.Count < 1)
				{
					continue;
				}

				matches.AddRange(newMatches);

				foreach (var property in style.Properties)
				{
					switch (property.Name)
					{
						case "font-size":
							var unitMultipliers = new Dictionary<string, double>
							{
								["px"] = 0,
								["em"] = 16,
								["rem"] = 16,
								["pt"] = .8
							};
							var valueBuilder = new StringBuilder();
							var unitBuilder = new StringBuilder();

							foreach (var c in property.Value)
							{
								if (char.IsNumber(c) || c == '.')
								{
									valueBuilder.Append(c);
								}

								else
								{
									unitBuilder.Append(c);
								}
							}

							var value = double.Parse(valueBuilder.ToString());
							value = value * unitMultipliers[unitBuilder.ToString()];
							element.FontSize = value;
							break;
						case "font-style":
							element.FontStyle = property.Value == "italic" ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal;
							break;
						case "font-weight":
							element.FontWeight = property.Value == "bold" ? FontWeights.Bold : FontWeights.Normal;
							break;
						case "text-align":
							if (element is Block block)
							{
								block.TextAlignment = property.Value == "left" ? Windows.UI.Xaml.TextAlignment.Left :
									property.Value == "center" ? Windows.UI.Xaml.TextAlignment.Center :
									property.Value == "justify" ? Windows.UI.Xaml.TextAlignment.Justify :
									property.Value == "right" ? Windows.UI.Xaml.TextAlignment.Right : Windows.UI.Xaml.TextAlignment.Left;
							}
							break;
					}
				}
			}

			return matches;
		}

		private IEnumerable<string> FindMatches(ElementStyle style, string node, string classes = "", string id = "", IEnumerable<string> force = null)
		{
			var matches = new List<string>();
			
			foreach (var key in style.Keys)
			{
				if (key == "*")
				{
					matches.Add(key);
				}

				if (key == node)
				{
					matches.Add(key);
				}

				if (force != null)
				{
					foreach (var item in force)
					{
						if (key == item)
						{
							matches.Add(key);
						}
					}
				}

				if (!string.IsNullOrWhiteSpace(id) && key == $"#{id}")
				{
					matches.Add(key);
				}

				foreach (var className in classes.Split(' ').ToList())
				{
					if (key == $".{className}")
					{
						matches.Add(key);
					}
				}
			}

			return matches;
		}

		public IEnumerable<ElementStyle> ParseStyles(string css)
		{
			var parser = new ExCSS.StylesheetParser();
			var stylesheet = parser.Parse(css);

			foreach (var child in stylesheet.Children)
			{
				if (child is Rule rule)
				{
					if (rule.Type == RuleType.Style)
					{
						var style = new ElementStyle();

						foreach (var ruleChild in rule.Children)
						{
							if (ruleChild is ISelector selector)
							{
								style.Keys.Add(selector.Text);
							}

							else if (ruleChild is StyleDeclaration declaration)
							{
								foreach (var prop in declaration.Children)
								{
									if (prop is Property property)
									{
										style.Properties.Add(property);
									}
								}
							}
						}

						yield return style;
					}
				}
			}
		}
	}
}
