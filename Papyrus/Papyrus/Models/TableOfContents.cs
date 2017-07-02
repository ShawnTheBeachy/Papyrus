using System.Collections.ObjectModel;

namespace Papyrus
{
    public class TableOfContents : BaseNotify
    {
        #region Items

        private ObservableCollection<NavPoint> _items = new ObservableCollection<NavPoint>();
        public ObservableCollection<NavPoint> Items { get => _items; }

        #endregion Items

        #region Title

        private string _title = default(string);
        public string Title { get => _title; set => Set(ref _title, value); }

        #endregion Title
    }
}
