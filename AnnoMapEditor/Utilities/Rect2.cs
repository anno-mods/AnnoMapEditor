namespace AnnoMapEditor.Utilities
{
    public struct Rect2
    {
        public Vector2 Position;
        public Vector2 Size;
        public Vector2 Max => Position + Size;

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

        public Rect2(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }
    }
}
