using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Papyrus.UI
{
    public sealed partial class TableOfContents : UserControl
    {
        #region Dependency properties

        #region Source

        public Papyrus.TableOfContents Source
        {
            get { return (Papyrus.TableOfContents)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(Papyrus.TableOfContents), typeof(TableOfContents), new PropertyMetadata(null));

        #endregion Source

        #endregion Dependency properties

        public TableOfContents()
        {
            InitializeComponent();
        }
    }
}
