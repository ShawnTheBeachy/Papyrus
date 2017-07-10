using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Papyrus.UI
{
    public sealed partial class TableOfContents : UserControl
    {
        #region Dependency properties

        #region Header

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(TableOfContents), new PropertyMetadata(null));

        #endregion Header

        #region Parchment

        public Parchment Parchment
        {
            get { return (Parchment)GetValue(ParchmentProperty); }
            set { SetValue(ParchmentProperty, value); }
        }


        public static readonly DependencyProperty ParchmentProperty =
            DependencyProperty.Register("Parchment", typeof(Parchment), typeof(TableOfContents), new PropertyMetadata(null));

        #endregion Parchment
        
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

        public event SelectionChangedEventHandler SelectionChanged;

        public TableOfContents()
        {
            InitializeComponent();
        }

        private async void TocListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(sender, e);

            if (Parchment != null)
                await Parchment.LoadContentAsync(e.AddedItems.FirstOrDefault() as NavPoint);
        }
    }

    public class LevelToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new Thickness(24 * (int)value, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => 
            throw new NotImplementedException();
    }

    public class ChapterPrependConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var s = value.ToString().Trim();

            if (s.Where(a => !char.IsNumber(a)).Count() == 0)
                return $"Chapter {s}";
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotImplementedException();
    }
}
