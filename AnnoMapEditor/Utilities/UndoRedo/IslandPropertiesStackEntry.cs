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
            IslandType? newIslandType = null,
            IslandSize? oldIslandSize = null,
            IslandSize? newIslandSize = null
        ) {
            _element = element;
            _randomizeSlots = randomizeSlots;
            _randomizeFertilities = randomizeFertilities;
            _randomizeRotation = randomizeRotation;
            _oldIslandType = oldIslandType;
            _newIslandType = newIslandType;
            _oldIslandSize = oldIslandSize;
            _newIslandSize = newIslandSize;
        }
        
        private readonly IslandElement _element;
        private readonly bool? _randomizeSlots;
        private readonly bool? _randomizeFertilities;
        private readonly bool? _randomizeRotation;
        private readonly IslandType? _oldIslandType;
        private readonly IslandType? _newIslandType;
        private readonly IslandSize? _oldIslandSize;
        private readonly IslandSize? _newIslandSize;

        public ActionType ActionType => ActionType.IslandProperties;

        public void Undo()
        {
            if (_element is FixedIslandElement fixedIsland)
            {
                if (_randomizeSlots != null) fixedIsland.RandomizeSlots = !_randomizeSlots.Value;
                if (_randomizeFertilities != null) fixedIsland.RandomizeFertilities = !_randomizeFertilities.Value;
                if (_randomizeRotation != null) fixedIsland.RandomizeRotation = !_randomizeRotation.Value;
            } 
            else if (_element is RandomIslandElement randomIsland)
            {
                if (_oldIslandSize != null) randomIsland.IslandSize = _oldIslandSize;    
            }
            if (_oldIslandType != null) _element.IslandType = _oldIslandType;
        }

        public void Redo()
        {
            if (_element is FixedIslandElement fixedIsland)
            {
                if (_randomizeSlots != null) fixedIsland.RandomizeSlots = _randomizeSlots.Value;
                if (_randomizeFertilities != null) fixedIsland.RandomizeFertilities = _randomizeFertilities.Value;
                if (_randomizeRotation != null) fixedIsland.RandomizeRotation = _randomizeRotation.Value;
            }
            else if (_element is RandomIslandElement randomIsland)
            {
                if (_newIslandSize != null) randomIsland.IslandSize = _newIslandSize;    
            }
            if (_newIslandType != null) _element.IslandType = _newIslandType;
        }
    }
}