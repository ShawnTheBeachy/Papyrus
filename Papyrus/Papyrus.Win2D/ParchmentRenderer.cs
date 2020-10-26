using HtmlAgilityPack;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Papyrus.Win2D.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI;
using Windows.UI.Text;

namespace Papyrus.Win2D
{
    public sealed class ParchmentRenderer
    {
        private readonly CanvasControl _canvas;
        private CanvasDrawingSession _drawingSession;
        private int? _nextRender = null;
        private readonly IList<NodeElement> _nodes =
            new List<NodeElement>();
        private readonly RenderingConfiguration _renderConfig;

        public ParchmentRenderer (CanvasControl canvas,
                                  string html,
                                  RenderingConfiguration renderingConfiguration)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            _canvas.Draw += Canvas_Draw;
            ForceCanvasDraw();
            _renderConfig = renderingConfiguration;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var baseNode = doc.DocumentNode.Element("body") ?? doc.DocumentNode;
            FlattenNodes(baseNode);
        }

        private void Canvas_Draw (CanvasControl sender, CanvasDrawEventArgs args)
        {
            _drawingSession = args.DrawingSession;
            var nextPosition = new Vector2();
            int i;

            for (i = _nextRender ?? 0; nextPosition.Y < (float)_canvas.ActualHeight && i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                nextPosition = DrawNode(node, nextPosition, null);
            }
        }

        private static string CollapseWhitespace (string value)
        {
            do
            {
                value = value.Replace("  ", " ");
            } while (value.Contains("  "));

            return value;
        }

        private Vector2 DrawNode (NodeElement node,
                                  Vector2 position,
                                  Color? color)
        {
            var format = new CanvasTextFormat
            {
                FontSize = node.FontSize,
                FontStyle = node.FontStyle,
                HorizontalAlignment = CanvasHorizontalAlignment.Justified,
                WordWrapping = CanvasWordWrapping.WholeWord
            };
            var singleLineLayout = new CanvasTextLayout(_drawingSession,
                node.Text,
                new CanvasTextFormat
                {
                    FontSize = node.FontSize,
                    FontStyle = node.FontStyle,
                    WordWrapping = CanvasWordWrapping.NoWrap
                }, 0.0f, 0.0f);
            var textLayout = new CanvasTextLayout(_drawingSession, 
                node.Text, 
                format, 
                (float)_canvas.ActualWidth - position.X, 
                0.0f);
            var nextPosition = position + new Vector2(
                (float)textLayout.LayoutBounds.Width,
                textLayout.LayoutBounds.Height > singleLineLayout.LayoutBounds.Height ?
                    (float)textLayout.LayoutBounds.Height - (float)singleLineLayout.LayoutBounds.Height :
                    position.Y);
            _drawingSession.DrawTextLayout(textLayout, position, color ?? Colors.Black);
            return nextPosition;
        }

        private void FlattenNodes (HtmlNode node,
                                   float fontSize = 14.0f,
                                   FontStyle fontStyle = FontStyle.Normal,
                                   float marginBottom = 0,
                                   float marginTop = 0)
        {
            foreach (var child in node.ChildNodes)
            {
                var childFontSize = fontSize;
                var childFontStyle = fontStyle;
                var childMarginBottom = marginBottom;
                var childMarginTop = marginTop;

                switch (child.Name)
                {
                    case "h1":
                        childFontSize = 24.0f;
                        childMarginBottom = 1.0f;
                        break;
                    case "i":
                        childFontStyle = FontStyle.Italic;
                        break;
                    case "p":
                        childMarginTop = _renderConfig.ParagraphSpacing;
                        break;
                    case "#text":
                        var text = CollapseWhitespace(child.InnerText.Trim('\t', '\r', '\n'));

                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            var nodeElement = new NodeElement
                            {
                                FontSize = childFontSize,
                                FontStyle = childFontStyle,
                                MarginBottom = childMarginBottom,
                                MarginTop = childMarginTop,
                                Text = text
                            };
                            _nodes.Add(nodeElement);
                        }

                        break;
                }

                FlattenNodes(child,
                    childFontSize, 
                    childFontStyle, 
                    childMarginBottom, 
                    childMarginTop);
            }
        }

        private void ForceCanvasDraw () => _canvas.Invalidate();

        public void RenderFrom (int? lastExcludedIndex)
        {
            _nextRender = lastExcludedIndex;
            ForceCanvasDraw();
        }
    }
}
