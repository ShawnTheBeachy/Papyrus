//using Papyrus.Win2D;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Papyrus.Win2DDemo
{
    public sealed partial class MainPage : Page
    {
        private readonly string _html =
@"
<html>
    <body>
        <h1>Title here</h1>
        <p>Here lies a paragraph.</p>
    </body>
</html>
";

        public MainPage()
        {
            InitializeComponent();
            //var renderer = new ParchmentRenderer(MainCanvas, _html);
            //renderer.RenderFrom(null);
        }

        private void Page_Unloaded (object sender, RoutedEventArgs e)
        {
            MainCanvas.RemoveFromVisualTree();
            MainCanvas = null;
        }
    }
}
