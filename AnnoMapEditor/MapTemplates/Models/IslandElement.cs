using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.Utilities;
using AnnoMapEditor.Utilities.UndoRedo;
using IslandType = AnnoMapEditor.MapTemplates.Enums.IslandType;

namespace AnnoMapEditor.MapTemplates.Models
{
    public abstract class IslandElement : MapElement
    {
        public string? Label
        { 
            get => _label;
            set {
                if (value == "")
                    value = null;

                SetProperty(ref _label, value);
            }
        }
        private string? _label;

        public IslandType IslandType
        {
            get => _islandType;
            set
            {
                if (this is RandomIslandElement randomIsland)
                {
                    UndoRedoStack.Instance.Do(new IslandPropertiesStackEntry(randomIsland, oldIslandType: _islandType, newIslandType: value));
                }
                else
                {
                    UndoRedoStack.Instance.Do(new IslandPropertiesStackEntry(this, oldIslandType: _islandType, newIslandType: value));
                }
                SetProperty(ref _islandType, value);
            } 
        }
        private IslandType _islandType;
        public void RestoreIslandType(IslandType islandType)
        {
            SetProperty(ref _islandType, islandType, propertyName: nameof(IslandType));
        }

        public IslandDifficulty? IslandDifficulty
        {
            get => _islandDifficulty;
            set => SetProperty(ref _islandDifficulty, value);
        }
        private IslandDifficulty? _islandDifficulty;

        public int SizeInTiles
        {
            get => _sizeInTiles;
            protected set => SetProperty(ref _sizeInTiles, value);
        }
        private int _sizeInTiles;


        public IslandElement(IslandType islandType)
        {
            _islandType = islandType;
        }


        // ---- Serialization ----

        public IslandElement(Element sourceTemplate)
            : base(sourceTemplate)
        {
            _label            = sourceTemplate.IslandLabel != null ? (string)sourceTemplate.IslandLabel : null;
            _islandType       = IslandType.FromElementValue(sourceTemplate.Config?.Type?.id ?? sourceTemplate.RandomIslandConfig?.value?.Type?.id);
            _islandDifficulty = IslandDifficulty.FromElementValue(sourceTemplate.Config?.Difficulty?.id ?? sourceTemplate.RandomIslandConfig?.value?.Difficulty?.id);
        }

        protected override void ToTemplate(Element resultElement)
        {
            if (_label != null)
                resultElement.IslandLabel = _label;
        }
    }
}
