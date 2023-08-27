using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;

namespace AnnoMapEditor.MapTemplates.Validation
{
    public class PoolSizeValidator : IMapTemplateValidator
    {
        private readonly IslandSize _islandSize;


        public PoolSizeValidator(IslandSize islandSize)
        {
            _islandSize = islandSize;
        }


        public MapTemplateValidatorResult Validate(MapTemplate mapTemplate)
        {
            int islandCount = 0;

            foreach (var element in mapTemplate.Elements)
            {
                if (element is RandomIslandElement randomIsland && randomIsland.IslandSize == _islandSize)
                {
                    ++islandCount;
                }
            }

            int maxPoolSize = Pool.GetPool(mapTemplate.Session.Region, _islandSize).Size;
            if (islandCount <= maxPoolSize)
                return MapTemplateValidatorResult.Ok;
            else
                return new(MapTemplateValidatorStatus.Warning, $"Too many {_islandSize.Name} random islands", $"Only the first {maxPoolSize} will be used.");
        }
    }
}
