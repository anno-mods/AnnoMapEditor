using System;
using System.Windows;

namespace AnnoMapEditor.Utilities
{
    public record Vector2
    {
        public static readonly Vector2 Zero = new(0, 0);
        public static readonly Vector2 Tile = new(8, 8);

        public int X 
        {
            get => _x;
            set => _x = Normalize(value);
        }
        private int _x = 0;

        public int Y
        {
            get => _y;
            set => _y = Normalize(value);
        }
        private int _y = 0;

        public double Length => Math.Sqrt(X * X + Y * Y);

        public Vector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector2(string? position)
        {
            if (position is not null)
            {
                string[] parts = position.Split(' ');
                if (parts.Length == 2)
                {
                    X = int.Parse(parts[0]);
                    Y = int.Parse(parts[1]);
                }
            }
        }

        public Vector2(int[]? numbers)
        {
            if (numbers?.Length >= 2)
            {
                X = numbers[0];
                Y = numbers[1];
            }
        }

        public Vector2(Point point)
        {
            X = (int)point.X;
            Y = (int)point.Y;
        }

        public Vector2(Vector2 vector2)
        {
            X = vector2.X;
            Y = vector2.Y;
        }

        public static Vector2 operator + (Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator -(Vector2 a, int b)
        {
            return new Vector2(a.X - b, a.Y - b);
        }

        public static bool Equals(Vector2 a, Vector2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public Vector2 Clamp(Rect2 area)
        {
            return new Vector2(Math.Clamp(X, area.X, area.Max.X - Tile.X), Math.Clamp(Y, area.Y, area.Max.Y - Tile.Y));
        }

        public Vector2 Clamp(Vector2 min, Vector2 max)
        {
            return new Vector2(Math.Clamp(X, min.X, max.X), Math.Clamp(Y, min.Y, max.Y));
        }

        public Vector2 FlipYItem(int mapTemplateSizeY, int itemSizeY)
        {
            return new Vector2(X, mapTemplateSizeY - itemSizeY - Y);
        }

        public bool Within(Rect2 area)
        {
            return X >= area.X && Y >= area.Y && X < area.X + area.Width && Y < area.Y + area.Height;
        }

        private static int Normalize(int x) => (x + 4) / 8 * 8;
    }
}
