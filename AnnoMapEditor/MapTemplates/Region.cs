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
        //Technically, Cape Trelawney is in Moderate Region but has ambientName "Moderate_01_day_night_st",
        //but we don't allow Mod exports for that anyways
        public static readonly Region Moderate = new("Moderate", "Moderate", "Moderate_01_day_night", 
            new()
            {
                [IslandSize.Small] = new Pool("data/sessions/islands/pool/moderate/moderate_s_{0}/moderate_s_{0}.a7m", 12),
                [IslandSize.Medium] = new Pool("data/sessions/islands/pool/moderate/moderate_m_{0}/moderate_m_{0}.a7m", 9),
                [IslandSize.Large] = new Pool(
                    new FilePathRange[]
                    {
                        new FilePathRange("data/sessions/islands/pool/moderate/moderate_l_{0}/moderate_l_{0}.a7m", 1, 14),
                        new FilePathRange("data/sessions/islands/pool/moderate/community_island/community_island.a7m", 1, 1)
                    })
            });
        public static readonly Region NewWorld = new("NewWorld", "New World", "south_america_caribic_01",
            new()
            {
                [IslandSize.Small] = new Pool(
                    new FilePathRange[]
                    {
                        new FilePathRange("data/sessions/islands/pool/colony01/colony01_s_{0}/colony01_s_{0}.a7m", 1, 4),
                        new FilePathRange("data/dlc12/sessions/islands/pool/colony01/colony01_s_{0}/colony01_s_{0}.a7m", 5, 3)
                    }),
                [IslandSize.Medium] = new Pool(
                    new FilePathRange[]
                    {
                        new FilePathRange("data/sessions/islands/pool/colony01/colony01_m_{0}/colony01_m_{0}.a7m", 1, 6),
                        new FilePathRange("data/dlc12/sessions/islands/pool/colony01/colony01_m_{0}/colony01_m_{0}.a7m", 7, 3)
                    }),
                [IslandSize.Large] = new Pool(
                    new FilePathRange[]
                    {
                        new FilePathRange("data/sessions/islands/pool/colony01/colony01_l_{0}/colony01_l_{0}.a7m", 1, 5),
                        new FilePathRange("data/dlc12/sessions/islands/pool/colony01/colony01_l_{0}/colony01_l_{0}.a7m", 6, 3),

                    })
            });
        public static readonly Region Arctic = new("Arctic", "Arctic", "DLC03_01",
            new()
            {
                [IslandSize.Small] = new Pool("data/dlc03/sessions/islands/pool/colony03_a01_{0}/colony03_a01_{0}.a7m", 8),
                [IslandSize.Medium] = new Pool("data/dlc03/sessions/islands/pool/colony03_a02_{0}/colony03_a02_{0}.a7m", 4),
                [IslandSize.Large] = new Pool("data/dlc03/sessions/islands/pool/moderate/moderate_l_{0}/moderate_l_{0}.a7m", 14)
            });
        public static readonly Region Enbesa = new("Enbesa", "Enbesa", "Colony_02",
            new()
            {
                [IslandSize.Small] = new Pool("data/dlc06/sessions/islands/pool/colony02_s_{0}/colony02_s_{0}.a7m", new int[] { 1, 2, 3, 5 }),
                [IslandSize.Medium] = new Pool("data/dlc06/sessions/islands/pool/colony02_m_{0}/colony02_m_{0}.a7m", new int[] { 2, 4, 5, 9 }),
                [IslandSize.Large] = new Pool("data/dlc06/sessions/islands/pool/colony02_l_{0}/colony02_l_{0}.a7m", new int[] { 1, 3, 5, 6 })
            });
        public static readonly Region[] All = new Region[] { Moderate, NewWorld, Arctic, Enbesa };
        #endregion

        public string Name { get; init; }
        public string AmbientName { get; init; }

        private static readonly Random rnd = new((int)DateTime.Now.Ticks);

        private readonly string value;

        #region Pool Islands
        public struct Pool
        {
            public FilePathRange[] paths { get; init; }
            public int size
            {
                get
                {
                    int sum = 0;
                    foreach(var path in paths)
                    {
                        sum += path.size;
                    }
                    return sum;
                }
            }

            public string GetPath(int i)
            {
                int rangeIdx = 0;
                FilePathRange range = paths[rangeIdx];
                int skipped = 0;
                while(skipped + range.size <= i)
                {
                    skipped += range.size;
                    range = paths[++rangeIdx];
                }

                return range.GetPath(i - skipped);
            }

            public Pool(string filePath, int size)
            {
                this.paths = new FilePathRange[]
                {
                    new FilePathRange(filePath, 1, size)
                };
            }

            public Pool(string filePath, int[] ids)
            {
                this.paths = new FilePathRange[]
                {
                    new FilePathRange(filePath, ids)
                };
            }

            public Pool(FilePathRange[] paths)
            {
                this.paths = paths;
            }
        }

        public struct FilePathRange
        {
            public string filePath;
            public int size;
            public int[] ids;

            public string GetPath(int i)
            {
                return string.Format(filePath, string.Format("{0:00}", ids[i]));
            }

            public FilePathRange(string filePath, int start, int count)
            {
                this.filePath = filePath;
                this.size = count;
                this.ids = Enumerable.Range(start, count).ToArray();
            }

            public FilePathRange(string filePath, int[] ids)
            {
                this.filePath = filePath;
                this.size = ids.Length;
                this.ids = ids;
            }
        }
        public Dictionary<IslandSize, Pool> PoolIslands { get; private init; }
        #endregion

        private Region(string type, string name, string ambientName, Dictionary<IslandSize, Pool> poolIslands)
        {
            value = type;
            Name = name;
            AmbientName = ambientName;
            PoolIslands = poolIslands;
        }

        public string GetRandomIslandPath(IslandSize size)
        {
            int index = rnd.Next(1, PoolIslands[size].size);

            string path = PoolIslands[size].GetPath(index);
            return path;
        }

        public override string ToString() => value;

        public override bool Equals(object? obj) => obj is Region other && value.Equals(other.value);
        public override int GetHashCode() => value.GetHashCode();

        public static bool operator !=(Region a, Region b) => !a.Equals(b);
        public static bool operator ==(Region a, Region b) => a.Equals(b);
    }
}
