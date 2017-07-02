using Windows.Storage;

namespace Papyrus
{
    public class EBook : BaseNotify
    {
        internal StorageFolder _rootFolder;

        public EBook() { }

        public EBook(StorageFolder folder)
        {
            _rootFolder = folder;
        }

        #region Title

        private string _title = default(string);
        public string Title { get => _title; set => Set(ref _title, value); }

        #endregion Title
    }
}
