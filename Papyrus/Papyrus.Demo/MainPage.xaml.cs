using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Papyrus.Demo
{
	public sealed partial class MainPage : Page
    {
        #region EBooks

        public ObservableCollection<EBook> EBooks
        {
            get { return (ObservableCollection<EBook>)GetValue(EBooksProperty); }
            set { SetValue(EBooksProperty, value); }
        }

        public static readonly DependencyProperty EBooksProperty =
            DependencyProperty.Register("EBooks", typeof(ObservableCollection<EBook>), typeof(MainPage), new PropertyMetadata(new ObservableCollection<EBook>()));

        #endregion EBooks

        public MainPage()
        {
            InitializeComponent();
        }
        
        private async void OpenBookButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                SettingsIdentifier = "com.TastesLikeTurkey.Papyrus.OpenEpubPicker",
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                ViewMode = PickerViewMode.List
            };
            picker.FileTypeFilter.Add(".epub");
            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                var epubFolder = await ExtractZipAsync(file);
                var eBook = new EBook(epubFolder);
                await eBook.InitializeAsync();
                EBooks.Add(eBook);
            }
        }

        private async Task<StorageFolder> ExtractZipAsync(StorageFile file)
        {
            var extractFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(file.DisplayName, CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenStreamForReadAsync())
            {
                var archive = new ZipArchive(stream);
                archive.ExtractToDirectory(extractFolder.Path);
            }

            return extractFolder;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            EBooks.Clear();

            var epubFolders = await ApplicationData.Current.LocalFolder.GetFoldersAsync();

            foreach (var epubFolder in epubFolders)
            {
                var eBook = new EBook(epubFolder);
                await eBook.InitializeAsync();
                EBooks.Add(eBook);
            }
        }

        private void EBooksGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(BookPage), e.ClickedItem);
        }
    }
}
