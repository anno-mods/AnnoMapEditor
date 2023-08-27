using Anno.FileDBModels.Anno1800.MapTemplate;

namespace AnnoMapEditor.MapTemplates.Models
{
    public class StartingSpotElement : MapElement 
    {
        // TODO: The StartingSpot's index is implicit based on its position in mapTemplate.Elements. It
        //       must be set from the outside.
        public int Index { get; set; } = -1;


        public StartingSpotElement()
        {
        }


        // ---- Serialization ----

        public StartingSpotElement(Element sourceTemplate)
            : base(sourceTemplate)
        {
            
        }

        protected override void ToTemplate(Element resultElement)
        {
        }
    }
}
