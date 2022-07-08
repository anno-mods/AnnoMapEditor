using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates
{
    public struct Region
    {
        #region Region enums
        public static readonly Region Moderate = new("Moderate", "Moderate",
            new()
            {
                [IslandSize.Small] = new Pool("data/sessions/islands/pool/moderate/moderate_s_{0}/moderate_s_{0}.a7m", 12),
                [IslandSize.Medium] = new Pool("data/sessions/islands/pool/moderate/moderate_m_{0}/moderate_m_{0}.a7m", 9),
                [IslandSize.Large] = new Pool("data/sessions/islands/pool/moderate/moderate_l_{0}/moderate_l_{0}.a7m", 14)
            });
        public static readonly Region NewWorld = new("NewWorld", "New World",
            new()
            {
                [IslandSize.Small] = new Pool("data/sessions/islands/pool/colony01/colony01_s_{0}/colony01_s_{0}.a7m", 4),
                [IslandSize.Medium] = new Pool("data/sessions/islands/pool/colony01/colony01_m_{0}/colony01_m_{0}.a7m", 6),
                [IslandSize.Large] = new Pool("data/sessions/islands/pool/colony01/colony01_l_{0}/colony01_l_{0}.a7m", 5)
            });
        public static readonly Region Arctic = new("Arctic", "Arctic",
            new()
            {
                [IslandSize.Small] = new Pool("data/dlc03/sessions/islands/pool/colony03_a01_{0}/colony03_a01_{0}.a7m", 8),
                [IslandSize.Medium] = new Pool("data/dlc03/sessions/islands/pool/colony03_a02_{0}/colony03_a02_{0}.a7m", 4),
                [IslandSize.Large] = new Pool("data/dlc03/sessions/islands/pool/moderate/moderate_l_{0}/moderate_l_{0}.a7m", 14)
            });
        public static readonly Region Enbesa = new("Enbesa", "Enbesa",
            new()
            {
                [IslandSize.Small] = new Pool("data/dlc06/sessions/islands/pool/colony02_s_{0}/colony02_s_{0}.a7m", new int[] { 1, 2, 3, 5 }),
                [IslandSize.Medium] = new Pool("data/dlc06/sessions/islands/pool/colony02_m_{0}/colony02_m_{0}.a7m", new int[] { 2, 4, 5, 9 }),
                [IslandSize.Large] = new Pool("data/dlc06/sessions/islands/pool/colony02_l_{0}/colony02_l_{0}.a7m", new int[] { 1, 3, 5, 6 })
            });
        public static readonly Region[] All = new Region[] { Moderate, NewWorld, Arctic, Enbesa };
        #endregion

        public string Name { get; init; }

        private static readonly Random rnd = new((int)DateTime.Now.Ticks);

        private readonly string value;

        #region Pool Islands
        public struct Pool
        {
            public string filePath;
            public int size;
            public int[]? ids;

            public Pool(string filePath, int size)
            {
                this.filePath = filePath;
                this.size = size;
                ids = null;
            }

            public Pool(string filePath, int[] ids)
            {
                this.filePath = filePath;
                size = ids.Length;
                this.ids = ids;
            }
        }
        public Dictionary<IslandSize, Pool> PoolIslands { get; private init; }
        #endregion

        private Region(string type, string name, Dictionary<IslandSize, Pool> poolIslands)
        {
            value = type;
            Name = name;
            PoolIslands = poolIslands;
        }

        public string GetRandomIslandPath(IslandSize size)
        {
            int index = rnd.Next(1, PoolIslands[size].size);
            var ids = PoolIslands[size].ids;
            if (ids is not null)
                index = ids[index];

            return string.Format(PoolIslands[size].filePath, string.Format("{0:00}", index));
        }

        public override string ToString() => value;

        public override bool Equals(object? obj) => obj is Region other && value.Equals(other.value);
        public override int GetHashCode() => value.GetHashCode();

        public static bool operator !=(Region a, Region b) => !a.Equals(b);
        public static bool operator ==(Region a, Region b) => a.Equals(b);
    }
}
