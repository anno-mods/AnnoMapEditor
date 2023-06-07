using AnnoMapEditor.DataArchives.Assets.Deserialization;
using AnnoMapEditor.MapTemplates.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate(TEMPLATE_NAME)]
    public class RandomIslandAsset : StandardAsset
    {
        public const string TEMPLATE_NAME = "RandomIsland";


        public string FilePath { get; init; }

        public Region? IslandRegion { get; init; }

        public IEnumerable<IslandDifficulty> IslandDifficulty { get; init; }

        public IEnumerable<IslandType> IslandType { get; init; }


        public RandomIslandAsset(XElement valuesXml)
            : base(valuesXml)
        {
            XElement randomIslandValues = valuesXml.Element(TEMPLATE_NAME)
                ?? throw new Exception($"XML is not a valid {nameof(RandomIslandAsset)}. It does not have '{TEMPLATE_NAME}' section in its values.");

            string? regionStr = randomIslandValues.Element(nameof(IslandRegion))?.Value;
            if (regionStr != null)
                IslandRegion = RegionFromName(regionStr);

            FilePath = randomIslandValues.Element(nameof(FilePath))?.Value
                ?? throw new Exception($"XML is not a valid {nameof(RandomIslandAsset)}. Required attribute '{nameof(FilePath)}' not found.");

            // IslandDifficulty
            string? islandDifficultyStr = randomIslandValues.Element(nameof(IslandDifficulty))?.Value;
            if (islandDifficultyStr != null)
            {
                IslandDifficulty = islandDifficultyStr.Split(';')
                    .Select(s => MapTemplates.Enums.IslandDifficulty.FromName(s))
                    .ToList();
            }
            else
                IslandDifficulty = Enumerable.Empty<IslandDifficulty>();

            // IslandType
            string? islandTypeStr = randomIslandValues.Element(nameof(IslandType))?.Value;
            if (islandTypeStr != null)
            {
                IslandType = islandTypeStr.Split(';')
                    .Select(s => MapTemplates.Enums.IslandType.FromName(s))
                    .ToList();
            }
            else
                IslandType = Enumerable.Empty<IslandType>();
        }


        private static Region RegionFromName(string name)
        {
            return name switch
            {
                "Moderate" => Region.Moderate,
                "Colony01" => Region.NewWorld,
                "Arctic" => Region.Arctic,
                "Africa" => Region.Enbesa,
                _ => throw new NotImplementedException()
            };
        }
    }
}
