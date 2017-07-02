using System;
using System.Collections.Generic;
using System.Linq;
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

            foreach (var item in (await this.GetManifestAsync()).ToList())
                Manifest.Add(item.Id, item);

            TableOfContents = await this.GetTableOfContentsAsync();
        }

        #region ContentLocation

        private string _contentLocation = default(string);
        public string ContentLocation { get => _contentLocation; set => Set(ref _contentLocation, value); }

        #endregion ContentLocation

        #region Manifest

        private Dictionary<string, ManifestItem> _manifest = new Dictionary<string, ManifestItem>();
        public Dictionary<string, ManifestItem> Manifest { get => _manifest; }

        #endregion Manifest

        #region Metadata

        private Metadata _metadata = default(Metadata);
        public Metadata Metadata { get => _metadata; set => Set(ref _metadata, value); }

        #endregion Metadata

        #region RootPath

        public string RootPath => _rootFolder.Path;

        #endregion RootPath

        #region TableOfContents

        private TableOfContents _tableOfContents = default(TableOfContents);
        public TableOfContents TableOfContents { get => _tableOfContents; set => Set(ref _tableOfContents, value); }

        #endregion TableOfContents
    }
}
