using System.Collections.ObjectModel;

namespace Papyrus
{
	public class Spine : ObservableCollection<SpineItem>
	{
		#region Toc

		private string _toc = default(string);
		public string Toc
		{
			get => _toc;
			set => _toc = value;
		}

		#endregion Toc
	}
}
