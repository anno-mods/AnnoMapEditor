using Anno.FileDBModels.Anno1800.MapTemplate;

namespace AnnoMapEditor.MapTemplates.Models
{
    public class RandomIslandElement : IslandElement
    {
        public IslandSize IslandSize
        {
            get => _islandSize;
            set => SetProperty(ref _islandSize, value);
        }
        private IslandSize _islandSize;


        public RandomIslandElement(IslandSize islandSize, IslandType islandType)
            : base(islandType)
        {
            _islandSize = islandSize;
        }


        // ---- Serialization ----

        public RandomIslandElement(Element sourceElement)
            : base(sourceElement)
        {
            _islandSize = IslandSize.FromElementValue(sourceElement.Size);
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
