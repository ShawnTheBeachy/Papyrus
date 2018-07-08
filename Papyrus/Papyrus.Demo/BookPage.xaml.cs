using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

		private void TableOfContentsButton_Click(object sender, RoutedEventArgs e)
		{
			MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
		}
        
        private void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
