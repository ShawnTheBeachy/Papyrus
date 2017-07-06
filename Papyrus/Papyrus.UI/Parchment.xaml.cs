using Papyrus.HtmlParser;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Papyrus.UI
{
    public sealed partial class Parchment : UserControl
    {
        private RichTextBlock _contentTextBlock;
        private Binding _paddingBinding, _lineHeightBinding, _indentationBinding;

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

            _paddingBinding = new Binding
            {
                FallbackValue = new Thickness(24),
                TargetNullValue = new Thickness(24),
                Source = this,
                Path = new PropertyPath("Padding"),
                Mode = BindingMode.OneWay
            };
            _lineHeightBinding = new Binding
            {
                FallbackValue = 30,
                TargetNullValue = 30,
                Source = this,
                Path = new PropertyPath("LineHeight"),
                Mode = BindingMode.OneWay
            };
            _contentTextBlock = new RichTextBlock();
        }

        public async Task LoadContentAsync(NavPoint navPoint)
        {
            _contentTextBlock.Blocks.Clear();
            MainFlipView.Items.Clear();
            var contents = await Source.GetContentsAsync(navPoint);
            var converter = new Converter();
            converter.Convert(contents);
            
            foreach (var block in converter.ConvertedBlocks)
                _contentTextBlock.Blocks.Add(block);
            
            Overflow(_contentTextBlock);

            MainFlipView.Items.Add(_contentTextBlock);
        }

        private async void Overflow(RichTextBlock rtb)
        {
            async Task Flow()
            {
                if (rtb.HasOverflowContent && rtb.OverflowContentTarget == null)
                {
                    var index = MainFlipView.Items.IndexOf(rtb);
                    var target = new RichTextBlockOverflow();
                    target.SetBinding(PaddingProperty, _paddingBinding);
                    rtb.OverflowContentTarget = target;
                    await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MainFlipView.Items.Insert(index + 1, target);
                    });
                    await Overflow(target);
                }

                else if (!rtb.HasOverflowContent && rtb.OverflowContentTarget != null)
                {
                    await Underflow(rtb.OverflowContentTarget);
                    await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MainFlipView.Items.Remove(rtb.OverflowContentTarget);
                    });
                    rtb.OverflowContentTarget = null;
                }
            }

            await Flow();

            rtb.RegisterPropertyChangedCallback(RichTextBlock.HasOverflowContentProperty, async (DependencyObject sender, DependencyProperty prop) =>
            {
                await Flow();
            });
        }

        private async Task Overflow(RichTextBlockOverflow rtb)
        {
            async Task Flow()
            {
                if (rtb.HasOverflowContent && rtb.OverflowContentTarget == null)
                {
                    var index = MainFlipView.Items.IndexOf(rtb);
                    var target = new RichTextBlockOverflow();
                    target.SetBinding(PaddingProperty, _paddingBinding);
                    rtb.OverflowContentTarget = target;
                    await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MainFlipView.Items.Insert(index + 1, target);
                    });
                    await Overflow(target);
                }

                else if (!rtb.HasOverflowContent && rtb.OverflowContentTarget != null)
                {
                    await Underflow(rtb.OverflowContentTarget);
                    await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MainFlipView.Items.Remove(rtb.OverflowContentTarget);
                    });
                    rtb.OverflowContentTarget = null;
                }
            }

            await Flow();

            rtb.RegisterPropertyChangedCallback(RichTextBlockOverflow.HasOverflowContentProperty, async (DependencyObject sender, DependencyProperty prop) =>
            {
                await Flow();
            });
        }

        private async Task Underflow(RichTextBlockOverflow rtbo)
        {
            async Task Flow()
            {
                if (rtbo.OverflowContentTarget != null)
                {
                    await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MainFlipView.Items.Remove(rtbo.OverflowContentTarget);
                        rtbo.OverflowContentTarget = null;
                    });
                }
            }

            await Flow();
        }
    }
}
