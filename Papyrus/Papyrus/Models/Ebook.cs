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
            Metadata = await this.GetMetadataAsync();
        }

        #region ContentLocation

        private string _contentLocation = default(string);
        public string ContentLocation { get => _contentLocation; set => Set(ref _contentLocation, value); }

        #endregion ContentLocation

        #region Metadata

        private Metadata _metadata = default(Metadata);
        public Metadata Metadata { get => _metadata; set => Set(ref _metadata, value); }

        #endregion Metadata

        #region RootPath

        public string RootPath => _rootFolder.Path;

        #endregion RootPath
    }
}
