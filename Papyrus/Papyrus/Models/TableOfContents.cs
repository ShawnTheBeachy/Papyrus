using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Papyrus
{
    public class TableOfContents : BaseNotify
    {
        #region FlatItems

        public IEnumerable<NavPoint> FlatItems => FlattenItems(Items);

        #endregion FlatItems

        #region Items

        private ObservableCollection<NavPoint> _items = new ObservableCollection<NavPoint>();
        public ObservableCollection<NavPoint> Items { get => _items; }

        #endregion Items

        #region Title

        private string _title = default(string);
        public string Title { get => _title; set => Set(ref _title, value); }

        #endregion Title

        public TableOfContents()
        {
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(FlatItems));
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
