using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;

namespace AnnoMapEditor.MapTemplates.Validation
{
    public class PoolSizeValidator : ISessionValidator
    {
        private readonly IslandSize _islandSize;


        public PoolSizeValidator(IslandSize islandSize)
        {
            _islandSize = islandSize;
        }


        public SessionValidatorResult Validate(Session session)
        {
            int islandCount = 0;

            foreach (var element in session.Elements)
            {
                if (element is RandomIslandElement randomIsland && randomIsland.IslandSize == _islandSize)
                {
                    ++islandCount;
                }
            }

            int maxPoolSize = Pool.GetPool(session.Region, _islandSize).Size;
            if (islandCount <= maxPoolSize)
                return SessionValidatorResult.Ok;
            else
                return new(SessionValidatorStatus.Warning, $"Too many {_islandSize.Name} random islands", $"Only the first {maxPoolSize} will be used.");
        }
    }
}
