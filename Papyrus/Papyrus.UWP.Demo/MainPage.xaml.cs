using Papyrus.Win2D;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Papyrus.UWP.Demo
{
    public sealed partial class MainPage : Page
    {
        private readonly string _html =
@"
<html>
    <body>
        <h1>Title here</h1>
        <p>Here lies a paragraph. It's pretty long so that we can try out <i>wrapping</i> and see if it works.</p>
    </body>
</html>
";
        private readonly ParchmentRenderer _renderer;

        public MainPage ()
        {
            InitializeComponent();
            _renderer = new ParchmentRenderer(MainCanvas, _html, new RenderingConfiguration
            {
                ParagraphSpacing = 5.0f
            });
        }

        private void Page_Unloaded (object sender, RoutedEventArgs e)
        {
            MainCanvas.RemoveFromVisualTree();
            MainCanvas = null;
        }

        private void MainCanvas_SizeChanged (object sender, SizeChangedEventArgs e) =>
             _renderer.RenderFrom(null);
    }
}
