using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;

namespace AnnoMapEditor.MapTemplates.Validation
{
    public class SmallPoolSizeValidator : ISessionValidator
    {
        public SessionValidatorResult Validate(Session session)
        {
            int smallIslandCount = 0;
            int thirdPartyCount = 0;
            int pirateCount = 0;

            foreach (var element in session.Elements)
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
            if ((session.Region == Region.Moderate || session.Region == Region.NewWorld) && thirdPartyCount > 0)
                --smallIslandCount;

            // subtract all but one pirate island from the counter
            if (pirateCount > 0)
                smallIslandCount -= pirateCount - 1;

            int maxPoolSize = Pool.GetPool(session.Region, IslandSize.Small).Size;
            if (smallIslandCount <= maxPoolSize)
                return SessionValidatorResult.Ok;
            else
                return new(SessionValidatorStatus.Warning, $"Too many {IslandSize.Small.Name} random islands", $"Only the first {maxPoolSize} will be used.");
        }
    }
}
