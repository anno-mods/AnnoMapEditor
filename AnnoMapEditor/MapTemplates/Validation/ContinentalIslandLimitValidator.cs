using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using System.Linq;

namespace AnnoMapEditor.MapTemplates.Validation
{
    public class ContinentalIslandLimitValidator : ISessionValidator
    {
        public SessionValidatorResult Validate(Session session)
        {
            int continentalIslandCount = session.Elements
                .Where(e => e is FixedIslandElement fixedIsland && fixedIsland.IslandAsset.IslandSize.Contains(IslandSize.Continental))
                .Count();
            
            if (continentalIslandCount <= 1)
                return SessionValidatorResult.Ok;
            else
                return new(SessionValidatorStatus.Error, $"Too many {IslandSize.Continental.Name} islands.", "More than 1 continental island per session may result in visual glitches.");
        }
    }
}
