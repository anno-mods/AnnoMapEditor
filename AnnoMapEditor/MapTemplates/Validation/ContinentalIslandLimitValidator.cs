using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using System.Linq;

namespace AnnoMapEditor.MapTemplates.Validation
{
    public class ContinentalIslandLimitValidator : IMapTemplateValidator
    {
        public MapTemplateValidatorResult Validate(MapTemplate mapTemplate)
        {
            int continentalIslandCount = mapTemplate.Elements
                .Where(e => e is FixedIslandElement fixedIsland && fixedIsland.IslandAsset.IslandSize.Contains(IslandSize.Continental))
                .Count();
            
            if (continentalIslandCount <= 1)
                return MapTemplateValidatorResult.Ok;
            else
                return new(MapTemplateValidatorStatus.Error, $"Too many {IslandSize.Continental.Name} islands.", "More than 1 continental island per session may result in visual glitches.");
        }
    }
}
