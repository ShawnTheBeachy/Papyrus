using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;

namespace Papyrus.UI
{
    public class PageProvider : ObservableCollection<UIElement>
    {
        private PageProviderBindings _bindings;

        public void CreateFromBlocks(IEnumerable<Block> blocks, PageProviderBindings bindings, bool canGoBack, bool canGoForward)
        {
            _bindings = bindings;

            if (canGoBack)
                Add(new FlipViewItem());

            var baseBlock = new RichTextBlock
            {
                IsTextSelectionEnabled = false
            };
            baseBlock.SetBinding(RichTextBlock.LineHeightProperty, _bindings.LineHeightBinding);
            baseBlock.SetBinding(RichTextBlock.PaddingProperty, _bindings.PaddingBinding);
            baseBlock.SetBinding(RichTextBlock.TextIndentProperty, _bindings.ParagraphIndentationBinding);

            foreach (var block in blocks)
                baseBlock.Blocks.Add(block);

            Add(baseBlock);
            baseBlock.UpdateLayout();
            Overflow(baseBlock);

            if (canGoForward)
                Add(new FlipViewItem());
        }

        private void Overflow(RichTextBlock rtb)
        {
            void Flow()
            {
                if (rtb.HasOverflowContent && rtb.OverflowContentTarget == null)
                {
                    var index = IndexOf(rtb);
                    var target = new RichTextBlockOverflow();
                    target.SetBinding(RichTextBlockOverflow.PaddingProperty, _bindings.PaddingBinding);
                    rtb.OverflowContentTarget = target;
                    Insert(index + 1, target);
                    target.UpdateLayout();
                    Overflow(target);
                }

                else if (!rtb.HasOverflowContent && rtb.OverflowContentTarget != null)
                {
                    Remove(rtb.OverflowContentTarget);
                    rtb.OverflowContentTarget = null;
                }
            }

            rtb.SizeChanged += (s, e) =>
            {
                Flow();
            };

            Flow();
            // rtb.RegisterPropertyChangedCallback(RichTextBlock.HasOverflowContentProperty, HasOverflowContentPropertyChanged);
        }

        private void Overflow(RichTextBlockOverflow rtbo)
        {
            void Flow()
            {
                if (rtbo.HasOverflowContent && rtbo.OverflowContentTarget == null)
                {
                    var index = IndexOf(rtbo);
                    var target = new RichTextBlockOverflow();
                    target.SetBinding(RichTextBlockOverflow.PaddingProperty, _bindings.PaddingBinding);
                    rtbo.OverflowContentTarget = target;
                    Insert(index + 1, rtbo.OverflowContentTarget);
                    target.UpdateLayout();
                    Overflow(target);
                }

                else if (!rtbo.HasOverflowContent && rtbo.OverflowContentTarget != null)
                {
                    Remove(rtbo.OverflowContentTarget);
                    rtbo.OverflowContentTarget = null;
                }
            }

            rtbo.SizeChanged += (s, e) =>
            {
                Flow();
            };

            Flow();
            // rtbo.RegisterPropertyChangedCallback(RichTextBlockOverflow.HasOverflowContentProperty, HasOverflowContentPropertyChanged);
        }
    }

    public class PageProviderBindings
    {
        public Binding LineHeightBinding { get; set; }
        public Binding PaddingBinding { get; set; }
        public Binding ParagraphIndentationBinding { get; set; }
    }
}
