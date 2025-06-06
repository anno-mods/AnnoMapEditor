using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;

namespace AnnoMapEditor.Utilities.UndoRedo
{
    public record IslandPropertiesStackEntry : IUndoRedoStackEntry
    {
        public IslandPropertiesStackEntry(
            IslandElement element, 
            bool? randomizeSlots = null, 
            bool? randomizeFertilities = null,
            bool? randomizeRotation = null,
            IslandType? oldIslandType = null,
            IslandType? newIslandType = null
        ) {
            _element = element;
            _randomizeSlots = randomizeSlots;
            _randomizeFertilities = randomizeFertilities;
            _randomizeRotation = randomizeRotation;
            _oldIslandType = oldIslandType;
            _newIslandType = newIslandType;
        }
        
        private readonly IslandElement _element;
        private readonly bool? _randomizeSlots;
        private readonly bool? _randomizeFertilities;
        private readonly bool? _randomizeRotation;
        private readonly IslandType? _oldIslandType;
        private readonly IslandType? _newIslandType;

        public ActionType ActionType => ActionType.IslandProperties;

        public void Undo()
        {
            if (_element is FixedIslandElement fixedIsland)
            {
                if (_randomizeSlots != null) fixedIsland.RestoreRandomizeSlots(!_randomizeSlots.Value);
                if (_randomizeFertilities != null) fixedIsland.RestoreRandomizeFertilities(!_randomizeFertilities.Value);
                if (_randomizeRotation != null) fixedIsland.RestoreRandomizeRotation(!_randomizeRotation.Value);
            }

            if (_element is IslandElement island)
            {
                if (_oldIslandType != null) island.RestoreIslandType(_oldIslandType);
            }
        }

        public void Redo()
        {
            if (_element is FixedIslandElement fixedIsland)
            {
                if (_randomizeSlots != null) fixedIsland.RestoreRandomizeSlots(_randomizeSlots.Value);
                if (_randomizeFertilities != null) fixedIsland.RestoreRandomizeFertilities(_randomizeFertilities.Value);
                if (_randomizeRotation != null) fixedIsland.RestoreRandomizeRotation(_randomizeRotation.Value);
            }
            
            if (_element is IslandElement island)
            {
                if (_newIslandType != null) island.RestoreIslandType(_newIslandType);
            }
        }
    }
}