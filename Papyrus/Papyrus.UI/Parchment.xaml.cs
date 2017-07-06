using Papyrus.HtmlParser;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Papyrus.UI
{
    public sealed partial class Parchment : UserControl
    {
        #region Dependency properties

        #region LineHeight

        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            set { SetValue(LineHeightProperty, value); }
        }
        
        public static readonly DependencyProperty LineHeightProperty =
            DependencyProperty.Register("LineHeight", typeof(double), typeof(Parchment), new PropertyMetadata(30));

        #endregion LineHeight

        #region ParagraphIndentation

        public double ParagraphIndentation
        {
            get { return (double)GetValue(ParagraphIndentationProperty); }
            set { SetValue(ParagraphIndentationProperty, value); }
        }
        
        public static readonly DependencyProperty ParagraphIndentationProperty =
            DependencyProperty.Register("ParagraphIndentation", typeof(double), typeof(Parchment), new PropertyMetadata(24));

        #endregion ParagraphIndentation

        #region Source

        public EBook Source
        {
            get { return (EBook)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(EBook), typeof(Parchment), new PropertyMetadata(null));

        #endregion Source

        #endregion Dependency properties

        public Parchment()
        {
            InitializeComponent();
        }

        public async Task LoadContentAsync(NavPoint navPoint)
        {

            ContentTextBlock.Blocks.Clear();
            var contents = await Source.GetContentsAsync(navPoint);
            var converter = new Converter();
            converter.Convert(contents);

            foreach (var block in converter.ConvertedBlocks)
                ContentTextBlock.Blocks.Add(block);
        }
    }
}
