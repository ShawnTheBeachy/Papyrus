using Windows.UI.Text;

namespace Papyrus.Win2D.Models
{
    public sealed class NodeElement
    {
        public float FontSize { get; set; }
        public FontStyle FontStyle { get; set; }
        public float MarginBottom { get; set; }
        public float MarginTop { get; set; }
        public string Text { get; set; }
    }
}
