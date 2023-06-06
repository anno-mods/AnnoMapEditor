using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.MapTemplates.Enums;
using IslandType = AnnoMapEditor.MapTemplates.Enums.IslandType;

namespace AnnoMapEditor.MapTemplates.Models
{
    public class RandomIslandElement : IslandElement
    {
        public IslandSize IslandSize
        {
            get => _islandSize;
            set
            {
                SetProperty(ref _islandSize, value);
                SizeInTiles = _islandSize.DefaultSizeInTiles;
            }
        }
        private IslandSize _islandSize;


        public RandomIslandElement(IslandSize islandSize, IslandType islandType)
            : base(islandType)
        {
            IslandSize = islandSize;
        }


        // ---- Serialization ----

        public RandomIslandElement(Element sourceElement)
            : base(sourceElement)
        {
            IslandSize = IslandSize.FromElementValue(sourceElement.Size);
        }

        protected override void ToTemplate(Element resultElement)
        {
            base.ToTemplate(resultElement);

            resultElement.Size = IslandSize.ElementValue;
            resultElement.Difficulty = new();
            resultElement.Config = new()
            {
                Type = new() { id = IslandType.ElementValue },
                Difficulty = new()
            };
        }
    }
}
