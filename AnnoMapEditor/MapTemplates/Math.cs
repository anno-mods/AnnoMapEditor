using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates
{
    public struct Vector2
    {
        public int X;
        public int Y;

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
                    return;
                }
            }

            X = 0;
            Y = 0;
        }

        public Vector2(byte[]? data)
        {
            if (data is not null && data.Length == 8)
            {
                X = BitConverter.ToInt32(new ReadOnlySpan<byte>(data, 0, 4));
                Y = BitConverter.ToInt32(new ReadOnlySpan<byte>(data, 4, 4));
                return;
            }

            X = 0;
            Y = 0;
        }
    }

    public struct Rect2
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public Rect2(string? area)
        {
            if (area is not null)
            {
                string[] parts = area.Split(' ');
                if (parts.Length == 4)
                {
                    X = int.Parse(parts[0]);
                    Y = int.Parse(parts[1]);
                    Width = int.Parse(parts[2]) - X;
                    Height = int.Parse(parts[3]) - Y;
                    return;
                }
            }

            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
        }

        public Rect2(byte[]? data)
        {
            if (data is not null && data.Length == 16)
            {
                X = BitConverter.ToInt32(new ReadOnlySpan<byte>(data, 0, 4));
                Y = BitConverter.ToInt32(new ReadOnlySpan<byte>(data, 4, 4));
                Width = BitConverter.ToInt32(new ReadOnlySpan<byte>(data, 8, 4)) - X;
                Height = BitConverter.ToInt32(new ReadOnlySpan<byte>(data, 12, 4)) - Y;
                return;
            }

            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
        }
    }
}
