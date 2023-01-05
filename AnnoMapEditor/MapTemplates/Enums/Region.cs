using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AnnoMapEditor.MapTemplates
{
    public class Region
    {
        //Technically, Cape Trelawney is in Moderate Region but has ambientName "Moderate_01_day_night_st",
        //but we don't allow Mod exports for that anyways
        public static readonly Region Moderate = new("Moderate", "Moderate_01_day_night", allowModding:true, "moderate",
            new[] { "ll", "lm", "ls", "ml", "mm", "ms", "sl", "sm", "ss" },
            new[] { "01", "02" },
            usesAllSizeIndices:false, hasMapExtension:false);

        public static readonly Region NewWorld = new("New World", "south_america_caribic_01", allowModding: true, "colony01", 
            new[] { "s", "m", "l"},
            new[] { "01", "02", "03" },
            usesAllSizeIndices: true, hasMapExtension: true);

        //poolFolderName is manually selected, the game files don't have a special one for the arctic as it only has one map
        public static readonly Region Arctic = new("Arctic", "DLC03_01", allowModding: false, poolFolderName:"colony03",
            new[] { "sp" },
            new[] { "" },
            usesAllSizeIndices: true, hasMapExtension: false);

        public static readonly Region Enbesa = new("Enbesa", "Colony_02", allowModding: false, "land_of_lions",
            new[] { "01" },
            new[] { "", "mp" },
            usesAllSizeIndices: true, hasMapExtension: false);

        public static readonly Region[] All = new Region[] { Moderate, NewWorld, Arctic, Enbesa };


        public string Name { get; init; }

        public string AmbientName { get; init; }

        public bool AllowModding { get; init; }

        public string PoolFolderName { get; init; }

        public IReadOnlyCollection<string> MapSizes { get; init; }

        public IReadOnlyCollection<string> MapSizeIndices { get; init; }

        public bool UsesAllSizeIndices { get; init; }

        public bool HasMapExtension { get; init; }


        private Region(string name, string ambientName, bool allowModding, string poolFolderName, 
            string[] mapSizes, string[] sizeIndices, bool usesAllSizeIndices, bool hasMapExtension)
        {
            Name = name;
            AmbientName = ambientName;
            AllowModding = allowModding;

            PoolFolderName = poolFolderName;
            MapSizes = new ReadOnlyCollection<string>(mapSizes);
            MapSizeIndices = new ReadOnlyCollection<string>(sizeIndices);

            UsesAllSizeIndices = usesAllSizeIndices;
            HasMapExtension = hasMapExtension;
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

        public static Region DetectFromPath(string filePath)
        {
            if (filePath.Contains("colony01"))
                return NewWorld;
            else if (filePath.Contains("dlc03") || filePath.Contains("colony_03"))
                return Arctic;
            else if (filePath.Contains("dlc06") || filePath.Contains("colony02"))
                return Enbesa;
            return Moderate;
        }


        public override string ToString() => Name;
    }
}
