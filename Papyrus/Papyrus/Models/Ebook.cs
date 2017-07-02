using System;
using System.Threading.Tasks;
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

        public async Task InitializeAsync()
        {
            if (await this.VerifyMimetypeAsync() == false)
                throw new Exception("Invalid mimetype.");

            ContentLocation = await this.GetContentLocationAsync();
        }

        #region ContentLocation

        private string _contentLocation = default(string);
        public string ContentLocation { get => _contentLocation; set => Set(ref _contentLocation, value); }

        #endregion ContentLocation

        #region RootPath

        public string RootPath => _rootFolder.Path;

        #endregion RootPath

        #region Title

        private string _title = default(string);
        public string Title { get => _title; set => Set(ref _title, value); }

        #endregion Title
    }
}
