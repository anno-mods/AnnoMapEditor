using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;

namespace AnnoMapEditor.MapTemplates.Validation
{
    public class SmallPoolSizeValidator : IMapTemplateValidator
    {
        public MapTemplateValidatorResult Validate(MapTemplate mapTemplate)
        {
            int smallIslandCount = 0;
            int thirdPartyCount = 0;
            int pirateCount = 0;

            foreach (var element in mapTemplate.Elements)
            {
                if (element is RandomIslandElement randomIsland)
                {
                    if (randomIsland.IslandType == IslandType.ThirdParty)
                        ++thirdPartyCount;

                    else if (randomIsland.IslandType == IslandType.PirateIsland)
                        ++pirateCount;

                    else if (randomIsland.IslandSize == IslandSize.Small)
                        ++smallIslandCount;
                }
            }

            // subtract Archibald / Nate / Isabel from the counter
            if ((mapTemplate.Session == SessionAsset.OldWorld || mapTemplate.Session == SessionAsset.NewWorld || mapTemplate.Session == SessionAsset.SunkenTreasures) && thirdPartyCount > 0)
                --smallIslandCount;

            // subtract all but one pirate island from the counter
            if (pirateCount > 0)
                smallIslandCount -= pirateCount - 1;

            int maxPoolSize = Pool.GetPool(mapTemplate.Session.Region, IslandSize.Small).Size;
            if (smallIslandCount <= maxPoolSize)
                return MapTemplateValidatorResult.Ok;
            else
                return new(MapTemplateValidatorStatus.Warning, $"Too many {IslandSize.Small.Name} random islands", $"Only the first {maxPoolSize} will be used.");
        }
    }
}
