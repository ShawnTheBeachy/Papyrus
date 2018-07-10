using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Papyrus
{
	public class TableOfContents : BaseNotify
    {
		#region FlatItems

		public IEnumerable<NavPoint> FlatItems => FlattenItems(Items);

		#endregion FlatItems

		#region Items

		public ObservableCollection<NavPoint> Items { get; } = new ObservableCollection<NavPoint>();

		#endregion Items

		#region Title

		private string _title;
		public string Title
		{
			get => _title;
			set => Set(ref _title, value);
		}

		#endregion Title

		public TableOfContents()
		{
			Items.CollectionChanged += (s, e) =>
			{
				RaisePropertyChanged(nameof(FlatItems));
			};
		}

		private IEnumerable<NavPoint> FlattenItems(IEnumerable<NavPoint> navPoints)
		{
			var flatList = new List<NavPoint>();

			foreach (var navPoint in navPoints)
			{
				flatList.Add(navPoint);

				foreach (var subNavPoint in FlattenItems(navPoint.Items))
					flatList.Add(subNavPoint);
			}

			return flatList;
		}
	}
}
