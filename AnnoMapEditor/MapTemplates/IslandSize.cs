using System;

namespace AnnoMapEditor.MapTemplates
{
    public struct IslandSize
    {
        public static readonly IslandSize Small = new("Small");
        public static readonly IslandSize Medium = new("Medium");
        public static readonly IslandSize Large = new("Large");

        private readonly string value;

        public int InTiles { get; private init; }
        public bool IsDefault { get; private init; }

        public IslandSize(string? size)
        {
            if (size == "Medium" || size == "Large")
            {
                value = size;
                InTiles = size == "Medium" ? 320 : 384;
                IsDefault = false;
                return;
            }

            value = "Small";
            InTiles = 128;
            IsDefault = size != "Small";
        }

        public IslandSize(byte[]? data)
        {
            if (data is not null && data.Length == 2)
            {
                int val = BitConverter.ToInt16(data);
                if (val >= 0 && val < 3)
                {
                    if (val == 0)
                    {
                        value = "Small";
                        InTiles = 128;
                    }
                    else if (val == 1)
                    {
                        value = "Medium";
                        InTiles = 320;
                    }
                    else
                    {
                        value = "Large";
                        InTiles = 384;
                    }

                    IsDefault = false;
                    return;
                }
            }

            value = "Small";
            InTiles = 128;
            IsDefault = true;
        }

        public override string ToString() => value;

        public override bool Equals(object? obj) => obj is IslandSize other && value.Equals(other.value);
        public override int GetHashCode() => value.GetHashCode();

        public static bool operator !=(IslandSize a, IslandSize b) => !a.Equals(b);
        public static bool operator ==(IslandSize a, IslandSize b) => a.Equals(b);
    }
}
