using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates
{
    public partial struct Region
    {
        #region Region enums
        //Technically, Cape Trelawney is in Moderate Region but has ambientName "Moderate_01_day_night_st",
        //but we don't allow Mod exports for that anyways
        public static readonly Region Moderate = new("Moderate", "Moderate", "Moderate_01_day_night", allowModding:true, "moderate",
            new[] { "ll", "lm", "ls", "ml", "mm", "ms", "sl", "sm", "ss" },
            new[] { "01", "02" },
            usesAllSizeIndices:false, hasMapExtension:false,
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

        public static readonly Region NewWorld = new("NewWorld", "New World", "south_america_caribic_01", allowModding: true, "colony01", 
            new[] { "s", "m", "l"},
            new[] { "01", "02", "03" },
            usesAllSizeIndices: true, hasMapExtension: true,
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

        //poolFolderName is manually selected, the game files don't have a special one for the arctic as it only has one map
        public static readonly Region Arctic = new("Arctic", "Arctic", "DLC03_01", allowModding: false, poolFolderName:"colony03",
            new[] { "sp" },
            new[] { "" },
            usesAllSizeIndices: true, hasMapExtension: false,
            new()
            {
                [IslandSize.Small] = new Pool("data/dlc03/sessions/islands/pool/colony03_a01_{0}/colony03_a01_{0}.a7m", 8),
                [IslandSize.Medium] = new Pool("data/dlc03/sessions/islands/pool/colony03_a02_{0}/colony03_a02_{0}.a7m", 4),
                [IslandSize.Large] = new Pool("data/dlc03/sessions/islands/pool/moderate/moderate_l_{0}/moderate_l_{0}.a7m", 14)
            });

        public static readonly Region Enbesa = new("Enbesa", "Enbesa", "Colony_02", allowModding: false, "land_of_lions",
            new[] { "01" },
            new[] { "", "mp" },
            usesAllSizeIndices: true, hasMapExtension: false,
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
        public bool AllowModding { get; init; }

        public string PoolFolderName { get; init; }
        public IReadOnlyCollection<string> MapSizes { get; init; }
        public IReadOnlyCollection<string> MapSizeIndices { get; init; }

        public bool UsesAllSizeIndices { get; init; }
        public bool HasMapExtension { get; init; }

        private static readonly Random rnd = new((int)DateTime.Now.Ticks);

        private readonly string value;


        public Dictionary<IslandSize, Pool> PoolIslands { get; private init; }


        private Region(string type, string name, string ambientName, bool allowModding, string poolFolderName, 
            string[] mapSizes, string[] sizeIndices, bool usesAllSizeIndices, bool hasMapExtension, Dictionary<IslandSize, Pool> poolIslands)
        {
            value = type;
            Name = name;
            AmbientName = ambientName;
            AllowModding = allowModding;

            PoolFolderName = poolFolderName;
            MapSizes = new ReadOnlyCollection<string>(mapSizes);
            MapSizeIndices = new ReadOnlyCollection<string>(sizeIndices);

            UsesAllSizeIndices = usesAllSizeIndices;
            HasMapExtension = hasMapExtension;

            PoolIslands = poolIslands;
        }

        public string GetRandomIslandPath(IslandSize size)
        {
            // use a random Small island for IslandSize.Default
            if (size == IslandSize.Default)
                size = IslandSize.Small;

            int index = rnd.Next(1, PoolIslands[size].size);

            string path = PoolIslands[size].GetPath(index);
            return path;
        }

        public IEnumerable<string> GetAllSizeCombinations()
        {
            if (UsesAllSizeIndices)
            {
                foreach (string size in MapSizes)
                {
                    foreach (string subsize in MapSizeIndices)
                    {
                        string result = size + (string.IsNullOrEmpty(size) || string.IsNullOrEmpty(subsize) ? "" : "_") + subsize;
                        yield return result;
                    }
                }
            }
            else
            {
                foreach (string size in MapSizes)
                {
                    yield return size;
                }
            }
        }

        public override string ToString() => value;

        public override bool Equals(object? obj) => obj is Region other && value.Equals(other.value);
        public override int GetHashCode() => value.GetHashCode();

        public static bool operator !=(Region a, Region b) => !a.Equals(b);
        public static bool operator ==(Region a, Region b) => a.Equals(b);
    }
}
