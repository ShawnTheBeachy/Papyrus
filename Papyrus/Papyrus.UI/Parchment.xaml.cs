using Papyrus.Extensions;
using Papyrus.HtmlParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
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
        public PageProvider Provider = new PageProvider();
        private SpineItem _currentSpineItem;
        private Binding _paddingBinding, _lineHeightBinding, _indentationBinding, _foregroundBinding, _characterSpacingBinding;
        private PageProviderBindings _bindings = new PageProviderBindings();
        private HtmlParser.HtmlParser _converter = new HtmlParser.HtmlParser();

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
            _characterSpacingBinding = new Binding
            {
                FallbackValue = CharacterSpacing,
                TargetNullValue = CharacterSpacing,
                Source = this,
                Path = new PropertyPath("CharacterSpacing"),
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
                Path = new PropertyPath("ParagraphIndentation"),
                Mode = BindingMode.OneWay
            };
            _bindings.LineHeightBinding = _lineHeightBinding;
            _bindings.PaddingBinding = _paddingBinding;
            _bindings.ParagraphIndentationBinding = _indentationBinding;
        }

        public async Task LoadContentAsync(NavPoint navPoint)
        {
            var manifestItem = Source.Manifest.FirstOrDefault(a => Path.GetFileName(a.Value.ContentLocation) == Path.GetFileName(navPoint.ContentPath));
            _currentSpineItem = Source.Spine.FirstOrDefault(a => a.IdRef == manifestItem.Key);

            var contents = await Source.GetContentsAsync(navPoint);
            var stylesheetLocations = GetStylesheetLocations(contents, navPoint.ContentPath);
            var css = string.Empty;

            foreach (var location in stylesheetLocations)
            {
                var file = await Source.GetFileAsync(location);
                css += $"\r{await FileIO.ReadTextAsync(file)}";
            }
            
            _converter.Convert(contents, css, new CssOptions { Foreground = false, LineHeight = false, TextIndent = false });
        }

        public async Task LoadContentAsync(SpineItem spineItem)
        {
            _currentSpineItem = spineItem;
            var contents = await Source.GetContentsAsync(spineItem);

            var stylesheetLocations = GetStylesheetLocations(contents, Source.Manifest[spineItem.IdRef].ContentLocation);
            var css = string.Empty;

            foreach (var location in stylesheetLocations)
            {
                var file = await Source.GetFileAsync(location);
                css += $"\r{await FileIO.ReadTextAsync(file)}";
            }
            
            _converter.Convert(contents, css, new CssOptions { Foreground = false, LineHeight = false, TextIndent = false });
        }

        private IEnumerable<string> GetStylesheetLocations(string html, string relativePath)
        {
            var stylesheetLocations = new List<string>();

            var stylesheetsRegex = new Regex(@"<link.*rel=""stylesheet"".*>");
            var hrefRegex = new Regex(@"href=""([^""]+)");
            var stylesheetMatches = stylesheetsRegex.Matches(html).OfType<Match>();

            foreach (var match in stylesheetMatches)
            {
                if (!match.Value.Contains(@"type=""text/css"""))
                    break;

                var href = hrefRegex.Match(match.Value).Groups.OfType<Group>().ElementAt(1);
                var contentLocation = Path.GetFullPath(Source.RootPath.EnsureEnd("\\") + Path.GetDirectoryName(relativePath));
                var stylesheetLocation = Path.GetFullPath(contentLocation.EnsureEnd("\\") + href);
                stylesheetLocations.Add(stylesheetLocation);
            }

            return stylesheetLocations;
        }

        public void BuildView()
        {
            Provider.Clear();

            var previousItem = Source.Spine.Previous(_currentSpineItem);
            var nextItem = Source.Spine.Next(_currentSpineItem);

            Provider.CreateFromBlocks(_converter.ConvertedBlocks.ToList(), _bindings, previousItem != null, nextItem != null);
        }
        
        private async void MainFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsBusy)
                return;

            if (MainFlipView.SelectedIndex == MainFlipView.Items.Count - 1)
            {
                // We're on the last page.
                
                if (MainFlipView.SelectedItem is FlipViewItem)
                {
                    IsBusy = true;
                    await LoadContentAsync(Source.Spine.Next(_currentSpineItem));
                    BuildView();
                    MainFlipView.SelectedIndex = 1;
                    IsBusy = false;
                }
            }

            else if (MainFlipView.SelectedIndex == 0)
            {
                // We're on the first page.
                var previousIndex = Source.Spine.IndexOf(_currentSpineItem) - 1;
                
                if (MainFlipView.SelectedItem is FlipViewItem)
                {
                    // Load the previous spine item.
                    IsBusy = true;
                    await LoadContentAsync(Source.Spine.Previous(_currentSpineItem));
                    BuildView();
                    MainFlipView.SelectedIndex = MainFlipView.Items.Count - 2;
                    IsBusy = false;
                }
            }
        }

        public void SelectPage(int index)
        {
            MainFlipView.SelectedIndex = index;
        }
    }
}
