using System.Collections.ObjectModel;
using Windows.Storage;

namespace Papyrus
{
    public class NavPoint : BaseNotify
    {
        internal StorageFolder _rootFolder;

        #region ContentPath

        private string _contentPath = default(string);
        public string ContentPath { get => _contentPath; set => Set(ref _contentPath, value); }

        #endregion ContentPath

        #region Id

        private string _id = default(string);
        public string Id { get => _id; set => Set(ref _id, value); }

        #endregion Id

        #region Items

        private ObservableCollection<NavPoint> _items = new ObservableCollection<NavPoint>();
        public ObservableCollection<NavPoint> Items { get => _items; }

        #endregion Items

        #region Level

        private int _level = 0;
        public int Level { get => _level; set => Set(ref _level, value); }

        #endregion Level

        #region PlayOrder

        private int _playOrder = default(int);
        public int PlayOrder { get => _playOrder; set => Set(ref _playOrder, value); }

        #endregion PlayOrder

        #region Text

        private string _text = default(string);
        public string Text { get => _text; set => Set(ref _text, value); }

        #endregion Text
    }
}
