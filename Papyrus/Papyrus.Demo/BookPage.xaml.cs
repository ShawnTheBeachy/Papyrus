using Papyrus.HtmlParser;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Papyrus.Demo
{
    public sealed partial class BookPage : Page
    {
        #region EBook

        public EBook EBook
        {
            get { return (EBook)GetValue(EBookProperty); }
            set { SetValue(EBookProperty, value); }
        }
        
        public static readonly DependencyProperty EBookProperty =
            DependencyProperty.Register("EBook", typeof(EBook), typeof(BookPage), new PropertyMetadata(null));

        #endregion EBook
        
        public BookPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            EBook = e.Parameter as EBook;
        }

        private void TableOfContentsButton_Click(object sender, RoutedEventArgs e) =>
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;

        private async void TableOfContentsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var navPoint = e.AddedItems.FirstOrDefault() as NavPoint;

            if (navPoint == null)
                return;

            ContentTextBlock.Blocks.Clear();
            var contents = await EBook.GetContentsAsync(navPoint);
            var converter = new Converter();
            converter.Convert(contents);

            foreach (var block in converter.ConvertedBlocks)
                ContentTextBlock.Blocks.Add(block);
        }

        private void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
        
        private void FontFamily_Checked(object sender, RoutedEventArgs e)
        {
            ContentTextBlock.FontFamily = new FontFamily((sender as RadioButton).Content as string);
        }
    }
}
