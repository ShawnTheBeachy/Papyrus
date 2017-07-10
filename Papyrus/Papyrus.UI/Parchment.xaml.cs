using Papyrus.HtmlParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace Papyrus.UI
{
    public sealed partial class Parchment : UserControl
    {
        private SpineItem _currentSpineItem;
        private Binding _paddingBinding, _lineHeightBinding, _indentationBinding, _foregroundBinding;

        #region Dependency properties
        
        #region IsBusy

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register("IsBusy", typeof(bool), typeof(Parchment), new PropertyMetadata(false));

        #endregion IsBusy

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
            _foregroundBinding = new Binding
            {
                FallbackValue = new SolidColorBrush(Colors.Black),
                TargetNullValue = new SolidColorBrush(Colors.Black),
                Source = this,
                Path = new PropertyPath("Foreground"),
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
            _indentationBinding = new Binding
            {
                FallbackValue = 24,
                TargetNullValue = 24,
                Source = this,
                Path = new PropertyPath("TextIndent"),
                Mode = BindingMode.OneWay
            };
        }

        public async Task LoadContentAsync(NavPoint navPoint)
        {
            IsBusy = true;
            var manifestItem = Source.Manifest.FirstOrDefault(a => Path.GetFileName(a.Value.ContentLocation) == Path.GetFileName(navPoint.ContentPath));
            _currentSpineItem = Source.Spine.FirstOrDefault(a => a.IdRef == manifestItem.Key);

            var contents = await Source.GetContentsAsync(navPoint);
            var converter = new Converter();
            converter.Convert(contents);
            await BuildViewAsync(converter.ConvertedBlocks);
            IsBusy = false;
        }

        public async Task LoadContentAsync(SpineItem spineItem)
        {
            IsBusy = true;
            _currentSpineItem = spineItem;
            var contents = await Source.GetContentsAsync(spineItem);
            var converter = new Converter();
            converter.Convert(contents);
            await BuildViewAsync(converter.ConvertedBlocks);
            IsBusy = false;
        }

        private async Task BuildViewAsync(IEnumerable<Block> blocks)
        {
            MainFlipView.ItemsSource = null;
            MainFlipView.Items.Clear();

            var previousIndex = Source.Spine.IndexOf(_currentSpineItem) - 1;

            if (previousIndex >= 0)
                MainFlipView.Items.Add(new FlipViewItem());

            var ContentTextBlock = new RichTextBlock
            {
                IsTextSelectionEnabled = false
            };
            ContentTextBlock.SetBinding(RichTextBlock.PaddingProperty, _paddingBinding);
            ContentTextBlock.SetBinding(RichTextBlock.LineHeightProperty, _lineHeightBinding);
            ContentTextBlock.SetBinding(RichTextBlock.TextIndentProperty, _indentationBinding);
            ContentTextBlock.SetBinding(RichTextBlock.ForegroundProperty, _foregroundBinding);

            foreach (var block in blocks)
                ContentTextBlock.Blocks.Add(block);

            MainFlipView.Items.Add(ContentTextBlock);

            if (MainFlipView.Items.FirstOrDefault() is FlipViewItem)
                MainFlipView.SelectedIndex = 1;

            await OverflowAsync(ContentTextBlock);
        }

        private async Task OverflowAsync(RichTextBlock rtb)
        {
            async Task Flow()
            {
                if (rtb.HasOverflowContent && rtb.OverflowContentTarget == null)
                {
                    var target = new RichTextBlockOverflow();
                    target.SetBinding(RichTextBlockOverflow.PaddingProperty, _paddingBinding);
                    rtb.OverflowContentTarget = target;
                    await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MainFlipView.Items.Add(target);
                    });
                    await OverflowAsync(target);
                }

                else if (!rtb.HasOverflowContent && rtb.OverflowContentTarget != null)
                {
                    await UnderflowAsync(rtb.OverflowContentTarget);
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

        private async Task OverflowAsync(RichTextBlockOverflow rtb)
        {
            async Task Flow()
            {
                if (rtb.HasOverflowContent && rtb.OverflowContentTarget == null)
                {
                    var target = new RichTextBlockOverflow();
                    target.SetBinding(RichTextBlockOverflow.PaddingProperty, _paddingBinding);
                    rtb.OverflowContentTarget = target;
                    await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MainFlipView.Items.Add(target);
                    });
                    await OverflowAsync(target);
                }

                else if (!rtb.HasOverflowContent && rtb.OverflowContentTarget != null)
                {
                    await UnderflowAsync(rtb.OverflowContentTarget);
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

        private async Task UnderflowAsync(RichTextBlockOverflow rtbo)
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

        private async void MainFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsBusy)
                return;

            if (MainFlipView.SelectedIndex == MainFlipView.Items.Count - 1)
            {
                // We're on the last page.
                var nextIndex = Source.Spine.IndexOf(_currentSpineItem) + 1;

                if (nextIndex > Source.Spine.Count - 1)
                    return;

                if (!(MainFlipView.SelectedItem is FlipViewItem))
                {
                    // Tell the FlipView it can go further.
                    MainFlipView.Items.Add(new FlipViewItem());
                }

                else
                {
                    // Load the next spine item.
                    await LoadContentAsync(Source.Spine[nextIndex]);
                }
            }

            else if (MainFlipView.SelectedIndex == 0)
            {
                // We're on the first page.
                var previousIndex = Source.Spine.IndexOf(_currentSpineItem) - 1;

                if (previousIndex < 0)
                    return;

                if (!(MainFlipView.SelectedItem is FlipViewItem))
                {
                    // Tell the FlipView it can go further.
                    MainFlipView.Items.Insert(0, new FlipViewItem());
                }

                else
                {
                    // Load the previous spine item.
                    await LoadContentAsync(Source.Spine[previousIndex]);
                    MainFlipView.SelectedItem = MainFlipView.Items.LastOrDefault();     // TODO: This get called before all pages have finished overflowing.
                }
            }
        }
    }
}
