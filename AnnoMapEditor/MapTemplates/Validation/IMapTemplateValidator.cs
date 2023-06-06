using AnnoMapEditor.MapTemplates.Models;

namespace AnnoMapEditor.MapTemplates.Validation
{
    public interface IMapTemplateValidator
    {
        public MapTemplateValidatorResult Validate(MapTemplate mapTemplate);
    }
}
