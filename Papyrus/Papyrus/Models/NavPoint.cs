using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace Papyrus
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NavPoint : PapyrusItem
    {
        #region ContentPath

        private string _contentPath = default(string);
        [JsonProperty("content_path")]
        public string ContentPath { get => _contentPath; set => Set(ref _contentPath, value); }

        #endregion ContentPath

        #region Id

        private string _id = default(string);
        [JsonProperty("id")]
        public string Id { get => _id; set => Set(ref _id, value); }

        #endregion Id

        #region Items

        private ObservableCollection<NavPoint> _items = new ObservableCollection<NavPoint>();
        [JsonProperty("items")]
        public ObservableCollection<NavPoint> Items { get => _items; }

        #endregion Items

        #region Level

        private int _level = 0;
        [JsonProperty("level")]
        public int Level { get => _level; set => Set(ref _level, value); }

        #endregion Level

        #region PlayOrder

        private int _playOrder = default(int);
        [JsonProperty("play_order")]
        public int PlayOrder { get => _playOrder; set => Set(ref _playOrder, value); }

        #endregion PlayOrder

        #region Text

        private string _text = default(string);
        [JsonProperty("text")]
        public string Text { get => _text; set => Set(ref _text, value); }

        #endregion Text
    }
}
