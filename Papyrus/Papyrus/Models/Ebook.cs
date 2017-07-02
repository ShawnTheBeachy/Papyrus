using Windows.Storage;

namespace Papyrus.Models
{
    public class EBook : BaseNotify
    {
        public EBook() { }

        public EBook(StorageFolder folder)
        {

        }

        #region Title

        private string _title = default(string);
        public string Title { get => _title; set => Set(ref _title, value); }

        #endregion Title
    }
}
