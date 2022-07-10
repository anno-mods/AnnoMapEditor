using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoMapEditor.MapTemplates
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

        public Vector2 Clamp(Rect2 area)
        {
            return new Vector2(Math.Clamp(X, area.X, area.Max.X), Math.Clamp(Y, area.Y, area.Max.Y));
        }

        public Vector2 Clamp(Vector2 min, Vector2 max)
        {
            return new Vector2(Math.Clamp(X, min.X, max.X), Math.Clamp(Y, min.Y, max.Y));
        }

        public Vector2 FlipY(int sessionSize)
        {
            return new Vector2(X, sessionSize - Y - 8);
        }

        public bool Within(Rect2 area)
        {
            return X >= area.X && Y >= area.Y && X < area.X + area.Width && Y < area.Y + area.Height;
        }

        private static int Normalize(int x) => (x + 4) / 8 * 8;
    }

    public struct Rect2
    {
        public Vector2 Position;
        public Vector2 Size;
        public Vector2 Max => Position + Size - Vector2.Tile;

        public int X => Position.X;
        public int Y => Position.Y;
        public int Width => Size.X;
        public int Height => Size.Y;

        public Rect2(string? area)
        {
            if (area is not null)
            {
                string[] parts = area.Split(' ');
                if (parts.Length == 4)
                {
                    Position = new Vector2(int.Parse(parts[0]), int.Parse(parts[1]));
                    Size = new Vector2(int.Parse(parts[2]), int.Parse(parts[3])) - Position;
                    return;
                }
            }

            Position = Vector2.Zero;
            Size = Vector2.Zero;
        }

        public Rect2(int[]? numbers)
        {
            if (numbers?.Length == 4)
            {
                Position = new Vector2(numbers);
                Size = new Vector2(numbers[2], numbers[3]) - Position;
                return;
            }

            Position = Vector2.Zero;
            Size = Vector2.Zero;
        }

        public Rect2(Vector2 max)
        {
            Position = Vector2.Zero;
            Size = max - Position;
        }

        public Rect2(Vector2 min, Vector2 max)
        {
            Position = min;
            Size = max - Position;
        }
    }
}
