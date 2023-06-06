using System.Windows;

namespace AnnoMapEditor.Utilities
{
    public static class PointExtensions
    {
        public static bool Within(this Point point, Rect2 area)
        {
            return point.X >= area.X && point.Y >= area.Y && point.X < area.X + area.Width && point.Y < area.Y + area.Height;
        }
    }
}
