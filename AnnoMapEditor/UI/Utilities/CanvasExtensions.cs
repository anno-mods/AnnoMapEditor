using AnnoMapEditor.MapTemplates;
using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Utilities
{
    public static class CanvasExtensions
    {
        public static void SetPosition(this UIElement that, Vector2 position)
        {
            Canvas.SetLeft(that, position.X);
            Canvas.SetTop(that, position.Y);
        }

        public static Vector2 GetPosition(this UIElement that)
        {
            return new Vector2((int)Canvas.GetLeft(that), (int)Canvas.GetTop(that));
        }
    }
}
