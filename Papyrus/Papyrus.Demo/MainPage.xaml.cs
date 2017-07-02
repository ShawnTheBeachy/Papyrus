using System;
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Papyrus.Demo
{
    public sealed partial class MainPage : Page
    {
        #region EBook

        public EBook EBook
        {
            get => (EBook)GetValue(EBookProperty); set => SetValue(EBookProperty, value);
        }
        
        public static readonly DependencyProperty EBookProperty = DependencyProperty.Register("EBook", typeof(EBook), typeof(MainPage), new PropertyMetadata(null));

        #endregion EBook

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker
            {
                SettingsIdentifier = "TastesLikeTurkey.Papyrus.OpenFolderPicker",
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add(".epub");                     // This line is very important. The FolderPicker crashes if this line is deleted. See https://stackoverflow.com/questions/40456200/universal-app-folderpicker-system-runtime-interopservices-comexception for more information.
            var folder = await picker.PickSingleFolderAsync();

            if (folder == null)
                return;

            EBook = new EBook(folder);
            Debug.WriteLine($"Valid mimetype: {await EBook.VerifyMimetypeAsync()}");
        }
    }
}
