using System.Collections.ObjectModel;

namespace Papyrus
{
	public class NavPoint : BaseNotify
    {
		#region ContentPath

		private string _contentPath;
		public string ContentPath
		{
			get => _contentPath;
			set => Set(ref _contentPath, value);
		}

		#endregion ContentPath

		#region Id

		private string _id;
		public string Id
		{
			get => _id;
			set => Set(ref _id, value);
		}

		#endregion Id

		#region Items

		public ObservableCollection<NavPoint> Items { get; } = new ObservableCollection<NavPoint>();

		#endregion Items

		#region Level

		private int _level;
		public int Level
		{
			get => _level;
			set => Set(ref _level, value);
		}

		#endregion Level

		#region PlayOrder

		private int _playOrder;
		public int PlayOrder
		{
			get => _playOrder;
			set => Set(ref _playOrder, value);
		}

		#endregion PlayOrder

		#region Text

		private string _text;
		public string Text
		{
			get => _text;
			set => Set(ref _text, value);
		}

		#endregion Text
	}
}
