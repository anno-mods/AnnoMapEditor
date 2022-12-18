using AnnoMapEditor.MapTemplates.Islands;
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
                [IslandSize.Small]  = IslandMapPool.Create("data/sessions/islands/pool/moderate/moderate_s_{0}/moderate_s_{0}.a7m", 12),
                [IslandSize.Medium] = IslandMapPool.Create("data/sessions/islands/pool/moderate/moderate_m_{0}/moderate_m_{0}.a7m", 9),
                [IslandSize.Large]  = IslandMapPool.Create("data/sessions/islands/pool/moderate/moderate_l_{0}/moderate_l_{0}.a7m", 14)
            });
        public static readonly Region NewWorld = new("NewWorld", "New World", "south_america_caribic_01",
            new()
            {
                [IslandSize.Small]  = IslandMapPool.Create("data/sessions/islands/pool/colony01/colony01_s_{0}/colony01_s_{0}.a7m", 4),
                [IslandSize.Medium] = IslandMapPool.Create("data/sessions/islands/pool/colony01/colony01_m_{0}/colony01_m_{0}.a7m", 6),
                [IslandSize.Large]  = IslandMapPool.Create("data/sessions/islands/pool/colony01/colony01_l_{0}/colony01_l_{0}.a7m", 5)
            });
        public static readonly Region Arctic = new("Arctic", "Arctic", "DLC03_01",
            new()
            {
                [IslandSize.Small]  = IslandMapPool.Create("data/dlc03/sessions/islands/pool/colony03_a01_{0}/colony03_a01_{0}.a7m", 8),
                [IslandSize.Medium] = IslandMapPool.Create("data/dlc03/sessions/islands/pool/colony03_a02_{0}/colony03_a02_{0}.a7m", 4),
                [IslandSize.Large]  = IslandMapPool.Create("data/dlc03/sessions/islands/pool/moderate/moderate_l_{0}/moderate_l_{0}.a7m", 14)
            });
        public static readonly Region Enbesa = new("Enbesa", "Enbesa", "Colony_02",
            new()
            {
                [IslandSize.Small]  = IslandMapPool.Create("data/dlc06/sessions/islands/pool/colony02_s_{0}/colony02_s_{0}.a7m", new int[] { 1, 2, 3, 5 }),
                [IslandSize.Medium] = IslandMapPool.Create("data/dlc06/sessions/islands/pool/colony02_m_{0}/colony02_m_{0}.a7m", new int[] { 2, 4, 5, 9 }),
                [IslandSize.Large]  = IslandMapPool.Create("data/dlc06/sessions/islands/pool/colony02_l_{0}/colony02_l_{0}.a7m", new int[] { 1, 3, 5, 6 })
            });
        public static readonly Region[] All = new Region[] { Moderate, NewWorld, Arctic, Enbesa };
        #endregion


        public string Name { get; init; }
        public string AmbientName { get; init; }

        private readonly string value;

        public Dictionary<IslandSize, IslandMapPool> IslandMapPools { get; private init; }


        private Region(string type, string name, string ambientName, Dictionary<IslandSize, IslandMapPool> islandMapPools)
        {
            value = type;
            Name = name;
            AmbientName = ambientName;
            IslandMapPools = islandMapPools;
        }

        public override string ToString() => value;

        public override bool Equals(object? obj) => obj is Region other && value.Equals(other.value);
        public override int GetHashCode() => value.GetHashCode();

        public static bool operator !=(Region a, Region b) => !a.Equals(b);
        public static bool operator ==(Region a, Region b) => a.Equals(b);
    }
}
